namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 保存当前画布状态（调用 <c>SKCanvas.Save()</c>），通常与 <see cref="RestoreCanvasCommand"/> 配对使用。
/// </summary>
public class SaveCanvasCommand : IWatermarkCommand
{
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;

    public void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx)
        => visitor.VisitSaveCanvas(this, ctx);
}
