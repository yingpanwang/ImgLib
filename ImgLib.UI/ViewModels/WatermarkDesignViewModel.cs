
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using System.IO;

namespace ImgLib.UI.ViewModels;

public sealed partial class WatermarkDesignViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap previewImageSource;

    [ObservableProperty]
    private ImageGenerateOption imageGenerateOption = new ImageGenerateOption(0.89f);

    private volatile ImageGenContext? ImgGenContext;

    public WatermarkDesignViewModel(string filePath)
    {
        PreviewImageSource = new Bitmap
            (
            filePath
//@"C:\Users\Administrator\Desktop\后期临时\DSC_337020240714000102.JPG"
//@"C:\Users\Administrator\Desktop\后期临时\DSC_1901.JPG"
);
    }


    [RelayCommand]
    public async Task SetBackground()
    {

        if (this.ImgGenContext == null)
        {
            Stream inputStream = new MemoryStream();

            PreviewImageSource.Save(inputStream);

            inputStream.Seek(0, SeekOrigin.Begin);

            var original = SKBitmap.Decode(inputStream);

            ImgGenContext = new ImageGenContext(original);

        }

        ImgGenContext.Options = ImageGenerateOption;

        Stream? output = default;

        //using Stream inputStream = new MemoryStream();

        //await Task.Run(() =>
        //{
        //    PreviewImageSource.Save(inputStream);

        //    inputStream.Seek(0, SeekOrigin.Begin);

        //    output = new MemoryStream();
        //    ImageService.Generate(inputStream, output);
        //    output.Seek(0, SeekOrigin.Begin);
        //});

        await ImgGenContext.Listen();

        using var imgData = ImgGenContext.CurrentOutputImage?.Encode(SKEncodedImageFormat.Jpeg, 100);

        output = imgData?.AsStream();

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
