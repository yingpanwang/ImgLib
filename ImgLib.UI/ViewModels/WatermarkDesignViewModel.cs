using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ImgLib.Models;
using ImgLib.UI.Models;
using ImgLib.UI.Services;
using ImgLib.UI.Views;
using ImgLib.WatermarkPipeline;
using SkiaSharp;
using System.Diagnostics;
using System.IO;
using System.Threading;
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

    /// <summary>
    /// 预览设置视图模型
    /// </summary>
    [ObservableProperty]
    public partial PreviewSettingsViewModel PreviewSettingsViewModel { get; set; }

    /// <summary>
    /// 设置面板是否展开（收起/展开）
    /// </summary>
    [ObservableProperty]
    public partial bool IsSettingsPanelExpanded { get; set; }

    /// <summary>预览时是否显示直方图（与 <see cref="PreviewSettingsViewModel.ShowHistogram"/> 同步）</summary>
    // [ObservableProperty]
    // public partial bool ShowHistogram { get; set; }

    /// <summary>直方图 X 方向平移偏移量（用于拖拽定位）</summary>
    [ObservableProperty]
    public partial double HistogramTranslateX { get; set; }

    /// <summary>直方图 Y 方向平移偏移量（用于拖拽定位）</summary>
    [ObservableProperty]
    public partial double HistogramTranslateY { get; set; }

    /// <summary>直方图是否正在被拖拽</summary>
    [ObservableProperty]
    public partial bool IsDraggingHistogram { get; set; }

    private ImageFile? _previewImageFile;
    private CancellationTokenSource? _previewCancellationTokenSource;

    /// <summary>
    /// 水印设置文件列表 ViewModel
    /// </summary>
    public WatermarkSettingListViewModel WatermarkSettingListViewModel { get; }

    public WatermarkDesignViewModel(
        WatermarkSettingsViewModel watermarkSettingsViewModel,
        HistogramViewModel histogramViewModel,
        PreviewSettingsViewModel previewSettingsViewModel,
        WatermarkSettingListViewModel watermarkSettingListViewModel)
    {
        WatermarkSettingsViewModel = watermarkSettingsViewModel;
        HistogramViewModel = histogramViewModel;
        IsSettingsPanelExpanded = true;

        PreviewSettingsViewModel = previewSettingsViewModel;
        WatermarkSettingListViewModel = watermarkSettingListViewModel;

        // 监听文件列表的加载请求
        WatermarkSettingListViewModel.LoadRequested += OnLoadRequested;
    }

    // private void OnPreviewSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    // {
    //     switch (e.PropertyName)
    //     {
    //         case nameof(PreviewSettingsViewModel.ShowHistogram):
    //             ShowHistogram = PreviewSettingsViewModel.ShowHistogram;
    //             break;
    //     }
    // }

    [RelayCommand]
    private void ToggleSettingsPanel()
    {
        IsSettingsPanelExpanded = !IsSettingsPanelExpanded;
    }

    private WatermarkSettingsViewModel? _previousWatermarkSettingsViewModel;

    partial void OnWatermarkSettingsViewModelChanged(WatermarkSettingsViewModel value)
    {
        // 取消旧 VM 的事件订阅
        if (_previousWatermarkSettingsViewModel != null)
        {
            _previousWatermarkSettingsViewModel.PreviewRequested -= OnPreviewRequested;
            _previousWatermarkSettingsViewModel.SettingsFileSaved -= OnSettingsFileSaved;
        }

        _previousWatermarkSettingsViewModel = value;

        // 注入预览命令
        value.PreviewCommand = SetBackgroundCommand;
        System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] 注入 PreviewCommand: {SetBackgroundCommand != null}, CanExecute: {SetBackgroundCommand?.CanExecute(null)}");

        // 监听自动预览事件
        value.PreviewRequested += OnPreviewRequested;

        // 监听保存成功事件 → 更新文件列表
        value.SettingsFileSaved += OnSettingsFileSaved;

        // 注入文件列表 VM（供 WatermarkSettingsView 内部绑定）
        value.WatermarkSettingListViewModel = WatermarkSettingListViewModel;
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
                var intervalMs = PreviewSettingsViewModel.AutoPreviewIntervalMs;

                // 确保间隔不小于 50ms，避免过于频繁的预览刷新
                if (intervalMs < 50)
                    intervalMs = 50;

                await Task.Delay(intervalMs, _previewCancellationTokenSource.Token);

                if (!_previewCancellationTokenSource.Token.IsCancellationRequested)
                {
                    Debug.WriteLine($"[WatermarkDesignViewModel] 执行 SetBackground");
                    await SetBackgroundWithPipeline();
                }
            }
            catch (TaskCanceledException)
            {
                // 预览被取消，忽略
                Debug.WriteLine($"预览取消:{PreviewFilePath}");
            }
        }, _previewCancellationTokenSource.Token);
    }

    /// <summary>水印设置保存成功后，将文件路径添加到列表</summary>
    private void OnSettingsFileSaved(object? sender, string filePath)
    {
        WatermarkSettingListViewModel.AddSavedFile(filePath);
    }

    /// <summary>用户从文件列表中选择加载某个设置文件</summary>
    private void OnLoadRequested(object? sender, WatermarkSettings settings)
    {
        WatermarkSettingsViewModel.Settings = settings;
        ToastService.ShowSuccess("水印设置已加载");
    }

    partial void OnPreviewFilePathChanged(string? value)
    {
        if (string.IsNullOrEmpty(PreviewFilePath))
            return;

        // 取消上一次预览加载，防止快速切换时旧图覆盖新图
        _previewCancellationTokenSource?.Cancel();
        _previewCancellationTokenSource = new System.Threading.CancellationTokenSource();
        var ct = _previewCancellationTokenSource.Token;

        // 切换图片时重置旋转角度（轻量操作，可在 UI 线程）
        PreviewAngle = 0;

        // 异步加载预览图，避免全分辨率解码阻塞 UI 线程
        _ = LoadPreviewAsync(value!, ct);
    }

    /// <summary>
    /// 后台加载预览图片：ImageFile 创建 + Bitmap 解码均在后台线程，
    /// 仅属性赋值回到 UI 线程，消除选中切换时的卡顿。
    /// </summary>
    private async Task LoadPreviewAsync(string filePath, System.Threading.CancellationToken ct)
    {
        IsLoading = true;

        try
        {
            // Phase 1: 后台线程执行文件 I/O + 全分辨率解码
            var (imageFile, bitmap) = await Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();

                var imgFile = ImageFile.GetImageFile(filePath);
                var bmp = new Bitmap(imgFile.GetSourceStream());

                ct.ThrowIfCancellationRequested();
                return (imgFile, bmp);
            }, ct);

            // Phase 2: UI 线程更新绑定属性
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ct.IsCancellationRequested)
                    return;

                _previewImageFile = imageFile;

                PreviewImageSource?.Dispose();
                PreviewImageSource = bitmap;

                // 更新图片基础信息面板
                WatermarkSettingsViewModel.ImageInfo.UpdateFromFile(filePath);

                // 重用 ImageFile 已创建的 ExifInfo
                var exifInfo = _previewImageFile.Exif is NikonExifInfo nef
                    ? nef
                    : new NikonExifInfo(_previewImageFile.Exif!);

                WatermarkSettingsViewModel.ExifInfo = exifInfo;

                // 将图片路径传递给直方图 ViewModel 以计算直方图
                HistogramViewModel.ImageFilePath = filePath;

                // 在后台线程预热 Lazy<Metadata>
                _ = exifInfo.EnsureMetadataLoadedAsync();

                IsLoading = false;

                // 如果开启了自动预览，切换图片时自动重新生成预览
                if (PreviewSettingsViewModel.AutoPreview)
                {
                    OnPreviewRequested(this, EventArgs.Empty);
                }
            });
        }
        catch (OperationCanceledException)
        {
            // 被新的选择取消，静默处理
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LoadPreviewAsync] 加载预览失败: {filePath}, {ex.Message}");
            await Dispatcher.UIThread.InvokeAsync(() => IsLoading = false);
        }
    }

    [Obsolete("使用管道方法")]
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

        using MemoryStream output = new();

        // 传递预览标志和降采样参数
        var options = WatermarkSettingsViewModel.Settings.ToImageGenerateOption();

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

        // ── 管线方式 ──
        using var inputStream = _previewImageFile.GetSourceStream();
        using var original = SKBitmap.Decode(inputStream);
        if (original == null)
            throw new InvalidOperationException("无法解码图像");

        // 预览降采样
        using var workingBitmap = ApplyPreviewDownsampling(original, options);

        if (workingBitmap == null)
        {
            ToastService.ShowWarning("获取降采样预览图失败!");
            return;
        }

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
        //PreviewSettingsViewModel.PropertyChanged -= OnPreviewSettingsChanged;
        Reset();
    }
}