using System.Diagnostics;
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
        ImageGenerateOption? options = null,
        bool isPreview = false)
    {
        using var original = SKBitmap.Decode(inputStream);
        if (original == null)
            throw new InvalidOperationException("无法解码图像");

        // 预览降采样处理
        SKBitmap? workingBitmap = null;
        bool needDisposeWorking = false;

        if (isPreview && options?.EnablePreviewDownsampling == true)
        {
            int maxDimension = Math.Max(original.Width, original.Height);
            int targetDimension;

            if (options.UsePreviewPercentMode)
            {
                // 按百分比计算目标边长
                targetDimension = (int)(maxDimension * options.PreviewMaxPercent / 100f);
            }
            else
            {
                // 按固定像素值
                targetDimension = options.PreviewMaxDimension;
            }

            if (maxDimension > targetDimension)
            {
                float downsampleScale = (float)targetDimension / maxDimension;
                int resizedWidth = (int)(original.Width * downsampleScale);
                int resizedHeight = (int)(original.Height * downsampleScale);

                workingBitmap = original.Resize(
                    new SKImageInfo(resizedWidth, resizedHeight),
                    new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
                needDisposeWorking = true;
            }
        }

        // 如果 workingBitmap 为空，说明不需要 resize，使用 original
        if (workingBitmap == null)
        {
            workingBitmap = original;
            needDisposeWorking = false; // original 会自动被 using 释放
        }

        int width = workingBitmap.Width;
        int height = workingBitmap.Height;

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
            canvas.DrawBitmap(workingBitmap, new SKRect(0, 0, width, height), blurPaint);

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
            canvas.DrawBitmap(workingBitmap, destRect);

            // 5. 恢复画布
            canvas.Restore();
        }

        void DrawWatermarkText()
        {
            var opts = options ?? new ImageGenerateOption(scale, cornerRadius, blurSigma, shadowOffsetX, shadowOffsetY, shadowSigma);

            // 获取有效的水印文本项列表（支持多水印）
            var effectiveTexts = opts.GetEffectiveWatermarkTexts();

            foreach (var textItem in effectiveTexts)
            {
                if (string.IsNullOrWhiteSpace(textItem.Template))
                    continue;

                DrawSingleWatermark(textItem);
            }

            void DrawSingleWatermark(WatermarkTextItem item)
            {
                // 解析水印模板
                string watermarkText = exifInfo != null
                    ? ParseTemplate(item.Template, exifInfo)
                    : item.Template;

                // 按换行符分割为多行
                string[] lines = watermarkText.Split('\n');
                int lineCount = lines.Length;

                // 解析颜色
                SKColor textColor = ParseColor(item.ColorHex);
                SKColor shadowColor = ParseColor(item.ShadowColorHex);

                // 计算初始字体大小
                float fontSize = Math.Max(12, height * item.FontSizeRatio);

                using var typeface = SKTypeface.Default;
                using var measurePaint = new SKPaint { IsAntialias = true };
                SKFont wFont = new(typeface, fontSize) { Embolden = item.Bold };

                // 测量所有行，找到最长行宽度
                float maxLineWidth = 0;
                foreach (string line in lines)
                {
                    float lineWidth = string.IsNullOrEmpty(line) ? 0 : wFont.MeasureText(line, measurePaint);
                    if (lineWidth > maxLineWidth) maxLineWidth = lineWidth;
                }

                // 行高：使用字体推荐行距 × 行间距系数
                float fontLineHeight = -wFont.Metrics.Ascent + wFont.Metrics.Descent + wFont.Metrics.Leading;
                float lineHeight = fontLineHeight * item.LineSpacing;
                float ascentAbove = -wFont.Metrics.Ascent;
                float descentBelow = wFont.Metrics.Descent;

                // 计算水印可用区域
                float watermarkAreaTop = y + newHeight;
                float watermarkAreaHeight = height - watermarkAreaTop;
                float maxAvailableWidth = width - 40;
                float maxAvailableHeight = watermarkAreaHeight - 20;

                // 计算文本块高度
                float CalcBlockHeight()
                {
                    return lineCount > 1
                        ? ascentAbove + (lineCount - 1) * lineHeight + descentBelow
                        : ascentAbove + descentBelow;
                }

                // 自动缩小字体以适应图片宽度
                bool needRemeasure = false;
                if (maxLineWidth > maxAvailableWidth && maxAvailableWidth > 0)
                {
                    float scaleRatio = maxAvailableWidth / maxLineWidth;
                    fontSize *= scaleRatio;
                    needRemeasure = true;
                }

                // 自动缩放适应高度
                if (item.AutoFitFont)
                {
                    float currentBlockHeight = CalcBlockHeight();
                    if (currentBlockHeight > maxAvailableHeight && maxAvailableHeight > 0)
                    {
                        float heightScale = maxAvailableHeight / currentBlockHeight;
                        if (maxLineWidth > maxAvailableWidth && maxAvailableWidth > 0)
                            heightScale = Math.Min(heightScale, maxAvailableWidth / maxLineWidth);
                        fontSize *= heightScale;
                        needRemeasure = true;
                    }
                }

                if (needRemeasure)
                {
                    fontSize = Math.Max(12, fontSize);
                    wFont.Dispose();
                    wFont = new SKFont(typeface, fontSize) { Embolden = item.Bold };

                    maxLineWidth = 0;
                    foreach (string line in lines)
                    {
                        float lineWidth = string.IsNullOrEmpty(line) ? 0 : wFont.MeasureText(line, measurePaint);
                        if (lineWidth > maxLineWidth) maxLineWidth = lineWidth;
                    }
                    fontLineHeight = -wFont.Metrics.Ascent + wFont.Metrics.Descent + wFont.Metrics.Leading;
                    lineHeight = fontLineHeight * item.LineSpacing;
                    ascentAbove = -wFont.Metrics.Ascent;
                    descentBelow = wFont.Metrics.Descent;
                }

                float totalBlockHeight = CalcBlockHeight();

                using var wPaint = new SKPaint
                {
                    Color = textColor,
                    IsAntialias = true,
                    ImageFilter = SKImageFilter.CreateDropShadow(
                        item.ShadowOffsetX,
                        item.ShadowOffsetY,
                        item.ShadowSigma,
                        item.ShadowSigma,
                        shadowColor)
                };

                // 垂直位置
                float blockCenterY = watermarkAreaTop + watermarkAreaHeight * (1 - item.VerticalPosition);
                float blockTop = blockCenterY - totalBlockHeight / 2f;
                float firstLineBaselineY = blockTop + ascentAbove;

                // 水平对齐
                float textX;
                SKTextAlign textAlign;
                if (item.HorizontalAlignment.Equals("Left", StringComparison.OrdinalIgnoreCase))
                {
                    textX = destRect.Left + 20;
                    textAlign = SKTextAlign.Left;
                }
                else if (item.HorizontalAlignment.Equals("Right", StringComparison.OrdinalIgnoreCase))
                {
                    textX = destRect.Right - 20;
                    textAlign = SKTextAlign.Right;
                }
                else
                {
                    textX = destRect.MidX;
                    textAlign = SKTextAlign.Center;
                }

                // 逐行绘制
                for (int i = 0; i < lineCount; i++)
                {
                    float lineY = firstLineBaselineY + i * lineHeight;
                    canvas.DrawText(lines[i], textX, lineY, textAlign, wFont, wPaint);
                }

                // 调试边框
                if (item.ShowBorder)
                {
                    using var borderPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        Color = ParseColor(item.BorderColorHex),
                        StrokeWidth = item.BorderWidth,
                        IsAntialias = true
                    };

                    float borderPadding = 10;
                    float borderLeft, borderRight;
                    float borderTop = blockTop - borderPadding;
                    float borderBottom = blockTop + totalBlockHeight + borderPadding;

                    if (item.HorizontalAlignment.Equals("Left", StringComparison.OrdinalIgnoreCase))
                    {
                        borderLeft = textX - borderPadding;
                        borderRight = textX + maxLineWidth + borderPadding;
                    }
                    else if (item.HorizontalAlignment.Equals("Right", StringComparison.OrdinalIgnoreCase))
                    {
                        borderLeft = textX - maxLineWidth - borderPadding;
                        borderRight = textX + borderPadding;
                    }
                    else
                    {
                        borderLeft = textX - maxLineWidth / 2 - borderPadding;
                        borderRight = textX + maxLineWidth / 2 + borderPadding;
                    }

                    var borderRect = new SKRect(borderLeft, borderTop, borderRight, borderBottom);
                    canvas.DrawRect(borderRect, borderPaint);
                    DrawDebugCornerMarkers(canvas, borderRect, borderPaint);
                }
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

        // 释放降采样生成的 bitmap（如果需要）
        if (needDisposeWorking && workingBitmap != null)
        {
            workingBitmap.Dispose();
        }
    }

    /// <summary>
    /// 使用 ImageGenerateOption 生成图像
    /// </summary>
    public static void GenerateWithOptions(
        Stream inputStream,
        Stream outputStream,
        ImageGenerateOption options,
        ExifInfo? exifInfo = null,
        bool isPreview = false)
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
            options,
            isPreview
        );
    }

    /// <summary>
    /// 解析水印模板，替换 EXIF 占位符。
    /// </summary>
    private static string ParseTemplate(string template, ExifInfo exif)
    {
        string result = template;
        foreach (var kvp in exif.GetTemplateReplacements())
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value ?? "N/A");
        }
        return result;
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