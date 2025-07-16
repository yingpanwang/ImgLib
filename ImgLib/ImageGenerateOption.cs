namespace ImgLib;

public record ImageGenerateOption
{
    public ImageGenerateOption(
        float scale,
        float cornerRadius = 45f,
        float blurSigma = 25f,
        float shadowOffsetX = 50f,
        float shadowOffsetY = 50f,
        float shadowSigma = 25f)
    {
        Scale = scale;
        CornerRadius = cornerRadius;
        BlurSigma = blurSigma;
        ShadowOffsetX = shadowOffsetX;
        ShadowOffsetY = shadowOffsetY;
        ShadowSigma = shadowSigma;
    }


    public float Scale { get; set; }
    public float CornerRadius { get; set; }
    public float BlurSigma { get; set; }
    public float ShadowOffsetX { get; set; }
    public float ShadowOffsetY { get; set; }
    public float ShadowSigma { get; set; }
}

