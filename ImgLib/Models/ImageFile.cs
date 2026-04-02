namespace ImgLib.Models;

using SysIOPath = System.IO.Path;

public abstract record ImageFile
{
    public ImageFile(string filePath)
    {
        Path = filePath;
        Name = SysIOPath.GetFileNameWithoutExtension(filePath);
        Extension = SysIOPath.GetExtension(filePath);
        Exif = new ExifInfo(filePath);

        // TODO:临时写的，改成非打开对象获取(读取元数据）
        var f = new FileInfo(filePath);
        Size = f.Length;
        Created = f.CreationTime;
        Modified = f.LastWriteTime;
    }

    public string Name { get; private set; }

    public string Path { get; init; }

    public string Extension { get; init; }

    public long Size { get; private set; }

    public DateTime Created { get; init; }

    public DateTime? Modified { get; private set; }

    public ExifInfo? Exif { get; init; }

    public Stream GetSourceStream()
    {
        return File.OpenRead(Path);
    }

    public static implicit operator string(ImageFile file)
    {
        return file.Path;
    }

    public static ImageFile GetImageFile(string filePath)
    {
        return filePath.EndsWith(".NEF", StringComparison.InvariantCultureIgnoreCase)
                        ? new RAWFile(filePath)
                        : new JpegFile(filePath);
    }
}

public enum RAWType
{
    Unknown,
    Sony,
    Nikon,
    Canon,
    Panasonic,
    Fujifilm,
    Olympus,
    Pentax,
    Leica,
    Hasselblad,
}

public record RAWFile : ImageFile
{
    public RAWFile(string filePath) : base(filePath)
    {
        RAWType = ParseRAWType(Extension);
    }

    public RAWType RAWType { get; private set; }

    private static RAWType ParseRAWType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".arw" => RAWType.Sony,
            ".nef" => RAWType.Nikon,
            ".cr2" or ".cr3" => RAWType.Canon,
            ".rw2" => RAWType.Panasonic,
            ".raf" => RAWType.Fujifilm,
            ".orf" => RAWType.Olympus,
            ".pef" => RAWType.Pentax,
            ".dng" => RAWType.Leica, // DNG can be used by multiple manufacturers
            _ => RAWType.Unknown,
        };
    }

    public static implicit operator RAWFile(string filePath)
    {
        return new RAWFile(filePath);
    }
}

public record JpegFile : ImageFile
{
    public JpegFile(string filePath) : base(filePath)
    {
    }
}
