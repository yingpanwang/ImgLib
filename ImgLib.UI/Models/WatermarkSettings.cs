using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImgLib.UI.Models;

/// <summary>
/// 界面设置类，用于绑定 UI 并触发属性变化通知
/// </summary>
public class WatermarkSettings : INotifyPropertyChanged
{
    // ═══ 图像处理参数 ═══
    private float _scale = 0.89f;
    public float Scale
    {
        get => _scale;
        set { _scale = value; OnPropertyChanged(); }
    }

    private float _cornerRadius = 45f;
    public float CornerRadius
    {
        get => _cornerRadius;
        set { _cornerRadius = value; OnPropertyChanged(); }
    }

    private float _blurSigma = 25f;
    public float BlurSigma
    {
        get => _blurSigma;
        set { _blurSigma = value; OnPropertyChanged(); }
    }

    private float _shadowOffsetX = 50f;
    public float ShadowOffsetX
    {
        get => _shadowOffsetX;
        set { _shadowOffsetX = value; OnPropertyChanged(); }
    }

    private float _shadowOffsetY = 50f;
    public float ShadowOffsetY
    {
        get => _shadowOffsetY;
        set { _shadowOffsetY = value; OnPropertyChanged(); }
    }

    private float _shadowSigma = 25f;
    public float ShadowSigma
    {
        get => _shadowSigma;
        set { _shadowSigma = value; OnPropertyChanged(); }
    }

    // ═══ 水印文本参数 ═══
    private string _watermarkTemplate = "{Model} | {LensModel} | f/{FNumber} | ISO {ISO} | {ExposureTime}";
    public string WatermarkTemplate
    {
        get => _watermarkTemplate;
        set { _watermarkTemplate = value; OnPropertyChanged(); }
    }

    private string _watermarkColor = "#FFFFFF";
    public string WatermarkColor
    {
        get => _watermarkColor;
        set { _watermarkColor = value; OnPropertyChanged(); }
    }

    private float _watermarkFontSizeRatio = 0.03f;
    public float WatermarkFontSizeRatio
    {
        get => _watermarkFontSizeRatio;
        set { _watermarkFontSizeRatio = value; OnPropertyChanged(); }
    }

    private bool _watermarkBold = true;
    public bool WatermarkBold
    {
        get => _watermarkBold;
        set { _watermarkBold = value; OnPropertyChanged(); }
    }

    private float _watermarkLineSpacing = 1.2f;
    public float WatermarkLineSpacing
    {
        get => _watermarkLineSpacing;
        set { _watermarkLineSpacing = value; OnPropertyChanged(); }
    }

    private bool _watermarkAutoFitFont = false;
    public bool WatermarkAutoFitFont
    {
        get => _watermarkAutoFitFont;
        set { _watermarkAutoFitFont = value; OnPropertyChanged(); }
    }

    private float _watermarkShadowOffsetX = 2f;
    public float WatermarkShadowOffsetX
    {
        get => _watermarkShadowOffsetX;
        set { _watermarkShadowOffsetX = value; OnPropertyChanged(); }
    }

    private float _watermarkShadowOffsetY = 2f;
    public float WatermarkShadowOffsetY
    {
        get => _watermarkShadowOffsetY;
        set { _watermarkShadowOffsetY = value; OnPropertyChanged(); }
    }

    private float _watermarkShadowSigma = 5f;
    public float WatermarkShadowSigma
    {
        get => _watermarkShadowSigma;
        set { _watermarkShadowSigma = value; OnPropertyChanged(); }
    }

    private string _watermarkShadowColor = "#80000000";
    public string WatermarkShadowColor
    {
        get => _watermarkShadowColor;
        set { _watermarkShadowColor = value; OnPropertyChanged(); }
    }

    private float _watermarkVerticalPosition = 0.5f;
    public float WatermarkVerticalPosition
    {
        get => _watermarkVerticalPosition;
        set { _watermarkVerticalPosition = value; OnPropertyChanged(); }
    }

    private string _watermarkHorizontalAlignment = "Center";
    public string WatermarkHorizontalAlignment
    {
        get => _watermarkHorizontalAlignment;
        set { _watermarkHorizontalAlignment = value; OnPropertyChanged(); }
    }

    // ═══ 调试参数 ═══
    private bool _showWatermarkBorder = false;
    public bool ShowWatermarkBorder
    {
        get => _showWatermarkBorder;
        set { _showWatermarkBorder = value; OnPropertyChanged(); }
    }

    private string _watermarkBorderColor = "#00FF00";
    public string WatermarkBorderColor
    {
        get => _watermarkBorderColor;
        set { _watermarkBorderColor = value; OnPropertyChanged(); }
    }

