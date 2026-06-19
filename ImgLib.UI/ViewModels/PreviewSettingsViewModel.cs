namespace ImgLib.UI.ViewModels;

/// <summary>
/// 预览设置视图模型，继承 <see cref="ViewModelBase"/> 以支持 Avalonia 双向绑定。
/// </summary>
public partial class PreviewSettingsViewModel : ViewModelBase
{
    /// <summary>预览时显示 RGB 直方图</summary>
    [ObservableProperty]
    public partial bool ShowHistogram { get; set; } = false;

    /// <summary>参数变化后自动刷新预览图像</summary>
    [ObservableProperty]
    public partial bool AutoPreview { get; set; } = false;

    /// <summary>自动预览触发间隔（毫秒）</summary>
    [ObservableProperty]
    public partial int AutoPreviewIntervalMs { get; set; } = 300;

    /// <summary>启用预览降采样</summary>
    [ObservableProperty]
    public partial bool EnablePreviewDownsampling { get; set; } = true;

    /// <summary>降采样模式：true=按百分比，false=按固定像素值</summary>
    [ObservableProperty]
    public partial bool UsePreviewPercentMode { get; set; } = false;

    /// <summary>预览降采样最大边长（固定像素模式）</summary>
    [ObservableProperty]
    public partial int PreviewMaxDimension { get; set; } = 1200;

    /// <summary>预览降采样百分比（百分比模式）</summary>
    [ObservableProperty]
    public partial float PreviewMaxPercent { get; set; } = 50f;

    /// <summary>
    /// 从数据模型填充视图模型。
    /// </summary>
    public void FromModel(PreviewSettings model)
    {
        ShowHistogram = model.ShowHistogram;
        AutoPreview = model.AutoPreview;
        AutoPreviewIntervalMs = model.AutoPreviewIntervalMs;
        EnablePreviewDownsampling = model.EnablePreviewDownsampling;
        UsePreviewPercentMode = model.UsePreviewPercentMode;
        PreviewMaxDimension = model.PreviewMaxDimension;
        PreviewMaxPercent = model.PreviewMaxPercent;
    }

    /// <summary>
    /// 将视图模型的值写入数据模型。
    /// </summary>
    public void ApplyTo(PreviewSettings model)
    {
        model.ShowHistogram = ShowHistogram;
        model.AutoPreview = AutoPreview;
        model.AutoPreviewIntervalMs = AutoPreviewIntervalMs;
        model.EnablePreviewDownsampling = EnablePreviewDownsampling;
        model.UsePreviewPercentMode = UsePreviewPercentMode;
        model.PreviewMaxDimension = PreviewMaxDimension;
        model.PreviewMaxPercent = PreviewMaxPercent;
    }

    /// <summary>
    /// 从视图模型创建新的数据模型实例。
    /// </summary>
    public PreviewSettings ToModel()
    {
        var model = new PreviewSettings();
        ApplyTo(model);
        return model;
    }
}
