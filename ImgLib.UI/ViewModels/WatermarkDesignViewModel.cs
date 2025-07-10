
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using System.IO;

namespace ImgLib.UI.ViewModels;

public sealed partial class WatermarkDesignViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap previewImageSource;

    public WatermarkDesignViewModel()
    {
        PreviewImageSource = new Bitmap
            (
           @"C:\Users\Administrator\Desktop\后期临时\DSC_337020240714000102.JPG"
//@"C:\Users\Administrator\Desktop\后期临时\DSC_1901.JPG"
);
    }

    [RelayCommand]
    public async Task SetBackground()
    {

        Stream? output = default;

        using Stream inputStream = new MemoryStream();

        await Task.Run(() =>
        {
            PreviewImageSource.Save(inputStream);

            inputStream.Seek(0, SeekOrigin.Begin);

            output = new MemoryStream();
            ImageService.Generate(inputStream, output);
            output.Seek(0, SeekOrigin.Begin);
        });


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
}
