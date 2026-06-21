// @CodeScene(disable:"Constructor Over-Injection")
namespace ImgLib;

public class ImageGenerateOption
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

    // ═══ 图像处理参数 ═══
    public float Scale { get; set; }
    public float CornerRadius { get; set; }
    public float BlurSigma { get; set; }
    public float ShadowOffsetX { get; set; }
    public float ShadowOffsetY { get; set; }
    public float ShadowSigma { get; set; }

    // ═══ 多水印文本列表（推荐使用） ═══
    /// <summary>
    /// 水印文本项列表。每个项代表一条独立的水印文本，按列表顺序依次绘制。
    /// 为空时自动使用旧的单水印属性（向后兼容）。
    /// </summary>
    public List<WatermarkTextItem> WatermarkTexts { get; set; } = [];

    /// <summary>
    /// 获取有效的水印文本项列表。
    /// </summary>
    public List<WatermarkTextItem> GetEffectiveWatermarkTexts()
    {
        return WatermarkTexts;
    }

    // ═══ 预览相关参数 ═══
    /// <summary>
    /// 是否启用预览降采样（默认启用）
    /// </summary>
    public bool EnablePreviewDownsampling { get; set; } = true;

    /// <summary>
    /// 预览降采样模式：true=按百分比，false=按固定像素值
    /// </summary>
    public bool UsePreviewPercentMode { get; set; } = false;

    /// <summary>
    /// 预览降采样最大边长（像素），默认 1200（仅在固定像素模式生效）
    /// </summary>
    public int PreviewMaxDimension { get; set; } = 1200;

    /// <summary>
    /// 预览降采样百分比（相对于原始图片最大边长），默认 50%（仅在百分比模式生效）
    /// </summary>
    public float PreviewMaxPercent { get; set; } = 50f;
}

