
using Avalonia.Media.Imaging;
using System.IO;
using System.Threading;

namespace ImgLib.UI.ViewModels;

public sealed partial class WatermarkDesignViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap previewImageSource;

    public WatermarkDesignViewModel()
    {
        PreviewImageSource = new Bitmap(@"C:\Users\Administrator\Desktop\后期临时\DSC_337020240714000102.JPG");

        Task.Run(() =>
        {
            Thread.Sleep(5000);

            using Stream inputStream = new MemoryStream();
            PreviewImageSource.Save(inputStream);
            inputStream.Seek(0, SeekOrigin.Begin);

            var output = new MemoryStream();
            ImageService.Generate(inputStream, output);
            output.Seek(0, SeekOrigin.Begin);
            PreviewImageSource = new Bitmap(output);
        });
    }

    public Task Load()
    {
        return Task.CompletedTask;
    }
}
