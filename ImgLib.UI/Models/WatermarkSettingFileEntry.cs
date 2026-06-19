using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace ImgLib.UI.Models;

/// <summary>
/// 水印设置文件条目，用于在列表中展示已保存的水印设置文件。
/// </summary>
public class WatermarkSettingFileEntry : INotifyPropertyChanged
{
    private string _filePath = string.Empty;

    /// <summary>文件完整路径</summary>
    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
            OnPropertyChanged(nameof(DirectoryName));
        }
    }

    /// <summary>文件名（不含路径）</summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>所在目录路径（用于 tooltip）</summary>
    public string DirectoryName => Path.GetDirectoryName(FilePath) ?? string.Empty;

    /// <summary>添加/更新时间</summary>
    public DateTime SavedAt { get; set; } = DateTime.Now;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
