using ImgLib.UI.ViewModels;

namespace ImgLib.UI.Models;


/// <summary>
/// 系统设置数据模型，用于 JSON 序列化/反序列化。
/// </summary>
public sealed class SystemSettings
{
    // ═══════════════════════════════════════════
    //  系统设置
    // ═══════════════════════════════════════════

    /// <summary>默认输出格式索引: 0=JPEG, 1=PNG</summary>
    public int DefaultOutputFormatIndex { get; set; } = 0;

    /// <summary>JPEG 输出质量 (1-100)</summary>
    public int JpegQuality { get; set; } = 90;

    // ═══════════════════════════════════════════
    //  预览设置
    // ═══════════════════════════════════════════

    /// <summary>预览相关设置</summary>
    public PreviewSettings PreviewSettings { get; set; } = new();

    /// <summary>
    /// 返回带有所有默认值的设置实例。
    /// </summary>
    public static SystemSettings Default => new();

    /// <summary>
    /// 转换为视图模型。
    /// </summary>
    public SystemSettingsViewModel ToViewModel()
    {
        var vm = new SystemSettingsViewModel();
        vm.FromModel(this);
        return vm;
    }
}

