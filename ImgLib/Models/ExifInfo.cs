using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Exif.Makernotes;

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
}


public sealed partial record class NikonExifInfo : ExifInfo
{
    public NikonExifInfo(ExifInfo original) : base(original) { }

    public NikonExifInfo(string filePath) : base(filePath) { }

    public NikonExifInfo(Stream fileStream) : base(fileStream) { }

    private NikonType1MakernoteDirectory? Type1Directory
    {
        get
        {
            if (field == null)
            {
                field = Metadata.Value.OfType<NikonType1MakernoteDirectory>().FirstOrDefault();
            }
            return field;
        }
    }

    private NikonType2MakernoteDirectory? Type2Directory
    {
        get
        {
            if (field == null)
            {
                field = Metadata.Value.OfType<NikonType2MakernoteDirectory>().FirstOrDefault();
            }
            return field;
        }
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