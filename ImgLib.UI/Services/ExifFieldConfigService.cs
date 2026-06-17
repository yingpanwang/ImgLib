using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ImgLib.UI;

namespace ImgLib.UI.Services;

/// <summary>
/// 加载 EXIF 字段名称映射配置，允许用户通过配置文件自定义显示名称
/// </summary>
public static class ExifFieldConfigService
{
    private const string ConfigFileName = "ExifFieldMappings.json";

    /// <summary>
    /// 默认映射（英文 Key → 中文显示名），配置文件缺失时使用
    /// </summary>
    private static readonly Dictionary<string, string> DefaultMappings = new()
    {
        ["Make"] = "制造商",
        ["Model"] = "相机型号",
        ["LensMake"] = "镜头制造商",
        ["LensModel"] = "镜头型号",
        ["FNumber"] = "光圈",
        ["ISO"] = "ISO",
        ["FocalLength"] = "焦距",
        ["FocalLengthIn35mmFormat"] = "等效焦距",
        ["ExposureTime"] = "快门",
        ["DateTimeOriginal"] = "时间",
        ["ExposureCompensation"] = "曝光补偿",
        ["WhiteBalance"] = "白平衡",
        ["ExposureProgram"] = "拍摄模式",
        ["MeteringMode"] = "测光模式",
        ["CameraSerialNumber"] = "机身序列号",
        ["FirmwareVersion"] = "固件版本",
        ["ShutterCount"] = "快门次数",
        ["LensType"] = "镜头类型",
        ["AfType"] = "对焦类型",
        ["AfFocusPosition"] = "AF对焦点",
        ["QualityAndFileFormat"] = "画质格式",
        ["ColorMode"] = "色彩模式",
        ["ActiveDLighting"] = "Active D-Lighting",
        ["VignetteControl"] = "暗角控制",
        ["HighIsoNoiseReduction"] = "高ISO降噪",
        ["PictureControlName"] = "Picture Control",
        ["ImageStabilisation"] = "防抖",
        ["ShootingMode"] = "拍摄模式(Nikon)",
        ["FlashUsed"] = "闪光灯",
        ["FlashMode"] = "闪光灯模式",
        ["IsoMode"] = "ISO模式",
        ["SceneMode"] = "场景模式",
        ["MultiExposure"] = "多重曝光",
        ["PictureControlSharpness"] = "PC锐度",
        ["PictureControlClarity"] = "PC清晰度",
        ["PictureControlContrast"] = "PC对比度",
        ["PictureControlBrightness"] = "PC亮度",
        ["PictureControlSaturation"] = "PC饱和度",
    };

    private static Dictionary<string, string>? _cachedMappings;

    /// <summary>
    /// 加载 EXIF 字段映射（带缓存），返回 Key→DisplayName 字典
    /// </summary>
    public static Dictionary<string, string> LoadMappings()
    {
        if (_cachedMappings != null)
            return _cachedMappings;

        // 1. 尝试从配置文件加载
        var configPath = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
        if (File.Exists(configPath))
        {
            try
            {
                var json = File.ReadAllText(configPath);
                var mappings = JsonSerializer.Deserialize(json, ImgLibUIJsonContext.Default.DictionaryStringString);
                if (mappings != null && mappings.Count > 0)
                {
                    _cachedMappings = mappings;
                    return _cachedMappings;
                }
            }
            catch
            {
                // 配置文件解析失败，使用默认值
            }
        }

        // 2. 回退到默认映射
        _cachedMappings = new Dictionary<string, string>(DefaultMappings);
        return _cachedMappings;
    }

    /// <summary>
    /// 清除缓存，下次加载时重新读取配置文件
    /// </summary>
    public static void ClearCache()
    {
        _cachedMappings = null;
    }
}
