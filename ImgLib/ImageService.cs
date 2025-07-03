using SkiaSharp;

namespace ImgLib;

public sealed partial class ImageService
{
    public static void Generate(Stream inputStream, Stream outputStream)
    {
        using var input = inputStream;
        using var original = SKBitmap.Decode(input);
        int width = original.Width;
        int height = original.Height;

        // 创建画布
        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        // ---------- 1. 绘制模糊背景 ----------
        using var paint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(25, 25), // sigma 越大越模糊
            IsAntialias = true
        };
        canvas.DrawBitmap(original, new SKRect(0, 0, width, height), paint);

        // ---------- 2. 绘制原图到中央 ----------
        float scale = 0.85f; // 缩放原图尺寸为原始的 85%
        float newWidth = width * scale;
        float newHeight = height * scale;

        float x = (width - newWidth) / 2;
        float y = (height - newHeight) / 2;

        var destRect = new SKRect(x, y, x + newWidth, y + newHeight);
        canvas.DrawBitmap(original, destRect);

        // ---------- 3. 输出图像 ----------
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        data.SaveTo(outputStream);
    }
    public static void Generate(string inputPath, string outputPath)
    {
        using var input = File.OpenRead(inputPath);

        Generate(input, File.Create(outputPath));
    }
}
