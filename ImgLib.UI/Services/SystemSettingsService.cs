using System;
using System.IO;
using System.Text.Json;
using ImgLib.UI.Models;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI.Services;

/// <summary>
/// 系统设置的加载与保存服务。使用 JSON 文件持久化，临时文件 + Move 保证原子写入。
/// 提供 <see cref="Current"/> 单例供 ViewModel 订阅设置变化。
/// </summary>
public static class SystemSettingsService
{
    private const string ConfigFileName = "SystemSettings.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static string ConfigPath => Path.Combine(AppContext.BaseDirectory, ConfigFileName);

    private static SystemSettingsViewModel? _current;
    private static readonly object _lock = new();

    /// <summary>
    /// 获取系统设置视图模型的单例实例。首次访问时从磁盘加载，
    /// 后续通过 <see cref="Save"/> 自动同步，所有绑定属性均可观察。
    /// </summary>
    public static SystemSettingsViewModel Current
    {
        get
        {
            if (_current == null)
            {
                lock (_lock)
                {
                    if (_current == null)
                    {
                        var model = Load();
                        _current = model.ToViewModel();
                    }
                }
            }
            return _current;
        }
    }

    /// <summary>
    /// 加载系统设置数据模型。文件缺失或损坏时返回默认值。
    /// 通常应使用 <see cref="Current"/> 获取可观察的视图模型，
    /// 仅在需要独立副本（如设置对话框）时直接调用此方法。
    /// </summary>
    public static SystemSettings Load()
    {
        if (!File.Exists(ConfigPath))
            return SystemSettings.Default;

        try
        {
            var json = File.ReadAllText(ConfigPath);
            var settings = JsonSerializer.Deserialize<SystemSettings>(json);
            return settings ?? SystemSettings.Default;
        }
        catch
        {
            return SystemSettings.Default;
        }
    }

    /// <summary>
    /// 保存系统设置。使用临时文件 + Move 保证原子写入，避免写入过程中崩溃导致文件损坏。
    /// 保存后自动同步更新 <see cref="Current"/> 单例，触发所有订阅者的 PropertyChanged。
    /// </summary>
    public static void Save(SystemSettings settings)
    {
        try
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var tmpFile = ConfigPath + ".tmp";
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(tmpFile, json);
            File.Move(tmpFile, ConfigPath, overwrite: true);

            // 同步更新内存中的单例，触发所有绑定属性的 PropertyChanged 通知
            if (_current != null)
            {
                _current.FromModel(settings);
            }
        }
        catch
        {
            // 保存失败不影响主流程
        }
    }
}
