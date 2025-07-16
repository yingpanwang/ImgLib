using SkiaSharp;
using System.Threading.Channels;
using static ImgLib.ImageService;

namespace ImgLib;

public class ImageGenContext : IDisposable
{
    public SKBitmap Origin { get; private set; }

    public SKSurface Surface { get; set; }

    public SKCanvas Canvas => Surface.Canvas;

    public ImageGenerateOption Options { get; set; } = new(scale: 0.85f);

    private Channel<ImageGenerateOption> _imageGenerateChannel;

    public SKImage? CurrentOutputImage { get; private set; }

    private readonly SemaphoreSlim _locker = new(1);

    public ImageGenContext(SKBitmap origin)
    {
        Origin = origin;
        Surface = SKSurface.Create(new SKImageInfo(origin.Width, origin.Height));

        _imageGenerateChannel = Channel.CreateUnbounded<ImageGenerateOption>();

        _ = Task.Factory.StartNew(Start, TaskCreationOptions.LongRunning);
    }

    private async Task Start()
    {
        await foreach (var option in _imageGenerateChannel.Reader.ReadAllAsync())
        {
            await _locker.WaitAsync();
            try
            {
                GenerateWithContext(this);

                var output = Generate();

                Reset(output);
            }
            finally
            {
                _locker.Release();
            }
        }
    }

    public async Task Listen()
    {
        await _imageGenerateChannel.Writer.WriteAsync(this.Options);
    }

    private SKImage Generate()
    {
        GenerateWithContext(this);
        return Surface.Snapshot();
    }

    public void Reset(SKImage? init = null)
    {
        Origin?.Dispose();

        CurrentOutputImage?.Dispose();

        if (init != null)
        {
            Origin = SKBitmap.FromImage(init);

            Surface = SKSurface.Create(new SKImageInfo(Origin.Width, Origin.Height));

            CurrentOutputImage = init;
        }
    }

    public void Dispose()
    {
        CurrentOutputImage?.Dispose();
        Canvas.Dispose();
        Surface.Dispose();
        Origin.Dispose();

        GC.SuppressFinalize(this); // Fix for CA1816  
    }
}

