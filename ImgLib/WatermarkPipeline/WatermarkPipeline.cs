using ImgLib.Models;
using SkiaSharp;

namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 水印绘制管线 —— 管理命令列表、提供流式构建 API、协调执行。
/// </summary>
public class WatermarkPipelineRunner
{
    private readonly List<IWatermarkCommand> _commands = new();
    private int _orderCounter;

    /// <summary>管线中的命令列表（只读）</summary>
    public IReadOnlyList<IWatermarkCommand> Commands => _commands.AsReadOnly();

    // ═══════════════════════════════════════════════
    //  执行
    // ═══════════════════════════════════════════════

    /// <summary>
    /// 按 <see cref="IWatermarkCommand.Order"/> 升序执行管线中所有已启用的命令。
    /// </summary>
    /// <param name="ctx">渲染上下文（含画布、位图、EXIF 等）</param>
    /// <param name="visitor">渲染访问者（默认使用 SkiaSharp 渲染器）</param>
    public void Execute(WatermarkRenderContext ctx, IWatermarkCommandVisitor? visitor = null)
    {
        visitor ??= new SkiaWatermarkRenderer();

        foreach (var cmd in _commands
            .Where(c => c.Enabled)
            .OrderBy(c => c.Order))
        {
            cmd.Accept(visitor, ctx);
        }
    }

    // ═══════════════════════════════════════════════
    //  命令管理
    // ═══════════════════════════════════════════════

    /// <summary>添加命令到管线末尾</summary>
    public WatermarkPipelineRunner AddCommand(IWatermarkCommand command)
    {
        if (command.Order <= 0)
            command.Order = ++_orderCounter;
        else
            _orderCounter = Math.Max(_orderCounter, command.Order);

        _commands.Add(command);
        return this;
    }

    /// <summary>移除命令</summary>
    public WatermarkPipelineRunner RemoveCommand(IWatermarkCommand command)
    {
        _commands.Remove(command);
        return this;
    }

    /// <summary>清空所有命令</summary>
    public WatermarkPipelineRunner Clear()
    {
        _commands.Clear();
        _orderCounter = 0;
        return this;
    }

    // ═══════════════════════════════════════════════
    //  流式构建 API
    // ═══════════════════════════════════════════════

    /// <summary>添加模糊背景</summary>
    public WatermarkPipelineRunner AddBlurBackground(float sigma = 25f)
        => AddCommand(new BlurBackgroundCommand(sigma));

    /// <summary>添加中央图片圆角阴影</summary>
    public WatermarkPipelineRunner AddImageShadow(
        float offsetX = 50f, float offsetY = 50f,
        float sigma = 25f, string colorHex = "#80000000",
        float cornerRadius = 45f)
        => AddCommand(new ImageShadowCommand(offsetX, offsetY, sigma, colorHex, cornerRadius));

    /// <summary>保存画布状态（用于裁剪等操作前后）</summary>
    public WatermarkPipelineRunner AddSaveCanvas()
        => AddCommand(new SaveCanvasCommand());

    /// <summary>恢复画布状态（与 <see cref="AddSaveCanvas"/> 配对）</summary>
    public WatermarkPipelineRunner AddRestoreCanvas()
        => AddCommand(new RestoreCanvasCommand());

    /// <summary>裁剪到圆角矩形区域</summary>
    public WatermarkPipelineRunner AddClipRoundedRect(float cornerRadius = 45f)
        => AddCommand(new ClipRoundedRectCommand(cornerRadius));

    /// <summary>在裁剪区域中绘制原图</summary>
    public WatermarkPipelineRunner AddDrawImage()
        => AddCommand(new DrawImageCommand());

    /// <summary>添加文本水印</summary>
    public WatermarkPipelineRunner AddText(TextWatermarkCommand cmd)
        => AddCommand(cmd);

    /// <summary>添加文本水印（快捷重载）</summary>
    public WatermarkPipelineRunner AddText(
        string template,
        string colorHex = "#FFFFFFFF",
        float fontSizeRatio = 0.03f,
        bool bold = true,
        float lineSpacing = 1.2f,
        string horizontalAlignment = "Center",
        float verticalPosition = 0.5f)
        => AddCommand(new TextWatermarkCommand(template)
        {
            ColorHex = colorHex,
            FontSizeRatio = fontSizeRatio,
            Bold = bold,
            LineSpacing = lineSpacing,
            HorizontalAlignment = horizontalAlignment,
            VerticalPosition = verticalPosition,
        });

    /// <summary>添加调试边框</summary>
    public WatermarkPipelineRunner AddDebugBorder(
        string colorHex = "#00FF00", float strokeWidth = 2f)
        => AddCommand(new DebugBorderCommand(colorHex, strokeWidth));

    // ═══════════════════════════════════════════════
    //  静态工厂方法
    // ═══════════════════════════════════════════════

    /// <summary>从 ImageGenerateOption 创建默认管线</summary>
    public static WatermarkPipelineRunner FromOptions(ImageGenerateOption options)
    {
        var pipeline = new WatermarkPipelineRunner();

        // 1. 模糊背景
        if (options.BlurSigma > 0)
            pipeline.AddBlurBackground(options.BlurSigma);

        // 2. 中央图阴影
        pipeline.AddImageShadow(
            options.ShadowOffsetX, options.ShadowOffsetY,
            options.ShadowSigma, "#80000000",
            options.CornerRadius);

        // 3. 裁剪 + 绘制中央图
        pipeline.AddSaveCanvas();
        pipeline.AddClipRoundedRect(options.CornerRadius);
        pipeline.AddDrawImage();
        pipeline.AddRestoreCanvas();

        // 4. 文本水印（支持多水印列表）
        var effectiveTexts = options.GetEffectiveWatermarkTexts();
        bool anyShowBorder = false;
        foreach (var textItem in effectiveTexts)
        {
            if (string.IsNullOrWhiteSpace(textItem.Template))
                continue;

            pipeline.AddCommand(new TextWatermarkCommand(textItem.Template)
            {
                ColorHex = textItem.ColorHex,
                FontSizeRatio = textItem.FontSizeRatio,
                Bold = textItem.Bold,
                LineSpacing = textItem.LineSpacing,
                AutoFitFont = textItem.AutoFitFont,
                VerticalPosition = textItem.VerticalPosition,
                HorizontalAlignment = textItem.HorizontalAlignment,
                ShadowOffsetX = textItem.ShadowOffsetX,
                ShadowOffsetY = textItem.ShadowOffsetY,
                ShadowSigma = textItem.ShadowSigma,
                ShadowColorHex = textItem.ShadowColorHex,
                ShowBorder = textItem.ShowBorder,
                BorderColorHex = textItem.BorderColorHex,
                BorderWidth = textItem.BorderWidth,
            });

            if (textItem.ShowBorder)
                anyShowBorder = true;
        }

        // 5. 调试边框（任一文本水印开启调试时启用）
        // 注意：多水印模式下，每个文本水印的调试边框在 VisitTextWatermark 中绘制；
        // DebugBorderCommand 仅作为兜底，遍历 TextBlockLayouts 统一绘制。
        if (anyShowBorder)
        {
            pipeline.AddDebugBorder(options.WatermarkBorderColor, options.WatermarkBorderWidth);
        }

        return pipeline;
    }
}
