namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 恢复最近一次 <see cref="SaveCanvasCommand"/> 保存的画布状态（调用 <c>SKCanvas.Restore()</c>）。
/// </summary>
public class RestoreCanvasCommand : IWatermarkCommand
{
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;

    public void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx)
        => visitor.VisitRestoreCanvas(this, ctx);
}
