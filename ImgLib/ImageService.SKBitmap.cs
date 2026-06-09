using SkiaSharp;

namespace ImgLib;

public sealed partial class ImageService
{

    /// <summary>
    /// 降采样
    /// </summary>
    /// <param name="original">准备要降采样的图片</param>
    /// <param name="value">降采样值 </param>
    /// <param name="isPercent">降采样值是否为百分比,false则表示为具体边长</param>
    /// <returns></returns>
    public static SKBitmap DownsampleImage(SKBitmap original, float value, bool isPercent = false)
    {
        int maxDimension = Math.Max(original.Width, original.Height);

        int targetDimension;

        if (isPercent)
        {
            float percent = value * maxDimension / 100f;
            targetDimension = (int)percent;
        }
        else
        {
            targetDimension = (int)value;
        }

        if (maxDimension <= targetDimension)
            return original;

        float scale = (float)targetDimension / maxDimension;
        int rw = (int)(original.Width * scale);
        int rh = (int)(original.Height * scale);

        return original.Resize(
            new SKImageInfo(rw, rh),
            new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
    }
}

public static class ImgLibSKBitmapExtensions
{
    public static SKBitmap DownsampleImage(this SKBitmap original, float value, bool isPercent = false) =>
        ImageService.DownsampleImage(original, value, isPercent);
}
