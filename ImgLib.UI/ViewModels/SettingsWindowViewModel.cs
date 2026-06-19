using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImgLib.Services;
using ImgLib.UI.Models;
using ImgLib.UI.Services;

namespace ImgLib.UI.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    /// <summary>缩略图缓存目录路径</summary>
    public string ThumbnailCachePath => ThumbnailCacheService.CacheDirectory;

    /// <summary>系统设置视图模型（含预览设置）</summary>
    [ObservableProperty]
    public partial SystemSettingsViewModel SystemSettings { get; set; } = new();

    [ObservableProperty]
    public partial string ClearThumbnailCacheButtonText { get; private set; }

    private static string FormatSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            < 1024L * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
            _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB",
        };
    }
    private SystemSettings _settings;

    public SettingsWindowViewModel()
    {
        _settings = SystemSettingsService.Load();

        ClearThumbnailCacheButtonText = $"清理 ({FormatSize(ThumbnailCacheService.GetCacheSize())})";

        SystemSettings.FromModel(_settings);
    }

    public SystemSettings Capture()
    {
        SystemSettings.ApplyTo(_settings);
        return _settings;
    }

    [RelayCommand]
    public async Task ClearThumbnailCache()
    {
        try
        {
            ClearThumbnailCacheButtonText = "⭕";

            await Task.Delay(Random.Shared.Next(500, 5000)).ContinueWith((delayTask) =>
            {
                DirectoryInfo dir = new(ThumbnailCachePath);

                if (!dir.Exists)
                    return;
                dir.Delete(true);
            });


            ToastService.ShowSuccess("清理缩略图完成");

            ClearThumbnailCacheButtonText = $"清理 ({FormatSize(ThumbnailCacheService.GetCacheSize())})";
        }
        catch (System.Exception)
        {
            ToastService.ShowWarning("清理缩略图失败,请检查对应目录权限!");
        }
    }
}
