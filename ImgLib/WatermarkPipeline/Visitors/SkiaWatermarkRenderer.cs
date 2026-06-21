using ImgLib.Models;
using SkiaSharp;

namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 基于 SkiaSharp 的水印渲染器 —— 实现 <see cref="IWatermarkCommandVisitor"/>，
/// 将命令数据翻译为 SKCanvas 绘制调用。
/// </summary>
public class SkiaWatermarkRenderer : IWatermarkCommandVisitor
{
    // ═══════════════════════════════════════════════
    //  Visit 方法 —— 双分派的接收端
    // ═══════════════════════════════════════════════

    public void VisitBlurBackground(BlurBackgroundCommand cmd, WatermarkRenderContext ctx)
    {
        if (cmd.Sigma <= 0)
        {
            // 无模糊：直接绘制原图
            ctx.Canvas.DrawBitmap(ctx.WorkingBitmap,
                new SKRect(0, 0, ctx.Width, ctx.Height));
            return;
        }

        using var blurPaint = new SKPaint
        {
            ImageFilter = SKImageFilter.CreateBlur(cmd.Sigma, cmd.Sigma),
            IsAntialias = true
        };
        ctx.Canvas.DrawBitmap(ctx.WorkingBitmap,
            new SKRect(0, 0, ctx.Width, ctx.Height), blurPaint);
    }

