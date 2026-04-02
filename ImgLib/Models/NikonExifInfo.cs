using MetadataExtractor.Formats.Exif.Makernotes;

namespace ImgLib.Models;

public sealed partial record class NikonExifInfo : ExifInfo
{
    public NikonExifInfo(ExifInfo original) : base(original) { }

    public NikonExifInfo(string filePath) : base(filePath) { }

    public NikonExifInfo(Stream fileStream) : base(fileStream) { }

    private NikonType1MakernoteDirectory? Type1Directory
    {
        get
        {
            field ??= Metadata.Value.OfType<NikonType1MakernoteDirectory>().FirstOrDefault();
            return field;
        }
    }

    private NikonType2MakernoteDirectory? Type2Directory
    {
        get
        {
            field ??= Metadata.Value.OfType<NikonType2MakernoteDirectory>().FirstOrDefault();
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