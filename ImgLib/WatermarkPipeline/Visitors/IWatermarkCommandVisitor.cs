namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 水印命令访问者 —— 为每种命令类型定义对应的 Visit 方法。
/// 实现此接口以提供具体的渲染后端（如 SkiaSharp、System.Drawing 等）。
/// </summary>
public interface IWatermarkCommandVisitor
{
    void VisitBlurBackground(BlurBackgroundCommand cmd, WatermarkRenderContext ctx);
    void VisitImageShadow(ImageShadowCommand cmd, WatermarkRenderContext ctx);
    void VisitSaveCanvas(SaveCanvasCommand cmd, WatermarkRenderContext ctx);
    void VisitRestoreCanvas(RestoreCanvasCommand cmd, WatermarkRenderContext ctx);
    void VisitClipRoundedRect(ClipRoundedRectCommand cmd, WatermarkRenderContext ctx);
    void VisitDrawImage(DrawImageCommand cmd, WatermarkRenderContext ctx);
    void VisitTextWatermark(TextWatermarkCommand cmd, WatermarkRenderContext ctx);
    void VisitDebugBorder(DebugBorderCommand cmd, WatermarkRenderContext ctx);
}
