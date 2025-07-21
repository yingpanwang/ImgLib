using SkiaSharp;
using static ImgLib.ImageService;

namespace ImgLib;

public class ImageGenContext : IDisposable
{
    public SKBitmap Origin { get; private set; }

    public SKSurface Surface { get; set; }

    public SKCanvas Canvas => Surface.Canvas;

    public ImageGenerateOption Options { get; set; } = new(scale: 0.85f);

    public SKImage? CurrentOutputImage { get; private set; }


    public ImageGenContext(SKBitmap origin)
    {
        Origin = origin;
        Surface = SKSurface.Create(new SKImageInfo(origin.Width, origin.Height));
    }

    public SKImage Generate()
    {
        GenerateWithContext(this);
        var snapshot = Surface.Snapshot();

        Reset(snapshot);

        return snapshot;
    }

    public void Reset(SKImage? init = null)
    {
        Surface?.Dispose();
        CurrentOutputImage?.Dispose();

        if (init != null)
        {
            Origin?.Dispose();
            Origin = SKBitmap.FromImage(init);

            Surface = SKSurface.Create(new SKImageInfo(Origin.Width, Origin.Height));
            CurrentOutputImage = init;
        }
    }

    public void Dispose()
    {
        CurrentOutputImage?.Dispose();
        Surface.Dispose();
        Origin.Dispose();

        GC.SuppressFinalize(this); // Fix for CA1816  
    }
}

