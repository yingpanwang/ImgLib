namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 绘制模糊背景 —— 将原图全幅绘制并叠加高斯模糊滤镜。
/// </summary>
public class BlurBackgroundCommand : IWatermarkCommand
{
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>背景模糊 sigma 值（0 = 不模糊）</summary>
    public float Sigma { get; set; }

    public BlurBackgroundCommand(float sigma = 25f) => Sigma = sigma;

    public void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx)
        => visitor.VisitBlurBackground(this, ctx);
}
