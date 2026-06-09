using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Text.Json;

namespace ImgLib.Models;

/// <summary>
/// Exif信息
/// </summary>
public partial record class ExifInfo
{
    public ExifInfo(string filePath)
    {
        Metadata = new Lazy<IReadOnlyList<MetadataExtractor.Directory>>(
            valueFactory: () => ImageMetadataReader.ReadMetadata(filePath),
            true
            );
    }

    public ExifInfo(Stream fileStream)
    {
        Metadata = new Lazy<IReadOnlyList<MetadataExtractor.Directory>>(
            valueFactory: () => ImageMetadataReader.ReadMetadata(fileStream),
            true
            );
    }

    /// <summary>
    /// 在后台线程上强制加载元数据，避免后续属性访问阻塞 UI。
    /// 多次调用是安全的（已加载则立即返回）。
    /// </summary>
    public async ValueTask EnsureMetadataLoadedAsync()
    {
        if (Metadata.IsValueCreated)
            return;

        await Task.Run(() => _ = Metadata.Value).ConfigureAwait(false);
    }

    /// <summary>
    /// 返回水印模板占位符 → 值的映射字典。
    /// Key 为 ExifInfo 属性名（不含 {} 包裹），Value 为对应字段的描述字符串。
    /// 子类可 override 扩展品牌专用字段。
    /// </summary>
    public virtual IReadOnlyDictionary<string, string?> GetTemplateReplacements()
    {
        return new Dictionary<string, string?>
        {
            ["Make"] = Make,
            ["Model"] = Model,
            ["LensMake"] = LensMake,
            ["LensModel"] = LensModel,
            ["FNumber"] = FNumber,
            ["ISO"] = ISO,
            ["FocalLength"] = FocalLength,
            ["FocalLengthIn35mmFormat"] = FocalLengthIn35mmFormat,
            ["ExposureTime"] = ExposureTime,
            ["DateTimeOriginal"] = DateTimeOriginal,
            ["ExposureCompensation"] = ExposureCompensation,
            ["WhiteBalance"] = WhiteBalance,
            ["ExposureProgram"] = ExposureProgram,
            ["MeteringMode"] = MeteringMode,
        }.AsReadOnly();
    }

    public virtual string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}

//{
//  "Make": "NIKON CORPORATION",
//  "Model": "NIKON Z 6_2",
//  "LensMake": "NIKON",
//  "LensModel": "NIKKOR Z 85mm f/1.8 S",
//  "FNumber": "5.6",
//  "ISO": "180",
//  "FocalLength": "85",
//  "FocalLengthIn35mmFormat": "85",
//  "ExposureTime": "1/800",
//  "DateTimeOriginal": "2025/04/21 09:59:20",
//  "ExposureCompensation": "0",
//  "WhiteBalance": "Auto",
//  "ExposureProgram": "M",
//  "MeteringMode": "点测光"
//}