namespace ImgLib.Models;

using SysIOPath = System.IO.Path;

public interface IImageFile
{
    Stream GetSourceStream();
}

public record ImageFile : IImageFile
{
    public ImageFile(string filePath)
    {
        Path = filePath;
        Name = SysIOPath.GetFileNameWithoutExtension(filePath);
        Exif = new ExifInfo(filePath);

        // TODO:临时写的，改成非打开对象获取(读取元数据）
        var f = new FileInfo(filePath);
        Created = f.CreationTime;
        Modified = f.LastWriteTime;
    }

    public string Name { get; private set; }

    public string Path { get; init; }

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
    public RAWFile(string filePath, bool throwExIfNotSupportRAWType = true) : base(filePath)
    {
        string ext = SysIOPath.GetExtension(filePath);

        RAWType = ParseRAWType(ext);

        if (throwExIfNotSupportRAWType && RAWType == RAWType.Unknown)
            throw new NotSupportedException($"Unsupported RAW file type: {ext}");
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
public record PngFile : ImageFile
{
    public PngFile(string filePath) : base(filePath)
    {
    }
}

public record VirtualFile : IImageFile
{
    public VirtualFile(string virtualPath)
    {
        throw new NotImplementedException();
        // TODO: 检查虚拟路径的合法性
        // throw new ArgumentException("Invalid virtual path", nameof(virtualPath));
    }

    public Stream GetSourceStream()
    {
        // TODO: 实现获取虚拟文件内容的逻辑
        throw new NotImplementedException();
    }
}

public class FileGroup
{
    public string DisplayName { get; private set; } = string.Empty;

    public string DisplayPath { get; private set; } = string.Empty;

    public int Count => Files?.Count() ?? 0;

    public IEnumerable<ImageFile>? Files
    {
        get => field;
        set
        {
            field = value;
            if (Count > 0)
            {
                var firstFile = field!.First();
                DisplayName = firstFile.Name;
                DisplayPath = System.IO.Path.GetDirectoryName(firstFile.Path) ?? string.Empty;
            }
            else
            {
                DisplayName = string.Empty;
                DisplayPath = string.Empty;
            }
        }
    }

    public static FileGroup CreateFromFiles(IEnumerable<ImageFile> files)
    {
        return new FileGroup { Files = files };
    }
}