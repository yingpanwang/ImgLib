using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ImgLib.UI.Services;

namespace ImgLib.UI.Models;

/// <summary>
/// 界面设置类，用于绑定 UI 并触发属性变化通知
/// </summary>
public partial class WatermarkSettings : ObservableObject
{
    // ═══ 多水印文本列表 ═══
    [ObservableProperty]
    public partial ObservableCollection<WatermarkTextItemSettings> WatermarkTextItems { get; set; } = new() { new WatermarkTextItemSettings() };

    // ═══ 图像处理参数 ═══
    [ObservableProperty]
    public partial float Scale { get; set; } = 0.89f;

    [ObservableProperty]
    private float _cornerRadius = 45f;

    [ObservableProperty]
    public partial float BlurSigma { get; set; } = 25f;

    [ObservableProperty]
    public partial float ShadowOffsetX { get; set; } = 50f;

    [ObservableProperty]
    public partial float ShadowOffsetY { get; set; } = 50f;

    [ObservableProperty]
    public partial float ShadowSigma { get; set; } = 25f;

    /// <summary>
    /// 从 ImageGenerateOption 复制值
    /// </summary>
    public void FromImageGenerateOption(ImageGenerateOption option)
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
    public ImageGenerateOption ToImageGenerateOption()
    {
        var systemSettings = SystemSettingsService.Current;
        var previewSettings = systemSettings.PreviewSettings;

        var option = new ImageGenerateOption(Scale)
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
}
