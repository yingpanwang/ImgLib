using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ImgLib.Models;

public partial record class ExifInfo(
    string? Make,
    string? Model,
    string? LensMake,
    string? LensModel,
    string? FNumber,
    string? ISO,
    string? FocalLength,
    string? FocalLengthIn35mmFormat,
    string? ExposureTime,
    string? DateTimeOriginal,
    string? ExposureCompensation,
    string? WhiteBalance,
    string? ExposureProgram,
    string? MeteringMode
    );

public sealed partial record class ExifInfo
{
    public static ExifInfo? From(Stream fileStrema)
    {
        var dirs = ImageMetadataReader.ReadMetadata(fileStrema);

        if (dirs == null || dirs.Count == 0)
            return default;

        var ifd0 = dirs.OfType<ExifIfd0Directory>().FirstOrDefault();
        if (ifd0 != null)
        {
            var model = ifd0.GetString(ExifDirectoryBase.TagModel);
            Console.WriteLine($"相机型号: {model}");
        }

        foreach (var item in dirs)
        {
            var t = item.GetType();
            foreach (var tag in item.Tags)
            {
                Console.WriteLine($"{item.Name} - {tag.Name} = {tag.Description}");
            }
        }

        return default;
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