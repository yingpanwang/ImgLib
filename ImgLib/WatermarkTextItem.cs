namespace ImgLib;

/// <summary>
/// 单个水印文本项，封装一条水印文本的全部属性。
/// 多个 WatermarkTextItem 组成多条独立水印，按列表顺序依次绘制（后者叠于前者之上）。
/// </summary>
public class WatermarkTextItem
{
    /// <summary>水印模板文本，支持 {Model}、{FNumber} 等 EXIF 占位符</summary>
    public string Template { get; set; } = "{Model} | {LensModel} | f/{FNumber} | ISO {ISO} | {ExposureTime}";

    /// <summary>文本颜色（ARGB 十六进制如 "#FFFFFFFF"）</summary>
    public string ColorHex { get; set; } = "#FFFFFF";

    /// <summary>字体大小 = 图片高度 × 此比例（0.01-0.1）</summary>
    public float FontSizeRatio { get; set; } = 0.03f;

    /// <summary>是否加粗</summary>
    public bool Bold { get; set; } = true;

    /// <summary>行间距系数（1.0=标准行距）</summary>
    public float LineSpacing { get; set; } = 1.2f;

    /// <summary>文本块超出可用区域时自动缩小字体</summary>
    public bool AutoFitFont { get; set; } = false;

    /// <summary>垂直位置（0=底部，1=顶部），相对于主图片下方区域</summary>
    public float VerticalPosition { get; set; } = 0.5f;

    /// <summary>水平对齐："Left" | "Center" | "Right"</summary>
    public string HorizontalAlignment { get; set; } = "Center";

    // ─── 投影参数 ───
    /// <summary>文字阴影 X 偏移（像素）</summary>
    public float ShadowOffsetX { get; set; } = 2f;

    /// <summary>文字阴影 Y 偏移（像素）</summary>
    public float ShadowOffsetY { get; set; } = 2f;

    /// <summary>文字阴影模糊半径</summary>
    public float ShadowSigma { get; set; } = 5f;

    /// <summary>文字阴影颜色（ARGB 十六进制）</summary>
    public string ShadowColorHex { get; set; } = "#80000000";

    // ─── 调试边框 ───
    /// <summary>是否显示水印边框（调试用）</summary>
    public bool ShowBorder { get; set; } = false;

    /// <summary>边框颜色（ARGB 十六进制）</summary>
    public string BorderColorHex { get; set; } = "#00FF00";

    /// <summary>边框宽度（像素）</summary>
    public float BorderWidth { get; set; } = 2f;

    /// <summary>
    /// 创建一个与默认值相同的浅拷贝。
    /// </summary>
    public WatermarkTextItem Clone()
    {
        return new WatermarkTextItem
        {
            Template = Template,
            ColorHex = ColorHex,
            FontSizeRatio = FontSizeRatio,
            Bold = Bold,
            LineSpacing = LineSpacing,
            AutoFitFont = AutoFitFont,
            VerticalPosition = VerticalPosition,
            HorizontalAlignment = HorizontalAlignment,
            ShadowOffsetX = ShadowOffsetX,
            ShadowOffsetY = ShadowOffsetY,
            ShadowSigma = ShadowSigma,
            ShadowColorHex = ShadowColorHex,
            ShowBorder = ShowBorder,
            BorderColorHex = BorderColorHex,
            BorderWidth = BorderWidth,
        };
    }
}
