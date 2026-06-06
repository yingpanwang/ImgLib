using ImgLib.Models;
using SkiaSharp;

namespace ImgLib;

public sealed partial class ImageService
{
    /// <summary>
    /// 生成带有模糊背景和圆角阴影的图像，并将结果保存到输出流中。
    /// </summary>
    public static void GenerateWithOptions(
        Stream inputStream,
        Stream outputStream,
        float scale = 0.85f,
        float cornerRadius = 45f,
        float blurSigma = 25f,
        float shadowOffsetX = 50f,
        float shadowOffsetY = 50f,
        float shadowSigma = 25f,
        SKColor? shadowColor = null,
        ExifInfo? exifInfo = null,
        ImageGenerateOption? options = null)
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
            // 绘制"路径阴影"——用 MaskFilter 模糊蒙版
            using (var shadowPaint = new SKPaint
            {
                Color = shadowColor ?? SKColors.Black.WithAlpha(128),
                IsAntialias = true,
                // 用 MaskFilter 而不是 ImageFilter
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, shadowSigma)
            })
            {
                // 平移画布，先画出偏移量上的"毛边"阴影
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
            // 使用自定义选项或默认值
            var opts = options ?? new ImageGenerateOption(scale, cornerRadius, blurSigma, shadowOffsetX, shadowOffsetY, shadowSigma);

            // 解析水印模板
            string watermarkText = exifInfo != null
                ? opts.ParseWatermarkTemplate(exifInfo)
                : opts.WatermarkTemplate;

            // 如果没有模板内容，使用默认
            if (string.IsNullOrWhiteSpace(watermarkText))
                watermarkText = "NIKON Z 6_2";

            // 解析颜色
            SKColor textColor = ParseColor(opts.WatermarkColor);
            SKColor shadowColor = ParseColor(opts.WatermarkShadowColor);

            // 计算字体大小
            float fontSize = Math.Max(12, height * opts.WatermarkFontSizeRatio);

            // 创建字体和画笔
            using var typeface = SKTypeface.Default;
            SKFont wFont = new(typeface, fontSize)
            {
                Embolden = opts.WatermarkBold
            };

            using var wPaint = new SKPaint
            {
                Color = textColor,
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateDropShadow(
                    opts.WatermarkShadowOffsetX,
                    opts.WatermarkShadowOffsetY,
                    opts.WatermarkShadowSigma,
                    opts.WatermarkShadowSigma,
                    shadowColor)
            };

            // 测量文本尺寸
            float textWidth = wFont.MeasureText(watermarkText, wPaint);
            float textHeight = wFont.Metrics.CapHeight;

            // 计算水印位置
            float watermarkAreaTop = y + newHeight;
            float watermarkAreaHeight = height - watermarkAreaTop;

            // 垂直位置（从底部算起）
            float availableHeight = watermarkAreaHeight;
            float yOffset = availableHeight * (1 - opts.WatermarkVerticalPosition);

            // 水平对齐
            float textX;
            if (opts.WatermarkHorizontalAlignment.Equals("Left", StringComparison.OrdinalIgnoreCase))
            {
                textX = destRect.Left + 20;
            }
            else if (opts.WatermarkHorizontalAlignment.Equals("Right", StringComparison.OrdinalIgnoreCase))
            {
                textX = destRect.Right - 20;
            }
            else
            {
                textX = destRect.MidX;
            }

            float textY = watermarkAreaTop + yOffset + textHeight / 2;

            // 绘制水印文本
            if (opts.WatermarkHorizontalAlignment.Equals("Left", StringComparison.OrdinalIgnoreCase))
            {
                canvas.DrawText(watermarkText, textX, textY, SKTextAlign.Left, wFont, wPaint);
            }
            else if (opts.WatermarkHorizontalAlignment.Equals("Right", StringComparison.OrdinalIgnoreCase))
            {
                canvas.DrawText(watermarkText, textX, textY, SKTextAlign.Right, wFont, wPaint);
            }
            else
            {
                canvas.DrawText(watermarkText, textX, textY, SKTextAlign.Center, wFont, wPaint);
            }

            // 调试：绘制水印边框
            if (opts.ShowWatermarkBorder)
            {
                using var borderPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = ParseColor(opts.WatermarkBorderColor),
                    StrokeWidth = opts.WatermarkBorderWidth,
                    IsAntialias = true
                };

                // 计算水印边框矩形
                float borderPadding = 10;
                float borderLeft, borderRight;
                float borderTop = textY - textHeight / 2 - borderPadding;
                float borderBottom = textY + textHeight / 2 + borderPadding;

                if (opts.WatermarkHorizontalAlignment.Equals("Left", StringComparison.OrdinalIgnoreCase))
                {
                    borderLeft = textX - borderPadding;
                    borderRight = textX + textWidth + borderPadding;
                }
                else if (opts.WatermarkHorizontalAlignment.Equals("Right", StringComparison.OrdinalIgnoreCase))
                {
                    borderLeft = textX - textWidth - borderPadding;
                    borderRight = textX + borderPadding;
                }
                else
                {
                    borderLeft = textX - textWidth / 2 - borderPadding;
                    borderRight = textX + textWidth / 2 + borderPadding;
                }

                var borderRect = new SKRect(borderLeft, borderTop, borderRight, borderBottom);
                canvas.DrawRect(borderRect, borderPaint);

                // 绘制角点标记（更明显的调试标记）
                DrawDebugCornerMarkers(canvas, borderRect, borderPaint);
            }
        }

        void DrawDebugCornerMarkers(SKCanvas canvas, SKRect rect, SKPaint paint)
        {
            float markerSize = 5;

            // 左上角
            canvas.DrawLine(rect.Left, rect.Top, rect.Left + markerSize, rect.Top, paint);
            canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Top + markerSize, paint);

            // 右上角
            canvas.DrawLine(rect.Right - markerSize, rect.Top, rect.Right, rect.Top, paint);
            canvas.DrawLine(rect.Right, rect.Top, rect.Right, rect.Top + markerSize, paint);

            // 左下角
            canvas.DrawLine(rect.Left, rect.Bottom - markerSize, rect.Left, rect.Bottom, paint);
            canvas.DrawLine(rect.Left, rect.Bottom, rect.Left + markerSize, rect.Bottom, paint);

            // 右下角
            canvas.DrawLine(rect.Right - markerSize, rect.Bottom, rect.Right, rect.Bottom, paint);
            canvas.DrawLine(rect.Right, rect.Bottom - markerSize, rect.Right, rect.Bottom, paint);
        }
    }

    /// <summary>
    /// 使用 ImageGenerateOption 生成图像
    /// </summary>
    public static void GenerateWithOptions(
        Stream inputStream,
        Stream outputStream,
        ImageGenerateOption options,
        ExifInfo? exifInfo = null)
    {
        GenerateWithOptions(
            inputStream,
            outputStream,
            options.Scale,
            options.CornerRadius,
            options.BlurSigma,
            options.ShadowOffsetX,
            options.ShadowOffsetY,
            options.ShadowSigma,
            null,
            exifInfo,
            options
        );
    }

    /// <summary>
    /// 解析颜色字符串（十六进制格式，如 #FFFFFF 或 #80FFFFFF）
    /// </summary>
    private static SKColor ParseColor(string colorHex)
    {
        // 移除 #
        string hex = colorHex.TrimStart('#');

        // 处理简写格式（如 #FFF -> #FFFFFF）
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        // 解析 ARGB 或 RGB
        byte a, r, g, b;

        if (hex.Length == 8) // ARGB
        {
            a = Convert.ToByte(hex.Substring(0, 2), 16);
            r = Convert.ToByte(hex.Substring(2, 2), 16);
            g = Convert.ToByte(hex.Substring(4, 2), 16);
            b = Convert.ToByte(hex.Substring(6, 2), 16);
        }
        else if (hex.Length == 6) // RGB，默认不透明
        {
            a = 255;
            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
        }
        else
        {
            // 默认白色
            return SKColors.White;
        }

        return new SKColor(r, g, b, a);
    }

    [Obsolete("此方法已弃用，请使用带 ImageGenerateOption 参数的版本")]
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

        return minFontSize;
    }
}