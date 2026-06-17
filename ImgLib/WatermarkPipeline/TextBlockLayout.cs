using SkiaSharp;

namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 单个水印文本块的布局信息，由 <see cref="SkiaWatermarkRenderer"/> 在绘制时计算并缓存。
/// 供调试边框等后续命令使用。
/// </summary>
public class TextBlockLayout
{
    /// <summary>解析并拆分后的文本行</summary>
    public string[] TextLines { get; set; } = Array.Empty<string>();

    /// <summary>文本行数</summary>
    public int TextLineCount { get; set; }

    /// <summary>实际使用的字体大小</summary>
    public float FontSize { get; set; }

    /// <summary>最长行的宽度（像素）</summary>
    public float MaxLineWidth { get; set; }

    /// <summary>行高（含间距）</summary>
    public float LineHeight { get; set; }

    /// <summary>基线以上的字形高度</summary>
    public float AscentAbove { get; set; }

    /// <summary>基线以下的字形高度</summary>
    public float DescentBelow { get; set; }

    /// <summary>文本块总高度</summary>
    public float TotalBlockHeight { get; set; }

    /// <summary>首行基线 Y 坐标</summary>
    public float FirstLineBaselineY { get; set; }

    /// <summary>文本水平起始 X 坐标</summary>
    public float TextX { get; set; }

    /// <summary>水平对齐方式</summary>
    public SKTextAlign TextAlign { get; set; }

    /// <summary>边框矩形（调试用）</summary>
    public SKRect DebugBorderRect { get; set; }

    /// <summary>对应的命令 Order（用于关联）</summary>
    public int CommandOrder { get; set; }

    /// <summary>是否绘制调试边框</summary>
    public bool ShowBorder { get; set; }

    /// <summary>调试边框颜色（ARGB 十六进制）</summary>
    public string BorderColorHex { get; set; } = "#00FF00";

    /// <summary>调试边框宽度（像素）</summary>
    public float BorderWidth { get; set; } = 2f;
}
