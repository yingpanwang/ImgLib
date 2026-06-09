using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ImgLib.Models;
using ImgLib.UI.Services;
using ImgLib.WatermarkPipeline;
using SkiaSharp;
using System.IO;
using System.Windows.Input;

namespace ImgLib.UI.ViewModels;

public sealed partial class WatermarkDesignViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    public partial Bitmap? PreviewImageSource { get; private set; }

    [ObservableProperty]
    public partial double PreviewAngle { get; private set; }

    //[ObservableProperty]
    //public partial ImageGenerateOption ImageGenerateOption { get; private set; } = new ImageGenerateOption(0.89f);

    [ObservableProperty]
    public partial string? PreviewFilePath { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial WatermarkSettingsViewModel WatermarkSettingsViewModel { get; private set; }

    [ObservableProperty]
    public partial HistogramViewModel HistogramViewModel { get; private set; }

    private ImageFile? _previewImageFile;
    private System.Threading.CancellationTokenSource? _previewCancellationTokenSource;

    public WatermarkDesignViewModel()
    {
        WatermarkSettingsViewModel = new();
        HistogramViewModel = new();
    }

    partial void OnWatermarkSettingsViewModelChanged(WatermarkSettingsViewModel value)
    {
        // 注入预览命令
        value.PreviewCommand = SetBackgroundCommand;
        System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] 注入 PreviewCommand: {SetBackgroundCommand != null}, CanExecute: {SetBackgroundCommand?.CanExecute(null)}");

        // 监听自动预览事件
        value.PreviewRequested += OnPreviewRequested;
    }

    private void OnPreviewRequested(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] 收到 PreviewRequested 事件");

        // 取消之前的预览任务
        _previewCancellationTokenSource?.Cancel();
        _previewCancellationTokenSource = new System.Threading.CancellationTokenSource();

        // 延迟触发预览（防抖）
        Task.Run(async () =>
        {
            try
            {
                var intervalMs = WatermarkSettingsViewModel.AutoPreviewIntervalMs;
                // 确保间隔不小于 50ms，避免过于频繁的预览刷新
                if (intervalMs < 50) intervalMs = 50;
                await Task.Delay(intervalMs, _previewCancellationTokenSource.Token);
                if (!_previewCancellationTokenSource.Token.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] 执行 SetBackground");
                    await SetBackground();
                }
            }
            catch (TaskCanceledException)
            {
                // 预览被取消，忽略
            }
        }, _previewCancellationTokenSource.Token);
    }

    partial void OnPreviewFilePathChanged(string? value)
    {
        if (string.IsNullOrEmpty(PreviewFilePath))
            return;

        _previewImageFile = ImageFile.GetImageFile(value!);

        // 切换图片时重置旋转角度
        PreviewAngle = 0;

        PreviewImageSource = new Bitmap(_previewImageFile.GetSourceStream());

        // 更新图片基础信息面板
        WatermarkSettingsViewModel.ImageInfo.UpdateFromFile(value!);

        // 重用 ImageFile 已创建的 ExifInfo，包裹为 NikonExifInfo（record 拷贝构造器，零 I/O）
        var exifInfo = _previewImageFile.Exif is NikonExifInfo nef
            ? nef
            : new NikonExifInfo(_previewImageFile.Exif!);

        WatermarkSettingsViewModel.ExifInfo = exifInfo;

        // 将图片路径传递给直方图 ViewModel 以计算直方图
        HistogramViewModel.ImageFilePath = value;

        // 在后台线程预热 Lazy<Metadata>，这样 UI 绑定后续访问属性时不会阻塞
        _ = exifInfo.EnsureMetadataLoadedAsync();

        // 如果开启了自动预览，切换图片时自动重新生成预览
        if (WatermarkSettingsViewModel.AutoPreview)
        {
            OnPreviewRequested(this, EventArgs.Empty);
        }
    }

    [RelayCommand]
    public async Task SetBackground()
    {
        System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] SetBackground 开始: PreviewImageSource={PreviewImageSource != null}, PreviewFilePath={PreviewFilePath}");

        if (PreviewImageSource == null || string.IsNullOrWhiteSpace(PreviewFilePath))
        {
            System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] SetBackground 提前返回: 预览条件不满足");
            return;
        }

        if (_previewImageFile == null)
        {
            System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] SetBackground 提前返回: _previewImageFile 为空");
            return;
        }
        await SetBackgroundWithPipeline();
        return;

        using MemoryStream output = new();

        // 传递预览标志和降采样参数
        var options = WatermarkSettingsViewModel.Settings.ToImageGenerateOption();
        options.EnablePreviewDownsampling = WatermarkSettingsViewModel.EnablePreviewDownsampling;
        options.UsePreviewPercentMode = WatermarkSettingsViewModel.UsePreviewPercentMode;
        options.PreviewMaxDimension = WatermarkSettingsViewModel.PreviewMaxDimension;
        options.PreviewMaxPercent = WatermarkSettingsViewModel.PreviewMaxPercent;

        ImageService.GenerateWithOptions(
            _previewImageFile.GetSourceStream(),
            output,
            options,
            WatermarkSettingsViewModel.ExifInfo,
            isPreview: true);

        output.Seek(0, SeekOrigin.Begin);
        if (output.Length > 0)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                PreviewImageSource?.Dispose();
                PreviewImageSource = new Bitmap(output);
                System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] 预览图像已更新");

                // 显示 Toast 通知
                ToastService.ShowSuccess("预览已更新");
            });
        }
    }

    /// <summary>
    /// 使用 WatermarkPipeline（访客模式）生成水印预览。
    /// 与 <see cref="SetBackground"/> 功能等价，但通过管线命令 + 访客渲染器实现。
    /// </summary>
    public async Task SetBackgroundWithPipeline()
    {
        if (PreviewImageSource == null || string.IsNullOrWhiteSpace(PreviewFilePath))
            return;

        if (_previewImageFile == null)
            return;

        using MemoryStream output = new();

        var options = WatermarkSettingsViewModel.Settings.ToImageGenerateOption();
        options.EnablePreviewDownsampling = WatermarkSettingsViewModel.EnablePreviewDownsampling;
        options.UsePreviewPercentMode = WatermarkSettingsViewModel.UsePreviewPercentMode;
        options.PreviewMaxDimension = WatermarkSettingsViewModel.PreviewMaxDimension;
        options.PreviewMaxPercent = WatermarkSettingsViewModel.PreviewMaxPercent;

        // ── 管线方式 ──
        using var inputStream = _previewImageFile.GetSourceStream();
        using var original = SKBitmap.Decode(inputStream);
        if (original == null)
            throw new InvalidOperationException("无法解码图像");

        // 预览降采样
        using var workingBitmap = ApplyPreviewDownsampling(original, options);

        int w = workingBitmap.Width;
        int h = workingBitmap.Height;

        using var surface = SKSurface.Create(new SKImageInfo(w, h));
        var canvas = surface.Canvas;

        // 构建渲染上下文
        var ctx = new WatermarkRenderContext(
            canvas, original, workingBitmap, w, h,
            options.Scale,
            WatermarkSettingsViewModel.ExifInfo);

        // 从 ImageGenerateOption 构建管线 → 执行
        var pipeline = WatermarkPipelineRunner.FromOptions(options);

        
        pipeline.Execute(ctx);

        // 编码输出
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        data.SaveTo(output);

        output.Seek(0, SeekOrigin.Begin);
        if (output.Length > 0)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                PreviewImageSource?.Dispose();
                PreviewImageSource = new Bitmap(output);
            });
        }
    }

    /// <summary>预览降采样（从原始 ImageService 逻辑提取）</summary>
    private static SKBitmap ApplyPreviewDownsampling(SKBitmap original, ImageGenerateOption options)
    {
        if (!options.EnablePreviewDownsampling)
            return original;

        bool isPercent = options.UsePreviewPercentMode;

        return ImageService.DownsampleImage
               (
                   original,
                   value: isPercent ? options.PreviewMaxPercent : options.PreviewMaxDimension,
                   isPercent
               );

        // int maxDimension = Math.Max(original.Width, original.Height);
        // int targetDimension = options.UsePreviewPercentMode
        //     ? (int)(maxDimension * options.PreviewMaxPercent / 100f)
        //     : options.PreviewMaxDimension;

        // if (maxDimension <= targetDimension)
        //     return original;

        // float scale = (float)targetDimension / maxDimension;
        // int rw = (int)(original.Width * scale);
        // int rh = (int)(original.Height * scale);

        // return original.Resize(
        //     new SKImageInfo(rw, rh),
        //     new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
    }

    public void Reset()
    {
        PreviewImageSource?.Dispose();
    }

    public void Dispose()
    {
        Reset();
    }
}