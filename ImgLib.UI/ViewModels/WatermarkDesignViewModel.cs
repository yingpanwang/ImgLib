using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ImgLib.Models;
using System.IO;
using System.Windows.Input;

namespace ImgLib.UI.ViewModels;

public sealed partial class WatermarkDesignViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    public partial Bitmap? PreviewImageSource { get; private set; }

    [ObservableProperty]
    public partial double PreviewAngle { get; private set; }

    public ICommand RotateLeftCommand => new RelayCommand(
            () => PreviewAngle -= 90
        );

    public ICommand RotateRightCommand => new RelayCommand(
            () => PreviewAngle += 90
        );

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

    public ToastViewModel? ToastViewModel { get; set; }

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
                await Task.Delay(300, _previewCancellationTokenSource.Token);
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

        PreviewImageSource = new Bitmap(_previewImageFile.GetSourceStream());

        // 重用 ImageFile 已创建的 ExifInfo，包裹为 NikonExifInfo（record 拷贝构造器，零 I/O）
        var exifInfo = _previewImageFile.Exif is NikonExifInfo nef
            ? nef
            : new NikonExifInfo(_previewImageFile.Exif!);

        WatermarkSettingsViewModel.ExifInfo = exifInfo;

        // 将图片路径传递给直方图 ViewModel 以计算直方图
        HistogramViewModel.ImageFilePath = value;

        // 在后台线程预热 Lazy<Metadata>，这样 UI 绑定后续访问属性时不会阻塞
        _ = exifInfo.EnsureMetadataLoadedAsync();
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

        using MemoryStream output = new();

        ImageService.GenerateWithOptions(_previewImageFile.GetSourceStream(), output, WatermarkSettingsViewModel.Settings.ToImageGenerateOption());

        output.Seek(0, SeekOrigin.Begin);
        if (output != null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                PreviewImageSource = new Bitmap(output);
                System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] 预览图像已更新");

                // 显示 Toast 通知
                System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] ToastViewModel = {ToastViewModel != null}");
                ToastViewModel?.ShowMessage("预览已更新", ToastType.Success);
                System.Diagnostics.Debug.WriteLine($"[WatermarkDesignViewModel] Toast 消息已添加，数量 = {ToastViewModel?.Messages.Count}");
            });
        }
    }

    public async Task Left()
    {
    }

    public Task Load()
    {
        return Task.CompletedTask;
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