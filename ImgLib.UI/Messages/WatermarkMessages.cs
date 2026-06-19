using ImgLib.UI.Models;

namespace ImgLib.UI.Messages;

/// <summary>
/// 水印预览请求消息
/// </summary>
public sealed class PreviewRequestedMessage;

/// <summary>
/// 水印设置文件已保存消息
/// </summary>
public sealed class SettingsFileSavedMessage
{
    public string FilePath { get; }
    public SettingsFileSavedMessage(string filePath) => FilePath = filePath;
}

/// <summary>
/// 加载水印设置文件消息
/// </summary>
public sealed class LoadWatermarkSettingsMessage
{
    public WatermarkSettings Settings { get; }
    public LoadWatermarkSettingsMessage(WatermarkSettings settings) => Settings = settings;
}
