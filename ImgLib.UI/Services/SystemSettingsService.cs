using System;
using System.IO;
using System.Text.Json;
using ImgLib.UI.Models;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI.Services;

/// <summary>
/// 系统设置的加载与保存服务。使用 JSON 文件持久化，临时文件 + Move 保证原子写入。
/// </summary>
public static class SystemSettingsService
{
    private const string ConfigFileName = "SystemSettings.json";

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static string ConfigPath => Path.Combine(AppContext.BaseDirectory, ConfigFileName);

    /// <summary>
    /// 加载系统设置。文件缺失或损坏时返回默认值。
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
        }
        catch
        {
            // 保存失败不影响主流程
        }
    }
}