    private float _watermarkBorderWidth = 2f;
    public float WatermarkBorderWidth
    {
        get => _watermarkBorderWidth;
        set { _watermarkBorderWidth = value; OnPropertyChanged(); }
    }

    // ═══ 预览相关参数 ═══
    private bool _enablePreviewDownsampling = true;
    public bool EnablePreviewDownsampling
    {
        get => _enablePreviewDownsampling;
        set { _enablePreviewDownsampling = value; OnPropertyChanged(); }
    }

    private bool _usePreviewPercentMode = false;
    public bool UsePreviewPercentMode
    {
        get => _usePreviewPercentMode;
        set { _usePreviewPercentMode = value; OnPropertyChanged(); }
    }

    private int _previewMaxDimension = 1200;
    public int PreviewMaxDimension
    {
        get => _previewMaxDimension;
        set { _previewMaxDimension = value; OnPropertyChanged(); }
    }

    private float _previewMaxPercent = 50f;
    public float PreviewMaxPercent
    {
        get => _previewMaxPercent;
        set { _previewMaxPercent = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// 从 ImageGenerateOption 复制值
    /// </summary>
    public void FromImageGenerateOption(ImgLib.ImageGenerateOption option)
    {
        Scale = option.Scale;
        CornerRadius = option.CornerRadius;
        BlurSigma = option.BlurSigma;
        ShadowOffsetX = option.ShadowOffsetX;
        ShadowOffsetY = option.ShadowOffsetY;
        ShadowSigma = option.ShadowSigma;
        WatermarkTemplate = option.WatermarkTemplate;
        WatermarkColor = option.WatermarkColor;
        WatermarkFontSizeRatio = option.WatermarkFontSizeRatio;
        WatermarkBold = option.WatermarkBold;
        WatermarkLineSpacing = option.WatermarkLineSpacing;
        WatermarkAutoFitFont = option.WatermarkAutoFitFont;
        WatermarkShadowOffsetX = option.WatermarkShadowOffsetX;
        WatermarkShadowOffsetY = option.WatermarkShadowOffsetY;
        WatermarkShadowSigma = option.WatermarkShadowSigma;
        WatermarkShadowColor = option.WatermarkShadowColor;
        WatermarkVerticalPosition = option.WatermarkVerticalPosition;
        WatermarkHorizontalAlignment = option.WatermarkHorizontalAlignment;
        ShowWatermarkBorder = option.ShowWatermarkBorder;
        WatermarkBorderColor = option.WatermarkBorderColor;
        WatermarkBorderWidth = option.WatermarkBorderWidth;
        EnablePreviewDownsampling = option.EnablePreviewDownsampling;
        UsePreviewPercentMode = option.UsePreviewPercentMode;
        PreviewMaxDimension = option.PreviewMaxDimension;
        PreviewMaxPercent = option.PreviewMaxPercent;
    }

    /// <summary>
    /// 转换为 ImageGenerateOption
    /// </summary>
    public ImgLib.ImageGenerateOption ToImageGenerateOption()
    {
        return new ImgLib.ImageGenerateOption(Scale)
        {
            CornerRadius = CornerRadius,
            BlurSigma = BlurSigma,
            ShadowOffsetX = ShadowOffsetX,
            ShadowOffsetY = ShadowOffsetY,
            ShadowSigma = ShadowSigma,
            WatermarkTemplate = WatermarkTemplate,
            WatermarkColor = WatermarkColor,
            WatermarkFontSizeRatio = WatermarkFontSizeRatio,
            WatermarkBold = WatermarkBold,
            WatermarkLineSpacing = WatermarkLineSpacing,
            WatermarkAutoFitFont = WatermarkAutoFitFont,
            WatermarkShadowOffsetX = WatermarkShadowOffsetX,
            WatermarkShadowOffsetY = WatermarkShadowOffsetY,
            WatermarkShadowSigma = WatermarkShadowSigma,
            WatermarkShadowColor = WatermarkShadowColor,
            WatermarkVerticalPosition = WatermarkVerticalPosition,
            WatermarkHorizontalAlignment = WatermarkHorizontalAlignment,
            ShowWatermarkBorder = ShowWatermarkBorder,
            WatermarkBorderColor = WatermarkBorderColor,
            WatermarkBorderWidth = WatermarkBorderWidth,
            EnablePreviewDownsampling = EnablePreviewDownsampling,
            UsePreviewPercentMode = UsePreviewPercentMode,
            PreviewMaxDimension = PreviewMaxDimension,
            PreviewMaxPercent = PreviewMaxPercent
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}