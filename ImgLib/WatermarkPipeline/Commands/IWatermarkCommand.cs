namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 水印管线中的单个绘制命令。
/// 命令是纯数据对象，不含任何渲染逻辑 —— 渲染由 <see cref="IWatermarkCommandVisitor"/> 完成。
/// </summary>
public interface IWatermarkCommand
{
    /// <summary>绘制顺序（升序执行）</summary>
    int Order { get; set; }

    /// <summary>是否参与本次绘制</summary>
    bool Enabled { get; }

    /// <summary>接受访问者，完成双分派到对应的 Visit 方法</summary>
    void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx);
}
