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
    public partial WatermarkSettingsViewModel WatermarkSettingsViewModel { get; private set; } = new();

    [ObservableProperty]
    public partial HistogramViewModel HistogramViewModel { get; private set; } = new();

    private ImageFile? _previewImageFile;

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
        if (PreviewImageSource == null || string.IsNullOrWhiteSpace(PreviewFilePath))
            return;

        if (_previewImageFile == null)
            return;

        using MemoryStream output = new();

        ImageService.GenerateWithOptions(_previewImageFile.GetSourceStream(), output, WatermarkSettingsViewModel.ImageGenerateOption);

        output.Seek(0, SeekOrigin.Begin);
        if (output != null)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                PreviewImageSource = new Bitmap(output);
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