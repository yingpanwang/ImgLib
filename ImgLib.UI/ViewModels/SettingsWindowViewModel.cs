using CommunityToolkit.Mvvm.ComponentModel;
using ImgLib.UI.Models;
using ImgLib.UI.Services;

namespace ImgLib.UI.ViewModels;

public partial class SettingsWindowViewModel : ViewModelBase
{
    /// <summary>缩略图缓存目录路径</summary>
    public string ThumbnailCachePath => Services.ThumbnailCacheService.CacheDirectory;

    /// <summary>系统设置视图模型（含预览设置）</summary>
    [ObservableProperty]
    public partial SystemSettingsViewModel SystemSettings { get; set; } = new();

    private SystemSettings _settings;

    public SettingsWindowViewModel()
    {
        _settings = SystemSettingsService.Load();
        SystemSettings.FromModel(_settings);
    }

    public SystemSettings Capture()
    {
        SystemSettings.ApplyTo(_settings);
        return _settings;
    }
}
