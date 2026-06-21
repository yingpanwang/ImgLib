namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 绘制多行文本水印，支持 EXIF 模板变量、自动缩放、阴影和调试边框。
/// </summary>
public class TextWatermarkCommand : IWatermarkCommand
{
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 水印模板文本
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// 文本颜色（ARGB 十六进制如 "#FFFFFFFF"）
    /// </summary>
    public string ColorHex { get; set; } = "#FFFFFFFF";

    /// <summary>
    /// 字体大小 = 图片高度 × 比例
    /// </summary>
    public float FontSizeRatio { get; set; } = 0.03f;

    /// <summary>
    /// 是否加粗
    /// </summary>
    public bool Bold { get; set; } = true;

    /// <summary>
    /// 行间距系数（1.0 = 标准）
    /// </summary>
    public float LineSpacing { get; set; } = 1.2f;

    /// <summary>
    /// 文本块超出可用区域时自动缩小字体
    /// </summary>
    public bool AutoFitFont { get; set; }

    /// <summary>
    /// 垂直位置（0 = 底部，1 = 顶部）
    /// </summary>
    public float VerticalPosition { get; set; } = 0.5f;

    /// <summary>
    /// 水平对齐："Left" | "Center" | "Right"
    /// </summary>
    public string HorizontalAlignment { get; set; } = "Center";

    public bool UseAdvancedPositioning { get; set; }
    public float PositionX { get; set; } = 0.5f;
    public float PositionY { get; set; } = 0.5f;
    public string AdvancedHAlign { get; set; } = "Center";
    public string AdvancedVAlign { get; set; } = "Bottom";
    public bool Repeat { get; set; }
    public float RepeatSpacingX { get; set; } = 0.3f;
    public float RepeatSpacingY { get; set; } = 0.2f;
    public float RepeatAngle { get; set; }
    public bool RelativeToCanvas { get; set; }

    // ─── 投影参数 ───
    public float ShadowOffsetX { get; set; } = 2f;
    public float ShadowOffsetY { get; set; } = 2f;
    public float ShadowSigma { get; set; } = 5f;
    public string ShadowColorHex { get; set; } = "#80000000";

    // ─── 调试边框 ───
    public bool ShowBorder { get; set; }
    public string BorderColorHex { get; set; } = "#00FF00";
    public float BorderWidth { get; set; } = 2f;

    public TextWatermarkCommand(string template = "")
        => Template = template;

    public void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx)
        => visitor.VisitTextWatermark(this, ctx);
}
