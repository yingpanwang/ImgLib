namespace ImgLib.UI.Models;

/// <summary>
/// 水印设置文件条目
/// </summary>
public partial class WatermarkSettingFileEntry : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileName))]
    [NotifyPropertyChangedFor(nameof(DirectoryName))]
    private string _filePath = string.Empty;

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    /// 所在目录路径
    /// </summary>
    public string DirectoryName => Path.GetDirectoryName(FilePath) ?? string.Empty;

    /// <summary>
    /// 添加/更新时间
    /// </summary>
    public DateTime SavedAt { get; set; } = DateTime.Now;
}
