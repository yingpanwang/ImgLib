namespace ImgLib;

/// <summary>
/// JSON 序列化源生成上下文，用于 AOT 编译支持。
/// 包含 ImgLib 项目中所有需要通过 JSON 序列化/反序列化的类型。
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ExifInfo))]
[JsonSerializable(typeof(NikonExifInfo))]
public partial class ImgLibJsonContext : JsonSerializerContext
{
}
