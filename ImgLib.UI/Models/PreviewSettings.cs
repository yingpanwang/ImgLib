using ImgLib.UI.ViewModels;
namespace ImgLib.UI.Models;

/// <summary>
/// 预览设置数据模型，用于 JSON 序列化/反序列化。
/// </summary>
public sealed class PreviewSettings
{
    /// <summary>预览时显示 RGB 直方图</summary>
    public bool ShowHistogram { get; set; } = false;

    /// <summary>参数变化后自动刷新预览图像</summary>
    public bool AutoPreview { get; set; } = false;

    /// <summary>自动预览触发间隔（毫秒）</summary>
    public int AutoPreviewIntervalMs { get; set; } = 300;

    /// <summary>启用预览降采样</summary>
    public bool EnablePreviewDownsampling { get; set; } = true;

    /// <summary>降采样模式：true=按百分比，false=按固定像素值</summary>
    public bool UsePreviewPercentMode { get; set; } = false;

    /// <summary>预览降采样最大边长（固定像素模式）</summary>
    public int PreviewMaxDimension { get; set; } = 1200;

    /// <summary>预览降采样百分比（百分比模式）</summary>
    public float PreviewMaxPercent { get; set; } = 50f;

    /// <summary>
    /// 转换为视图模型。
    /// </summary>
    public PreviewSettingsViewModel ToViewModel()
    {
        var vm = new PreviewSettingsViewModel();
        vm.FromModel(this);
        return vm;
    }
}

