using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Text.Json;

namespace ImgLib.Models;

public partial record class ExifInfo
{
    /// <summary>
    /// 制造商
    /// </summary>
    public virtual string? Make =>
        ExifIfd0Directory?.GetDescription(ExifDirectoryBase.TagMake);

    /// <summary>
    /// 相机型号
    /// </summary>
    public virtual string? Model =>
        ExifIfd0Directory?.GetDescription(ExifDirectoryBase.TagModel);

    /// <summary>
    /// 镜头制造商
    /// </summary>
    public virtual string? LensMake
        => ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagLensMake);

    /// <summary>
    /// 镜头型号
    /// </summary>
    public virtual string? LensModel
        => ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagLensModel);

    /// <summary>
    /// 光圈值（F值）
    /// </summary>
    public virtual string? FNumber =>
        ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagFNumber);

    /// <summary>
    /// ISO感光度
    /// </summary>
    public virtual string? ISO =>
        ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagIsoEquivalent);

    /// <summary>
    /// 焦距
    /// </summary>
    public virtual string? FocalLength =>
        ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagFocalLength);

    /// <summary>
    /// 焦距（等效35mm格式）
    /// </summary>
    public virtual string? FocalLengthIn35mmFormat =>
        ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.Tag35MMFilmEquivFocalLength);

    /// <summary>
    /// 曝光时间
    /// </summary>
    public virtual string? ExposureTime
        => ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureTime);

    /// <summary>
    /// 拍摄时间（原始时间）
    /// </summary>
    public virtual string? DateTimeOriginal
        => ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

    /// <summary>
    /// 曝光补偿值
    /// </summary>
    public virtual string? ExposureCompensation
        => ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureBias);

    /// <summary>
    /// 白平衡模式
    /// </summary>
    public virtual string? WhiteBalance
        => ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagWhiteBalance);

    /// <summary>
    /// 曝光程序（拍摄模式）
    /// </summary>
    public virtual string? ExposureProgram
        => ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagExposureProgram);

    /// <summary>
    /// 测光模式
    /// </summary>
    public virtual string? MeteringMode
        => ExifSubIfdDirectory?.GetDescription(ExifDirectoryBase.TagMeteringMode);
}

public partial record class ExifInfo
{
    protected ExifSubIfdDirectory? ExifSubIfdDirectory
    {
        get
        {
            if (field == null)
            {
                field = Metadata.Value.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            }

            return field;
        }
    }

    protected ExifIfd0Directory? ExifIfd0Directory
    {
        get
        {
            if (field == null)
            {
                field = Metadata.Value.OfType<ExifIfd0Directory>().FirstOrDefault();
            }

            return field;
        }
    }

    protected Lazy<IReadOnlyList<MetadataExtractor.Directory>> Metadata { get; init; }

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