namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 绘制调试边框及角点标记 —— 用于视觉调试文本水印的布局边界。
/// </summary>
public class DebugBorderCommand : IWatermarkCommand
{
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>边框颜色（ARGB 十六进制）</summary>
    public string ColorHex { get; set; } = "#00FF00";

    /// <summary>边框线宽（像素）</summary>
    public float StrokeWidth { get; set; } = 2f;

    public DebugBorderCommand(string colorHex = "#00FF00", float strokeWidth = 2f)
    {
        ColorHex = colorHex;
        StrokeWidth = strokeWidth;
    }

    public void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx)
        => visitor.VisitDebugBorder(this, ctx);
}
