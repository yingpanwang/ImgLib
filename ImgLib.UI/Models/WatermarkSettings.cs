using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImgLib.UI.Services;

namespace ImgLib.UI.Models;

/// <summary>
/// 界面设置类，用于绑定 UI 并触发属性变化通知
/// </summary>
public class WatermarkSettings : INotifyPropertyChanged
{
    // ═══ 多水印文本列表 ═══
    private ObservableCollection<WatermarkTextItemSettings> _watermarkTextItems = new() { new WatermarkTextItemSettings() };
    public ObservableCollection<WatermarkTextItemSettings> WatermarkTextItems
    {
        get => _watermarkTextItems;
        set { _watermarkTextItems = value; OnPropertyChanged(); }
    }

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

    // // ═══ 预览相关参数 ═══
    // private bool _enablePreviewDownsampling = true;
    // public bool EnablePreviewDownsampling
    // {
    //     get => _enablePreviewDownsampling;
    //     set { _enablePreviewDownsampling = value; OnPropertyChanged(); }
    // }

    // private bool _usePreviewPercentMode = false;
    // public bool UsePreviewPercentMode
    // {
    //     get => _usePreviewPercentMode;
    //     set { _usePreviewPercentMode = value; OnPropertyChanged(); }
    // }

    // private int _previewMaxDimension = 1200;
    // public int PreviewMaxDimension
    // {
    //     get => _previewMaxDimension;
    //     set { _previewMaxDimension = value; OnPropertyChanged(); }
    // }

    // private float _previewMaxPercent = 50f;
    // public float PreviewMaxPercent
    // {
    //     get => _previewMaxPercent;
    //     set { _previewMaxPercent = value; OnPropertyChanged(); }
    // }

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

        // 同步多水印列表
        WatermarkTextItems.Clear();
        foreach (var item in option.WatermarkTexts)
        {
            var settings = new WatermarkTextItemSettings();
            settings.FromWatermarkTextItem(item);
            WatermarkTextItems.Add(settings);
        }

        // 确保至少有一个默认水印项
        if (WatermarkTextItems.Count == 0)
        {
            WatermarkTextItems.Add(new WatermarkTextItemSettings());
        }
    }

    /// <summary>
    /// 转换为 ImageGenerateOption
    /// </summary>
    public ImgLib.ImageGenerateOption ToImageGenerateOption()
    {
        var systemSettings = SystemSettingsService.Current;
        var previewSettings = systemSettings.PreviewSettings;

        var option = new ImgLib.ImageGenerateOption(Scale)
        {
            CornerRadius = CornerRadius,
            BlurSigma = BlurSigma,
            ShadowOffsetX = ShadowOffsetX,
            ShadowOffsetY = ShadowOffsetY,
            ShadowSigma = ShadowSigma,
            EnablePreviewDownsampling = previewSettings.EnablePreviewDownsampling,
            UsePreviewPercentMode = previewSettings.UsePreviewPercentMode,
            PreviewMaxDimension = previewSettings.PreviewMaxDimension,
            PreviewMaxPercent = previewSettings.PreviewMaxPercent
        };

        // 同步多水印列表
        option.WatermarkTexts.Clear();
        foreach (var item in WatermarkTextItems)
        {
            option.WatermarkTexts.Add(item.ToWatermarkTextItem());
        }

        return option;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}