using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ImgLib.Models;
using System.IO;

namespace ImgLib.UI.ViewModels;

public sealed partial class WatermarkDesignViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    public partial Bitmap? PreviewImageSource { get; private set; }

    //[ObservableProperty]
    //public partial ImageGenerateOption ImageGenerateOption { get; private set; } = new ImageGenerateOption(0.89f);

    [ObservableProperty]
    public partial string? PreviewFilePath { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial WatermarkSettingsViewModel WatermarkSettingsViewModel { get; private set; } = new();

    private ImageFile? _previewImageFile;

    [ObservableProperty]
    public partial bool PreviewFilePathRequested { get; private set; } = false;

    partial void OnPreviewFilePathChanged(string? value)
    {
        if (string.IsNullOrEmpty(PreviewFilePath))
            return;

        _previewImageFile = ImageFile.GetImageFile(value!);

        PreviewImageSource = new Bitmap
            (
            _previewImageFile.GetSourceStream()
//@"C:\Users\Administrator\Desktop\后期临时\DSC_337020240714000102.JPG"
//@"C:\Users\Administrator\Desktop\后期临时\DSC_1901.JPG"
);
        PreviewFilePathRequested = !PreviewFilePathRequested;
    }

    [RelayCommand]
    public async Task SetBackground()
    {
        if (PreviewImageSource == null || string.IsNullOrWhiteSpace(PreviewFilePath))
            return;

        if (_previewImageFile == null)
            return;

        NikonExifInfo exif = new NikonExifInfo(_previewImageFile.Path);

        WatermarkSettingsViewModel.ExifInfo = exif;
        WatermarkSettingsViewModel.BuildExifInfoTree();

        Console.WriteLine(exif.ToJson());

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