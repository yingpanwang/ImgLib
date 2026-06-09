namespace ImgLib.WatermarkPipeline;

/// <summary>
/// 为中央图片绘制圆角矩形路径阴影（使用 MaskFilter 模糊蒙版）。
/// </summary>
public class ImageShadowCommand : IWatermarkCommand
{
    public int Order { get; set; }
    public bool Enabled { get; set; } = true;

    /// <summary>阴影水平偏移（像素）</summary>
    public float OffsetX { get; set; }

    /// <summary>阴影垂直偏移（像素）</summary>
    public float OffsetY { get; set; }

    /// <summary>阴影模糊半径</summary>
    public float Sigma { get; set; }

    /// <summary>阴影颜色（ARGB 十六进制字符串，如 "#80000000"）</summary>
    public string ColorHex { get; set; }

    /// <summary>圆角半径（像素）</summary>
    public float CornerRadius { get; set; }

    public ImageShadowCommand(
        float offsetX = 50f,
        float offsetY = 50f,
        float sigma = 25f,
        string colorHex = "#80000000",
        float cornerRadius = 45f)
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
        Sigma = sigma;
        ColorHex = colorHex;
        CornerRadius = cornerRadius;
    }

    public void Accept(IWatermarkCommandVisitor visitor, WatermarkRenderContext ctx)
        => visitor.VisitImageShadow(this, ctx);
}
