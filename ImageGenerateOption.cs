using ImgLib.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImgLib;

public class ImageGenerateOption : INotifyPropertyChanged
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
    private float _scale;
    public float Scale
    {
        get => _scale;
        set { _scale = value; OnPropertyChanged(); }
    }

    private float _cornerRadius;
    public float CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = value; OnPropertyChanged(); }
    }

    private float _blurSigma;
    public float BlurSigma
    {
        get => _blurSigma;
        set { _blurSigma = value; OnPropertyChanged(); }
    }

    private float _shadowOffsetX;
    public float ShadowOffsetX
    {
        get => _shadowOffsetX;
        set { _shadowOffsetX = value; OnPropertyChanged(); }
    }

    private float _shadowOffsetY;
    public float ShadowOffsetY
    {
        get => _shadowOffsetY;
        set { _shadowOffsetY = value; OnPropertyChanged(); }
    }

    private float _shadowSigma;
    public float ShadowSigma
    {
        get => _shadowSigma;
        set { _shadowSigma = value; OnPropertyChanged(); }
    }

    // ═══ 水印文本参数 ═══
    /// <summary>
    /// 水印文本模板，支持 EXIF 变量替换
    /// 可用变量: {相机型号}, {镜头型号}, {光圈}, {ISO}, {焦距}, {快门}, {时间}
    /// 示例: "{相机型号} | {镜头型号} | f/{光圈} | ISO {ISO} | {快门}"
    /// </summary>
    private string _watermarkTemplate = "{相机型号} | {镜头型号} | f/{光圈} | ISO {ISO} | {快门}";
    public string WatermarkTemplate
    {
        get => _watermarkTemplate;
        set { _watermarkTemplate = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印文本颜色（十六进制格式，如 #FFFFFF）
    /// </summary>
    private string _watermarkColor = "#FFFFFF";
    public string WatermarkColor
    {
        get => _watermarkColor;
        set { _watermarkColor = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印字体大小（相对于图片高度的百分比，0.01-0.1）
    /// </summary>
    private float _watermarkFontSizeRatio = 0.03f;
    public float WatermarkFontSizeRatio
    {
        get => _watermarkFontSizeRatio;
        set { _watermarkFontSizeRatio = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印是否加粗
    /// </summary>
    private bool _watermarkBold = true;
    public bool WatermarkBold
    {
        get => _watermarkBold;
        set { _watermarkBold = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印文字阴影偏移 X
    /// </summary>
    private float _watermarkShadowOffsetX = 2f;
    public float WatermarkShadowOffsetX
    {
        get => _watermarkShadowOffsetX;
        set { _watermarkShadowOffsetX = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印文字阴影偏移 Y
    /// </summary>
    private float _watermarkShadowOffsetY = 2f;
    public float WatermarkShadowOffsetY
    {
        get => _watermarkShadowOffsetY;
        set { _watermarkShadowOffsetY = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印文字阴影模糊半径
    /// </summary>
    private float _watermarkShadowSigma = 5f;
    public float WatermarkShadowSigma
    {
        get => _watermarkShadowSigma;
        set { _watermarkShadowSigma = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印文字阴影颜色（十六进制格式）
    /// </summary>
    private string _watermarkShadowColor = "#80000000";
    public string WatermarkShadowColor
    {
        get => _watermarkShadowColor;
        set { _watermarkShadowColor = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印垂直位置（相对于底部，0-1，0为底部，1为顶部）
    /// </summary>
    private float _watermarkVerticalPosition = 0.5f;
    public float WatermarkVerticalPosition
    {
        get => _watermarkVerticalPosition;
        set { _watermarkVerticalPosition = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印水平对齐（Left, Center, Right）
    /// </summary>
    private string _watermarkHorizontalAlignment = "Center";
    public string WatermarkHorizontalAlignment
    {
        get => _watermarkHorizontalAlignment;
        set { _watermarkHorizontalAlignment = value; OnPropertyChanged(); }
    }

    // ═══ 调试参数 ═══
    /// <summary>
    /// 是否显示水印边框（调试用）
    /// </summary>
    private bool _showWatermarkBorder = false;
    public bool ShowWatermarkBorder
    {
        get => _showWatermarkBorder;
        set { _showWatermarkBorder = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印边框颜色（十六进制格式）
    /// </summary>
    private string _watermarkBorderColor = "#00FF00";
    public string WatermarkBorderColor
    {
        get => _watermarkBorderColor;
        set { _watermarkBorderColor = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 水印边框宽度
    /// </summary>
    private float _watermarkBorderWidth = 2f;
    public float WatermarkBorderWidth
    {
        get => _watermarkBorderWidth;
        set { _watermarkBorderWidth = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
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