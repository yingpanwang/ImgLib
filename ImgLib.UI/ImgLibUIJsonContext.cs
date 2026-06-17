using System.Collections.Generic;
using System.Text.Json.Serialization;
using ImgLib.Models;
using ImgLib.UI.Models;

namespace ImgLib.UI;

/// <summary>
/// JSON 序列化源生成上下文，用于 AOT 编译支持。
/// 包含 ImgLib.UI 项目中所有需要通过 JSON 序列化/反序列化的类型。
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SystemSettings))]
[JsonSerializable(typeof(PreviewSettings))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(ExifInfo))]
[JsonSerializable(typeof(NikonExifInfo))]
public partial class ImgLibUIJsonContext : JsonSerializerContext
{
}
