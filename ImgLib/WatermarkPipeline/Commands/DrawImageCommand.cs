namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 将原始图片绘制到目标矩形区域。
/// 通常放在 <see cref="ClipRoundedRectCommand"/> 之后以在圆角裁剪内绘制。
/// </summary>
public class DrawImageCommand : IWatermarkCommand
{
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;

    public DrawImageCommand() { }

    public void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx)
        => visitor.VisitDrawImage(this, ctx);
}
