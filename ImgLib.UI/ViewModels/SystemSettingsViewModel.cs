using CommunityToolkit.Mvvm.ComponentModel;
using ImgLib.UI.Models;
using ImgLib.UI.Services;

namespace ImgLib.UI.ViewModels;

/// <summary>
/// 系统设置视图模型，继承 <see cref="ViewModelBase"/> 以支持 Avalonia 双向绑定。
/// 作为 <see cref="SystemSettings"/> 数据模型的视图适配层，支持相互转换。
/// </summary>
public partial class SystemSettingsViewModel : ViewModelBase
{
    // ═══════════════════════════════════════════
    //  系统设置
    // ═══════════════════════════════════════════

    /// <summary>默认输出格式索引: 0=JPEG, 1=PNG</summary>
    [ObservableProperty]
    public partial int DefaultOutputFormatIndex { get; set; }

    /// <summary>JPEG 输出质量 (1-100)</summary>
    [ObservableProperty]
    public partial int JpegQuality { get; set; } = 90;

    // ═══════════════════════════════════════════
    //  预览设置
    // ═══════════════════════════════════════════

    /// <summary>预览设置视图模型</summary>
    [ObservableProperty]
    public partial PreviewSettingsViewModel PreviewSettings { get; set; } = new();

    /// <summary>
    /// 从数据模型填充视图模型。
    /// </summary>
    public void FromModel(SystemSettings model)
    {
        DefaultOutputFormatIndex = model.DefaultOutputFormatIndex;
        JpegQuality = model.JpegQuality;
        PreviewSettings.FromModel(model.PreviewSettings);
    }

    /// <summary>
    /// 将视图模型的值写入数据模型。
    /// </summary>
    public void ApplyTo(SystemSettings model)
    {
        model.DefaultOutputFormatIndex = DefaultOutputFormatIndex;
        model.JpegQuality = JpegQuality;
        PreviewSettings.ApplyTo(model.PreviewSettings);
    }

    /// <summary>
    /// 从视图模型创建新的数据模型实例。
    /// </summary>
    public SystemSettings ToModel()
    {
        var model = new SystemSettings();
        ApplyTo(model);
        return model;
    }
}
