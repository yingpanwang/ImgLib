namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 将画布裁剪到圆角矩形区域（后续绘制仅在此区域内可见）。
/// 必须在 <see cref="SaveCanvasCommand"/> 之后、<see cref="RestoreCanvasCommand"/> 之前使用。
/// </summary>
public class ClipRoundedRectCommand : IWatermarkCommand
{
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>圆角半径（像素）</summary>
    public float CornerRadius { get; set; }

    public ClipRoundedRectCommand(float cornerRadius = 45f) => CornerRadius = cornerRadius;

    public void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx)
        => visitor.VisitClipRoundedRect(this, ctx);
}
