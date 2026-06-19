using Avalonia.Platform.Storage;

namespace ImgLib.UI.ViewModels;

public partial class OpenFolderDialogViewModel : ViewModelBase
{
    private readonly IStorageProvider _storageProvider;
    /// <summary>防止 OnFolderPathChanged 和 OnDisplayFolderPathChanged 互相触发时形成循环</summary>
    private bool _suppressTruncation;

    public OpenFolderDialogViewModel(IStorageProvider storageProvider)
    {
        _storageProvider = storageProvider;
    }

    /// <summary>
    /// 用户选择或输入的完整文件夹路径（真实值）
    /// </summary>
    [ObservableProperty]
    public partial string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// 用于 TextBox 显示的缩略路径。超长时自动截断，中间用 "..." 代替。
    /// </summary>
    [ObservableProperty]
    public partial string DisplayFolderPath { get; set; } = string.Empty;

    /// <summary>
    /// 用户点击确认后，关闭窗口前由 Window 设置
    /// </summary>
    public bool Confirmed { get; set; }

    // ═══════════════════════════════════════════════════════════════
    //  聚焦 / 失焦（由 View 层调用）
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// TextBox 获得焦点时展开完整路径，方便用户编辑
    /// </summary>
    public void ExpandForEditing()
    {
        if (!string.IsNullOrEmpty(FolderPath))
        {
            _suppressTruncation = true;
            DisplayFolderPath = FolderPath;
            _suppressTruncation = false;
        }
    }

    /// <summary>
    /// TextBox 失去焦点时缩略显示
    /// </summary>
    public void TruncateForDisplay()
    {
        _suppressTruncation = true;
        DisplayFolderPath = TruncatePath(FolderPath);
        _suppressTruncation = false;
    }

    // ═══════════════════════════════════════════════════════════════
    //  属性变更回调
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// FolderPath 变化时（来自浏览按钮或代码设置）→ 自动缩略到 DisplayFolderPath
    /// </summary>
    partial void OnFolderPathChanged(string oldValue, string newValue)
    {
        if (_suppressTruncation) return;
        if (oldValue == newValue) return;

        DisplayFolderPath = TruncatePath(newValue);
    }

    /// <summary>
    /// DisplayFolderPath 变化时（用户在 TextBox 中手动输入）→ 同步到 FolderPath
    /// </summary>
    partial void OnDisplayFolderPathChanged(string oldValue, string newValue)
    {
        if (_suppressTruncation) return;
        if (oldValue == newValue) return;

        _suppressTruncation = true;
        FolderPath = newValue;
        _suppressTruncation = false;
    }

    // ═══════════════════════════════════════════════════════════════
    //  命令
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 点击文件夹图标 → 打开系统文件夹选择对话框
    /// </summary>
    [RelayCommand]
    public async Task BrowseFolder()
    {
        var folders = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "选择图片目录",
        });

        if (folders == null || folders.Count == 0)
            return;

        FolderPath = folders[0].Path.LocalPath;
    }

    /// <summary>
    /// 确认：验证路径不为空且是有效目录
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        if (string.IsNullOrWhiteSpace(FolderPath))
            return;

        if (!Directory.Exists(FolderPath))
            return;

        Confirmed = true;
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        Confirmed = false;
    }

    // ═══════════════════════════════════════════════════════════════
    //  路径缩略工具
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// 将长路径缩略为 "C:\开头\...\末尾1\末尾2" 的格式。
    /// 短路径（≤50 字符）原样返回。
    /// </summary>
    private static string TruncatePath(string path, int maxLength = 50)
    {
        if (string.IsNullOrEmpty(path) || path.Length <= maxLength)
            return path;

        var parts = path.Split(Path.DirectorySeparatorChar);
        if (parts.Length <= 3)
            return path;

        // 保留 根（如 "C:"）+ 第一段 + "..." + 最后两段
        var root = parts[0];
        var first = parts[1];
        var last1 = parts[^1];
        var last2 = parts[^2];

        return $"{root}\\{first}\\...\\{last2}\\{last1}";
    }
}