    public void VisitImageShadow(ImageShadowCommand cmd, WatermarkRenderContext ctx)
    {
        var color = ParseColor(cmd.ColorHex, SKColors.Black.WithAlpha(128));

        using var shadowPaint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, cmd.Sigma)
        };

        ctx.Canvas.Save();
        ctx.Canvas.Translate(cmd.OffsetX, cmd.OffsetY);
        ctx.Canvas.DrawRoundRect(ctx.ImageDestRect, cmd.CornerRadius, cmd.CornerRadius, shadowPaint);
        ctx.Canvas.Restore();
    }

    public void VisitSaveCanvas(SaveCanvasCommand cmd, WatermarkRenderContext ctx)
        => ctx.Canvas.Save();

    public void VisitRestoreCanvas(RestoreCanvasCommand cmd, WatermarkRenderContext ctx)
        => ctx.Canvas.Restore();

    public void VisitClipRoundedRect(ClipRoundedRectCommand cmd, WatermarkRenderContext ctx)
    {
        var roundRect = new SKRoundRect(ctx.ImageDestRect, cmd.CornerRadius, cmd.CornerRadius);
        ctx.Canvas.ClipRoundRect(roundRect, SKClipOperation.Intersect, true);
    }

    public void VisitDrawImage(DrawImageCommand cmd, WatermarkRenderContext ctx)
        => ctx.Canvas.DrawBitmap(ctx.WorkingBitmap, ctx.ImageDestRect);

    public void VisitTextWatermark(TextWatermarkCommand cmd, WatermarkRenderContext ctx)
    {
        // ── 1. 解析模板 ──
        string text = ctx.Exif != null
            ? ParseTemplate(cmd.Template, ctx.Exif)
            : cmd.Template;

        string[] lines = text.Split('\n');
        int lineCount = lines.Length;

        // ── 2. 计算最终字体大小（含自动缩放）──
        float fontSize = ComputeFinalFontSize(cmd, ctx, lines, lineCount,
            out float maxLineWidth, out float lineHeight,
            out float ascentAbove, out float descentBelow);

        float totalBlockHeight = CalcBlockHeight(lineCount, ascentAbove, descentBelow, lineHeight);

        // ── 3. 可用区域 & 定位 ──
        float textX, blockTop, firstLineBaselineY;
        SKTextAlign textAlign;

        if (cmd.UseAdvancedPositioning)
        {
            (textX, blockTop, firstLineBaselineY, textAlign) =
                ComputeAdvancedPosition(cmd, ctx, totalBlockHeight, maxLineWidth,
                    fontSize, ascentAbove);
        }
        else
        {
            (textX, blockTop, firstLineBaselineY, textAlign) =
                ComputeSimplePosition(cmd, ctx, totalBlockHeight, maxLineWidth,
                    fontSize, ascentAbove);
        }

        // ── 4. 逐行绘制 ──
        var textColor = ParseColor(cmd.ColorHex, SKColors.White);
        var shadowColor = ParseColor(cmd.ShadowColorHex, SKColors.Black.WithAlpha(128));

        using var typeface = SKTypeface.Default;
        using var drawFont = new SKFont(typeface, fontSize) { Embolden = cmd.Bold };
        using var wPaint = new SKPaint
        {
            Color = textColor,
            IsAntialias = true,
            ImageFilter = SKImageFilter.CreateDropShadow(
                cmd.ShadowOffsetX, cmd.ShadowOffsetY,
                cmd.ShadowSigma, cmd.ShadowSigma,
                shadowColor)
        };

        // ═══ 高级定位且不包含模糊边框时：裁剪到图片区域 ═══
        bool needClip = cmd.UseAdvancedPositioning && !cmd.RelativeToCanvas;
        if (needClip)
        {
            ctx.Canvas.Save();
            var roundRect = new SKRoundRect(ctx.ImageDestRect, 0, 0);
            ctx.Canvas.ClipRoundRect(roundRect, SKClipOperation.Intersect, true);
        }

        if (cmd.Repeat)
        {
            if (cmd.UseAdvancedPositioning)
                DrawRepeatedLines(ctx, cmd, lines, drawFont, wPaint, lineHeight,
                    ascentAbove, totalBlockHeight);
            else
                DrawSimpleRepeatedLines(ctx, cmd, lines, drawFont, wPaint, lineHeight,
                    ascentAbove, totalBlockHeight);
        }
        else
        {
            for (int i = 0; i < lineCount; i++)
            {
                float lineY = firstLineBaselineY + i * lineHeight;
                ctx.Canvas.DrawText(lines[i], textX, lineY, textAlign, drawFont, wPaint);
            }
        }

        if (needClip)
        {
            ctx.Canvas.Restore();
        }

        // ── 7. 缓存布局到上下文（供后续 DebugBorder 命令使用）──

        // 计算调试边框矩形
        float borderPadding = 10;
        float borderTop = blockTop - borderPadding;
        float borderBottom = blockTop + totalBlockHeight + borderPadding;
        float borderLeft, borderRight;

        string hAlign = cmd.UseAdvancedPositioning ? cmd.AdvancedHAlign : cmd.HorizontalAlignment;

        if (hAlign.Equals("Left", StringComparison.OrdinalIgnoreCase))
        {
            borderLeft = textX - borderPadding;
            borderRight = textX + maxLineWidth + borderPadding;
        }
        else if (hAlign.Equals("Right", StringComparison.OrdinalIgnoreCase))
        {
            borderLeft = textX - maxLineWidth - borderPadding;
            borderRight = textX + borderPadding;
        }
        else
        {
            borderLeft = textX - maxLineWidth / 2 - borderPadding;
            borderRight = textX + maxLineWidth / 2 + borderPadding;
        }

        var debugRect = new SKRect(borderLeft, borderTop, borderRight, borderBottom);

        // 追加到多文本布局列表（供 DebugBorder 遍历）
        ctx.TextBlockLayouts.Add(new TextBlockLayout
        {
            TextLines = lines,
            TextLineCount = lineCount,
            FontSize = fontSize,
            MaxLineWidth = maxLineWidth,
            LineHeight = lineHeight,
            AscentAbove = ascentAbove,
            DescentBelow = descentBelow,
            TotalBlockHeight = totalBlockHeight,
            FirstLineBaselineY = firstLineBaselineY,
            TextX = textX,
            TextAlign = textAlign,
            DebugBorderRect = debugRect,
            CommandOrder = cmd.Order,
            ShowBorder = cmd.ShowBorder,
            BorderColorHex = cmd.BorderColorHex,
            BorderWidth = cmd.BorderWidth,
        });

        // 保留最后一个文本块的布局到旧字段（向后兼容）
        // ctx.TextLines = lines;
        // ctx.TextLineCount = lineCount;
        // ctx.FontSize = fontSize;
        // ctx.MaxLineWidth = maxLineWidth;
        // ctx.LineHeight = lineHeight;
        // ctx.AscentAbove = ascentAbove;
        // ctx.DescentBelow = descentBelow;
        // ctx.TotalBlockHeight = totalBlockHeight;
        // ctx.FirstLineBaselineY = firstLineBaselineY;
        // ctx.TextX = textX;
        // ctx.TextAlign = textAlign;
        // ctx.DebugBorderRect = debugRect;
    }

    public void VisitDebugBorder(DebugBorderCommand cmd, WatermarkRenderContext ctx)
    {
        // 多水印模式：遍历所有文本块布局，为开启调试边框的块绘制
        if (ctx.TextBlockLayouts.Count > 0)
        {
            foreach (var layout in ctx.TextBlockLayouts)
            {
                if (!layout.ShowBorder)
                    continue;

                var rect = layout.DebugBorderRect;
                if (rect == SKRect.Empty)
                    continue;

                using var borderPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = ParseColor(layout.BorderColorHex, SKColors.Lime),
                    StrokeWidth = layout.BorderWidth,
                    IsAntialias = true
                };

                ctx.Canvas.DrawRect(rect, borderPaint);
                DrawCornerMarkers(ctx.Canvas, rect, borderPaint);
            }
            return;
        }

        // 单水印模式（向后兼容）：使用旧字段
        using var borderPaint2 = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = ParseColor(cmd.ColorHex, SKColors.Lime),
            StrokeWidth = cmd.StrokeWidth,
            IsAntialias = true
        };

        var legacyRect = ctx.DebugBorderRect;
        if (legacyRect == SKRect.Empty) return;

        ctx.Canvas.DrawRect(legacyRect, borderPaint2);
        DrawCornerMarkers(ctx.Canvas, legacyRect, borderPaint2);
    }

    #region 辅助方法

    /// <summary>简单模式定位（原有逻辑）：文本块在图片下方水印区域内</summary>
    private static (float textX, float blockTop, float firstLineBaselineY, SKTextAlign align)
        ComputeSimplePosition(TextWatermarkCommand cmd, WatermarkRenderContext ctx,
            float totalBlockHeight, float maxLineWidth, float fontSize, float ascentAbove)
    {
        float watermarkAreaTop = ctx.ImageDestRect.Top + ctx.NewHeight;
        float watermarkAreaHeight = ctx.Height - watermarkAreaTop;

        float blockCenterY = watermarkAreaTop + watermarkAreaHeight * (1 - cmd.VerticalPosition);
        float blockTop = blockCenterY - totalBlockHeight / 2f;
        float firstLineBaselineY = blockTop + ascentAbove;

        return (GetSimpleX(cmd, ctx, maxLineWidth, fontSize), blockTop,
            firstLineBaselineY, GetSimpleAlign(cmd));
    }

    private static float GetSimpleX(TextWatermarkCommand cmd, WatermarkRenderContext ctx,
        float maxLineWidth, float fontSize)
    {
        if (cmd.HorizontalAlignment.Equals("Left", StringComparison.OrdinalIgnoreCase))
            return ctx.ImageDestRect.Left + 20;
        if (cmd.HorizontalAlignment.Equals("Right", StringComparison.OrdinalIgnoreCase))
            return ctx.ImageDestRect.Right - 20;
        return ctx.ImageDestRect.MidX;
    }

    private static SKTextAlign GetSimpleAlign(TextWatermarkCommand cmd)
    {
        if (cmd.HorizontalAlignment.Equals("Left", StringComparison.OrdinalIgnoreCase))
            return SKTextAlign.Left;
        if (cmd.HorizontalAlignment.Equals("Right", StringComparison.OrdinalIgnoreCase))
            return SKTextAlign.Right;
        return SKTextAlign.Center;
    }

    /// <summary>高级模式定位：以图片内部比例坐标 + 锚点定位</summary>
    private static (float textX, float blockTop, float firstLineBaselineY, SKTextAlign align)
        ComputeAdvancedPosition(TextWatermarkCommand cmd, WatermarkRenderContext ctx,
            float totalBlockHeight, float maxLineWidth, float fontSize, float ascentAbove)
    {
        // 选择参考矩形：图片区域 vs 整个画布（含模糊边框）
        float refLeft, refTop, refRight, refBottom;
        if (cmd.RelativeToCanvas)
        {
            refLeft = 0;
            refTop = 0;
            refRight = ctx.Width;
            refBottom = ctx.Height;
        }
        else
        {
            refLeft = ctx.ImageDestRect.Left;
            refTop = ctx.ImageDestRect.Top;
            refRight = ctx.ImageDestRect.Right;
            refBottom = ctx.ImageDestRect.Bottom;
        }

        float refWidth = refRight - refLeft;
        float refHeight = refBottom - refTop;

        // 锚点基准坐标（比例 → 像素）
        float anchorX = refLeft + refWidth * cmd.PositionX;
        float anchorY = refTop + refHeight * cmd.PositionY;

        // 水平偏移（根据 AdvancedHAlign）
        float textX;
        SKTextAlign align;
        if (cmd.AdvancedHAlign.Equals("Left", StringComparison.OrdinalIgnoreCase))
        {
            textX = anchorX;
            align = SKTextAlign.Left;
        }
        else if (cmd.AdvancedHAlign.Equals("Right", StringComparison.OrdinalIgnoreCase))
        {
            textX = anchorX;
            align = SKTextAlign.Right;
        }
        else // Center
        {
            textX = anchorX;
            align = SKTextAlign.Center;
        }

        // 垂直偏移（根据 AdvancedVAlign）
        float blockTop;
        if (cmd.AdvancedVAlign.Equals("Top", StringComparison.OrdinalIgnoreCase))
            blockTop = anchorY;
        else if (cmd.AdvancedVAlign.Equals("Bottom", StringComparison.OrdinalIgnoreCase))
            blockTop = anchorY - totalBlockHeight;
        else // Center
            blockTop = anchorY - totalBlockHeight / 2f;

        // ═══ 边界裁剪：确保文本始终完全在参考范围内 ═══

        // 水平裁剪
        switch (align)
        {
            case SKTextAlign.Left:
                textX = Math.Clamp(textX, refLeft, refRight - maxLineWidth);
                break;
            case SKTextAlign.Right:
                textX = Math.Clamp(textX, refLeft + maxLineWidth, refRight);
                break;
            default: // Center
                float halfW = maxLineWidth / 2f;
                textX = Math.Clamp(textX, refLeft + halfW, refRight - halfW);
                break;
        }

        // 垂直裁剪
        blockTop = Math.Clamp(blockTop, refTop, refBottom - totalBlockHeight);
        float firstLineBaselineY = blockTop + ascentAbove;

        return (textX, blockTop, firstLineBaselineY, align);
    }

    /// <summary>平铺模式：在图片范围内网格状重复绘制文本</summary>
    private static void DrawRepeatedLines(WatermarkRenderContext ctx, TextWatermarkCommand cmd,
        string[] lines, SKFont font, SKPaint paint, float lineHeight,
        float ascentAbove, float totalBlockHeight)
    {
        float refLeft, refTop, refRight, refBottom;
        if (cmd.RelativeToCanvas)
        {
            refLeft = 0;
            refTop = 0;
            refRight = ctx.Width;
            refBottom = ctx.Height;
        }
        else
        {
            refLeft = ctx.ImageDestRect.Left;
            refTop = ctx.ImageDestRect.Top;
            refRight = ctx.ImageDestRect.Right;
            refBottom = ctx.ImageDestRect.Bottom;
        }

        float refWidth = refRight - refLeft;
        float refHeight = refBottom - refTop;
        float spacingX = refWidth * cmd.RepeatSpacingX;
        float spacingY = refHeight * cmd.RepeatSpacingY;

        // 旋转
        if (cmd.RepeatAngle != 0)
        {
            float cx = refLeft + refWidth / 2f;
            float cy = refTop + refHeight / 2f;
            ctx.Canvas.Save();
            ctx.Canvas.RotateDegrees(cmd.RepeatAngle, cx, cy);
        }

        for (float cy = refTop; cy < refBottom + totalBlockHeight; cy += spacingY)
        {
            for (float cx = refLeft; cx < refRight; cx += spacingX)
            {
                if (cy + totalBlockHeight < refTop || cy > refBottom)
                    continue;

                for (int i = 0; i < lines.Length; i++)
                {
                    float ly = cy + ascentAbove + i * lineHeight;
                    if (ly < refTop || ly > refBottom)
                        continue;

                    float clampedX = Math.Clamp(cx, refLeft, refRight);
                    ctx.Canvas.DrawText(lines[i], clampedX, ly, SKTextAlign.Left, font, paint);
                }
            }
        }

        if (cmd.RepeatAngle != 0)
        {
            ctx.Canvas.Restore();
        }
    }

    /// <summary>简单模式平铺：在水印区域（图片下方）内网格状重复</summary>
    private static void DrawSimpleRepeatedLines(WatermarkRenderContext ctx, TextWatermarkCommand cmd,
        string[] lines, SKFont font, SKPaint paint, float lineHeight,
        float ascentAbove, float totalBlockHeight)
    {
        float areaLeft = ctx.ImageDestRect.Left;
        float areaTop = ctx.ImageDestRect.Top + ctx.NewHeight;
        float areaRight = ctx.ImageDestRect.Right;
        float areaBottom = ctx.Height;

        float areaWidth = areaRight - areaLeft;
        float areaHeight = areaBottom - areaTop;

        float spacingX = areaWidth * cmd.RepeatSpacingX;
        float spacingY = areaHeight * cmd.RepeatSpacingY;

        if (cmd.RepeatAngle != 0)
        {
            float cx = areaLeft + areaWidth / 2f;
            float cy = areaTop + areaHeight / 2f;
            ctx.Canvas.Save();
            ctx.Canvas.RotateDegrees(cmd.RepeatAngle, cx, cy);
        }

        for (float cy = areaTop; cy < areaBottom + totalBlockHeight; cy += spacingY)
        {
            for (float cx = areaLeft; cx < areaRight; cx += spacingX)
            {
                if (cy + totalBlockHeight < areaTop || cy > areaBottom)
                    continue;

                for (int i = 0; i < lines.Length; i++)
                {
                    float ly = cy + ascentAbove + i * lineHeight;
                    if (ly < areaTop || ly > areaBottom)
                        continue;

                    float clampedX = Math.Clamp(cx, areaLeft, areaRight);
                    ctx.Canvas.DrawText(lines[i], clampedX, ly, SKTextAlign.Left, font, paint);
                }
            }
        }

        if (cmd.RepeatAngle != 0)
        {
            ctx.Canvas.Restore();
        }
    }

    /// <summary>计算文本块总高度</summary>
    private static float CalcBlockHeight(int lineCount, float ascentAbove, float descentBelow, float lineHeight)
    {
        return lineCount > 1
            ? ascentAbove + (lineCount - 1) * lineHeight + descentBelow
            : ascentAbove + descentBelow;
    }

    /// <summary>计算最终字体大小：初始估算 → 宽度约束缩放 → AutoFitFont 高度约束缩放 → 重测量</summary>
    private static float ComputeFinalFontSize(
        TextWatermarkCommand cmd,
        WatermarkRenderContext ctx,
        string[] lines,
        int lineCount,
        out float maxLineWidth,
        out float lineHeight,
        out float ascentAbove,
        out float descentBelow)
    {
        float fontSize = Math.Max(12, ctx.Height * cmd.FontSizeRatio);

        float maxAvailableWidth = ctx.Width - 40;
        float watermarkAreaTop = ctx.ImageDestRect.Top + ctx.NewHeight;
        float watermarkAreaHeight = ctx.Height - watermarkAreaTop;
        float maxAvailableHeight = watermarkAreaHeight - 20;

        using var typeface = SKTypeface.Default;
        using var measurePaint = new SKPaint { IsAntialias = true };

        // 迭代最多 2 次：初始测量 + 一次缩放后的重测量
        for (int pass = 0; pass < 2; pass++)
        {
            using var font = new SKFont(typeface, fontSize) { Embolden = cmd.Bold };

            float fontLineHeight = -font.Metrics.Ascent + font.Metrics.Descent + font.Metrics.Leading;
            float curLineHeight = fontLineHeight * cmd.LineSpacing;
            float curAscentAbove = -font.Metrics.Ascent;
            float curDescentBelow = font.Metrics.Descent;

            float curMaxLineWidth = 0;
            foreach (string line in lines)
            {
                float lw = string.IsNullOrEmpty(line) ? 0 : font.MeasureText(line, measurePaint);
                if (lw > curMaxLineWidth) curMaxLineWidth = lw;
            }

            bool scaled = false;

            // 宽度约束
            if (curMaxLineWidth > maxAvailableWidth && maxAvailableWidth > 0)
            {
                fontSize *= maxAvailableWidth / curMaxLineWidth;
                scaled = true;
            }

            // 高度约束（AutoFitFont 模式）
            if (cmd.AutoFitFont)
            {
                float blockHeight = CalcBlockHeight(lineCount, curAscentAbove, curDescentBelow, curLineHeight);
                if (blockHeight > maxAvailableHeight && maxAvailableHeight > 0)
                {
                    fontSize *= maxAvailableHeight / blockHeight;
                    scaled = true;
                }
            }

            fontSize = Math.Max(12, fontSize);

            if (!scaled)
            {
                // 无需缩放：此 pass 的测量即为最终值
                maxLineWidth = curMaxLineWidth;
                lineHeight = curLineHeight;
                ascentAbove = curAscentAbove;
                descentBelow = curDescentBelow;
                return fontSize;
            }
        }

        // 第二次 pass 的测量（缩放后重测）
        using (var finalFont = new SKFont(typeface, fontSize) { Embolden = cmd.Bold })
        {
            float fontLineHeight = -finalFont.Metrics.Ascent + finalFont.Metrics.Descent + finalFont.Metrics.Leading;
            lineHeight = fontLineHeight * cmd.LineSpacing;
            ascentAbove = -finalFont.Metrics.Ascent;
            descentBelow = finalFont.Metrics.Descent;

            maxLineWidth = 0;
            foreach (string line in lines)
            {
                float lw = string.IsNullOrEmpty(line) ? 0 : finalFont.MeasureText(line, measurePaint);
                if (lw > maxLineWidth) maxLineWidth = lw;
            }
        }

        return fontSize;
    }

    /// <summary>绘制调试角点标记</summary>
    private static void DrawCornerMarkers(SKCanvas canvas, SKRect rect, SKPaint paint)
    {
        float s = 5f; // 角点标记长度

        canvas.DrawLine(rect.Left, rect.Top, rect.Left + s, rect.Top, paint);
        canvas.DrawLine(rect.Left, rect.Top, rect.Left, rect.Top + s, paint);

        canvas.DrawLine(rect.Right - s, rect.Top, rect.Right, rect.Top, paint);
        canvas.DrawLine(rect.Right, rect.Top, rect.Right, rect.Top + s, paint);

        canvas.DrawLine(rect.Left, rect.Bottom - s, rect.Left, rect.Bottom, paint);
        canvas.DrawLine(rect.Left, rect.Bottom, rect.Left + s, rect.Bottom, paint);

        canvas.DrawLine(rect.Right - s, rect.Bottom, rect.Right, rect.Bottom, paint);
        canvas.DrawLine(rect.Right, rect.Bottom - s, rect.Right, rect.Bottom, paint);
    }

    /// <summary>解析 ARGB 十六进制颜色字符串</summary>
    internal static SKColor ParseColor(string hex, SKColor fallback)
    {
        hex = hex.TrimStart('#');

        if (hex.Length == 3)
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";

        byte a, r, g, b;

        if (hex.Length == 8)
        {
            a = Convert.ToByte(hex.Substring(0, 2), 16);
            r = Convert.ToByte(hex.Substring(2, 2), 16);
            g = Convert.ToByte(hex.Substring(4, 2), 16);
            b = Convert.ToByte(hex.Substring(6, 2), 16);
        }
        else if (hex.Length == 6)
        {
            a = 255;
            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
        }
        else
        {
            return fallback;
        }

        return new SKColor(r, g, b, a);
    }

    /// <summary>解析水印模板，替换 EXIF 占位符</summary>
    private static string ParseTemplate(string template, ExifInfo exif)
    {
        string result = template;
        foreach (var kvp in exif.GetTemplateReplacements())
        {
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value ?? "N/A");
        }
        return result;
    }

    #endregion
}
