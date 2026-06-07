using ImgLib.Models;

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

    // ═══ 水印文本参数 ═══
    /// <summary>
    /// 水印文本模板，支持 EXIF 变量替换
    /// 可用变量: {相机型号}, {镜头型号}, {光圈}, {ISO}, {焦距}, {快门}, {时间}
    /// 示例: "{相机型号} | {镜头型号} | f/{光圈} | ISO {ISO} | {快门}"
    /// </summary>
    public string WatermarkTemplate { get; set; } = "{相机型号} | {镜头型号} | f/{光圈} | ISO {ISO} | {快门}";

    /// <summary>
    /// 水印文本颜色（十六进制格式，如 #FFFFFF）
    /// </summary>
    public string WatermarkColor { get; set; } = "#FFFFFF";

    /// <summary>
    /// 水印字体大小（相对于图片高度的百分比，0.01-0.1）
    /// </summary>
    public float WatermarkFontSizeRatio { get; set; } = 0.03f;

    /// <summary>
    /// 水印是否加粗
    /// </summary>
    public bool WatermarkBold { get; set; } = true;

    /// <summary>
    /// 水印行间距系数（1.0=标准行距，>1.0 增大间距，<1.0 缩小间距）
    /// </summary>
    public float WatermarkLineSpacing { get; set; } = 1.2f;

    /// <summary>
    /// 是否启用自动缩放字体以适应水印区域（当文本块高度超出可用区域时自动缩小）
    /// </summary>
    public bool WatermarkAutoFitFont { get; set; } = false;

    /// <summary>
    /// 水印文字阴影偏移 X
    /// </summary>
    public float WatermarkShadowOffsetX { get; set; } = 2f;

    /// <summary>
    /// 水印文字阴影偏移 Y
    /// </summary>
    public float WatermarkShadowOffsetY { get; set; } = 2f;

    /// <summary>
    /// 水印文字阴影模糊半径
    /// </summary>
    public float WatermarkShadowSigma { get; set; } = 5f;

    /// <summary>
    /// 水印文字阴影颜色（十六进制格式）
    /// </summary>
    public string WatermarkShadowColor { get; set; } = "#80000000";

    /// <summary>
    /// 水印垂直位置（相对于底部，0-1，0为底部，1为顶部）
    /// </summary>
    public float WatermarkVerticalPosition { get; set; } = 0.5f;

    /// <summary>
    /// 水印水平对齐（Left, Center, Right）
    /// </summary>
    public string WatermarkHorizontalAlignment { get; set; } = "Center";

    // ═══ 调试参数 ═══
    /// <summary>
    /// 是否显示水印边框（调试用）
    /// </summary>
    public bool ShowWatermarkBorder { get; set; } = false;

    /// <summary>
    /// 水印边框颜色（十六进制格式）
    /// </summary>
    public string WatermarkBorderColor { get; set; } = "#00FF00";

    /// <summary>
    /// 水印边框宽度
    /// </summary>
    public float WatermarkBorderWidth { get; set; } = 2f;

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

public static class ImageGenerateOptionExtensions
{
    /// <summary>
    /// 解析水印模板，替换 EXIF 变量
    /// </summary>
    public static string ParseWatermarkTemplate(this ImageGenerateOption option, ExifInfo? exifInfo)
    {
        if (exifInfo == null)
            return option.WatermarkTemplate;

        string result = option.WatermarkTemplate;

        // EXIF 变量映射
        var replacements = new Dictionary<string, string?>
        {
            { "{相机型号}", exifInfo.Model },
            { "{镜头型号}", exifInfo.LensModel },
            { "{光圈}", exifInfo.FNumber },
            { "{ISO}", exifInfo.ISO },
            { "{焦距}", exifInfo.FocalLength },
            { "{等效焦距}", exifInfo.FocalLengthIn35mmFormat },
            { "{快门}", exifInfo.ExposureTime },
            { "{时间}", exifInfo.DateTimeOriginal },
            { "{曝光补偿}", exifInfo.ExposureCompensation },
            { "{白平衡}", exifInfo.WhiteBalance },
            { "{拍摄模式}", exifInfo.ExposureProgram },
            { "{测光模式}", exifInfo.MeteringMode },
            { "{制造商}", exifInfo.Make },
            { "{镜头制造商}", exifInfo.LensMake },
        };

        foreach (var kvp in replacements)
        {
            result = result.Replace(kvp.Key, kvp.Value ?? "N/A");
        }

        return result;
    }
}