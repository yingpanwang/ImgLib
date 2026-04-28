using SkiaSharp;

namespace ImgLib;

public sealed partial class ImageService
{
    /// <summary>
    /// 生成带有模糊背景和圆角阴影的图像，并将结果保存到输出流中。
    /// </summary>
    /// <param name="inputStream"></param>
    /// <param name="outputStream"></param>
    /// <param name="scale"></param>
    /// <param name="cornerRadius"></param>
    /// <param name="blurSigma"></param>
    /// <param name="shadowOffsetX"></param>
    /// <param name="shadowOffsetY"></param>
    /// <param name="shadowSigma"></param>
    /// <param name="shadowColor"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void GenerateWithOptions(
        Stream inputStream,
        Stream outputStream,
        float scale = 0.85f,
        float cornerRadius = 45f,
        float blurSigma = 25f,
        float shadowOffsetX = 50f,
        float shadowOffsetY = 50f,
        float shadowSigma = 25f,
        SKColor? shadowColor = null)
    {
        using var original = SKBitmap.Decode(inputStream);
        if (original == null)
            throw new InvalidOperationException("无法解码图像");

        int width = original.Width;
        int height = original.Height;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        // 新生成中间图片参数
        float newWidth = width * scale;
        float newHeight = height * scale;
        float x = (width - newWidth) / 2;
        float y = (height - newHeight) / 2;

        var destRect = new SKRect(x, y, x + newWidth, y + newHeight);

        DrawMainContent();

        DrawWatermarkText();

        // ---------- 5. 导出结果 ----------
        using var image = surface.Snapshot();

        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        data.SaveTo(outputStream);

        void DrawMainContent()
        {
            // ---------- 1. 背景模糊 ----------
            using var blurPaint = new SKPaint
            {
                ImageFilter = SKImageFilter.CreateBlur(blurSigma, blurSigma),
                IsAntialias = true
            };
            canvas.DrawBitmap(original, new SKRect(0, 0, width, height), blurPaint);

            // ---------- 2. 中央图绘制准备 ----------
            // 绘制“路径阴影”——用 MaskFilter 模糊蒙版
            using (var shadowPaint = new SKPaint
            {
                Color = shadowColor ?? SKColors.Black.WithAlpha(128),
                IsAntialias = true,
                // 用 MaskFilter 而不是 ImageFilter
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, shadowSigma)
            })
            {
                // 平移画布，先画出偏移量上的“毛边”阴影
                canvas.Save();
                canvas.Translate(shadowOffsetX, shadowOffsetY);
                canvas.DrawRoundRect(destRect, cornerRadius, cornerRadius, shadowPaint);
                canvas.Restore();
            }

            // 3. 裁剪圆角区域
            canvas.Save();
            canvas.ClipRoundRect(new SKRoundRect(destRect, cornerRadius, cornerRadius),
                                 SKClipOperation.Intersect, true);

            // 4. 在裁剪后的区域内绘制原图
            canvas.DrawBitmap(original, destRect);

            // 5. 恢复画布
            canvas.Restore();
        }

        void DrawWatermarkText()
        {
            #region 水印文本

            string watermarkText = "NIKON Z 6_2";
            // 计算水印最大可用宽度（使用 destRect 宽度或整图宽度的 90%）
            float maxTextWidth = destRect.Width * 0.9f;

            // 自动计算最优字体大小
            float fontSize = CalculateOptimalFontSize(canvas, watermarkText, maxTextWidth);
            fontSize = 4000 * 0.03f;
            // 创建字体和画笔
            SKFont wFont = new(SKTypeface.Default, fontSize) { Embolden = true };
            SKPaint wPaint = new()
            {
                Color = SKColors.White,
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateDropShadow(2, 2, 5, 5, SKColors.Black.WithAlpha(128)),
            };

            // 计算垂直位置（依然居中于图像下方区域）
            float watermarkHeight = height - (y + newHeight);
            float watermarkY = y + newHeight + (watermarkHeight / 2);

            canvas.DrawText(watermarkText, destRect.MidX, watermarkY, SKTextAlign.Center, wFont, wPaint);

            // 创建一个画笔用于绘制边框
            using var borderPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,   // 只绘制边框
                Color = SKColors.LimeGreen,    // 边框颜色（调试建议用亮色）
                StrokeWidth = 2,
                IsAntialias = true
            };

            // 构造水印区域矩形（与文字对齐区域一致）
            var watermarkRect = new SKRect(
                destRect.Left,
                y + newHeight,     // 下边缘起点
                destRect.Right,
                height             // 整个画布底部
            );

            // 画出矩形边框
            canvas.DrawRect(watermarkRect, borderPaint);

            #endregion 水印文本
        }
    }

    public static void GenerateWithOptions(
        Stream inputStream,
        Stream outputStream,
        ImageGenerateOption options
        )
    {
        GenerateWithOptions(
            inputStream,
            outputStream,
            options.Scale,
            options.CornerRadius,
            options.BlurSigma,
            options.ShadowOffsetX,
            options.ShadowOffsetX,
            options.ShadowSigma
            );
    }

    private static float CalculateOptimalFontSize(SKCanvas canvas, string text, float maxWidth, float maxFontSize = 72, float minFontSize = 12)
    {
        using var paint = new SKPaint { IsAntialias = true };

        for (float fontSize = maxFontSize; fontSize >= minFontSize; fontSize -= 1f)
        {
            paint.TextSize = fontSize;
            float textWidth = paint.MeasureText(text);

            if (textWidth <= maxWidth)
                return fontSize;
        }

        return minFontSize; // 最小 fallback 字号
    }
}