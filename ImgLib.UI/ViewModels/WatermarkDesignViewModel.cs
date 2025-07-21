
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ImgLib.Models;
using System.IO;

namespace ImgLib.UI.ViewModels;

public sealed partial class WatermarkDesignViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    public Bitmap? _previewImageSource;

    [ObservableProperty]
    private ImageGenerateOption _imageGenerateOption = new ImageGenerateOption(0.89f);

    private volatile ImageGenContext? ImgGenContext;

    [ObservableProperty]
    public partial string? PreviewFilePath { get; set; }

    public WatermarkDesignViewModel()
    {
        if (string.IsNullOrEmpty(PreviewFilePath))
            return;

        PreviewImageSource = new(PreviewFilePath);
    }

    partial void OnPreviewFilePathChanged(string? value)
    {
        if (string.IsNullOrEmpty(PreviewFilePath))
            return;

        PreviewImageSource = new Bitmap
            (
            PreviewFilePath
//@"C:\Users\Administrator\Desktop\后期临时\DSC_337020240714000102.JPG"
//@"C:\Users\Administrator\Desktop\后期临时\DSC_1901.JPG"
);
    }

    [RelayCommand]
    public async Task SetBackground()
    {
        if (PreviewImageSource == null || string.IsNullOrWhiteSpace(PreviewFilePath))
            return;

        using MemoryStream input = new();
        using MemoryStream output = new();

        PreviewImageSource.Save(input);

        input.Seek(0, SeekOrigin.Begin);

        ExifInfo.From(File.OpenRead(PreviewFilePath));

        ImageService.GenerateWithOptions(input, output, ImageGenerateOption);

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
        ImgGenContext?.Dispose();
    }

    public void Dispose()
    {
        Reset();
    }
}
