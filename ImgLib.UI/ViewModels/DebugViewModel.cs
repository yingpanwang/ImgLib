namespace ImgLib.UI.ViewModels;

public partial class DebugViewModel : ViewModelBase
{
    public string Title => "调试面板";

    public string AppVersion => "1.0.0";

    public string RuntimeVersion => Environment.Version.ToString();

    public string OSVersion => Environment.OSVersion.ToString();

    public string ProcessMemory => $"{(Process.GetCurrentProcess().WorkingSet64 / 1024.0 / 1024.0):F1} MB";

    public string ThumbnailCacheSize
    {
        get
        {
            var size = ThumbnailCacheService.GetCacheSize();
            return size switch
            {
                0 => "空",
                < 1024 => $"{size} B",
                < 1024 * 1024 => $"{size / 1024.0:F1} KB",
                _ => $"{size / 1024.0 / 1024.0:F1} MB"
            };
        }
    }

    public string ThumbnailCacheDir => ThumbnailCacheService.CacheDirectory;

    public string SkiaSharpVersion => typeof(SKBitmap).Assembly.GetName().Version?.ToString() ?? "未知";

    public string ProcessorCount => Environment.ProcessorCount.ToString();

    public string WorkingDirectory => Environment.CurrentDirectory;

    [RelayCommand]
    private void Refresh()
    {
        OnPropertyChanged(nameof(ProcessMemory));
        OnPropertyChanged(nameof(ThumbnailCacheSize));
    }

    [RelayCommand]
    private void ClearThumbnailCache()
    {
        try
        {
            var dir = ThumbnailCacheService.CacheDirectory;
            if (Directory.Exists(dir))
            {
                foreach (var file in Directory.EnumerateFiles(dir))
                    File.Delete(file);
            }
            OnPropertyChanged(nameof(ThumbnailCacheSize));
            ToastService.ShowSuccess("缩略图缓存已清空");
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"清空缓存失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void OpenInExplorer(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"无法打开目录: {ex.Message}");
        }
    }
}
