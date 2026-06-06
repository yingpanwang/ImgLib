using Avalonia.Media.Imaging;
using SkiaSharp;

namespace ImgLib.UI.ViewModels;

public sealed partial class ImgFileDescViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileName))]
    [NotifyPropertyChangedFor(nameof(FileExtension))]
    [NotifyPropertyChangedFor(nameof(DirectoryPath))]
    public partial string? FilePath { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DimensionDisplay))]
    [NotifyPropertyChangedFor(nameof(MegapixelDisplay))]
    public partial int PixelWidth { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DimensionDisplay))]
    [NotifyPropertyChangedFor(nameof(MegapixelDisplay))]
    public partial int PixelHeight { get; set; }

    [ObservableProperty]
    public partial string? FileCreationTime { get; set; }

    [ObservableProperty]
    public partial string? FileModifiedTime { get; set; }

    [ObservableProperty]
    public partial string? FileSize { get; set; }

    public string? FileName => FilePath is not null ? System.IO.Path.GetFileName(FilePath) : null;
    public string? FileExtension => FilePath is not null ? System.IO.Path.GetExtension(FilePath) : null;
    public string? DirectoryPath => FilePath is not null ? System.IO.Path.GetDirectoryName(FilePath) : null;
    public string? DimensionDisplay => PixelWidth > 0 && PixelHeight > 0
        ? $"{PixelWidth} × {PixelHeight} px"
        : null;
    public string? MegapixelDisplay => PixelWidth > 0 && PixelHeight > 0
        ? $"{PixelWidth * PixelHeight / 1_000_000.0:F1} MP"
        : null;

    public Bitmap? ImageFileSource { get; set; }

    public ImgFileDescViewModel(string? filePath = null)
    {
        if (filePath is not null)
            UpdateFromFile(filePath);
    }

    /// <summary>
    /// 从文件路径加载并更新所有图片信息
    /// </summary>
    public void UpdateFromFile(string filePath)
    {
        FilePath = filePath;

        // 文件系统信息
        if (System.IO.File.Exists(filePath))
        {
            var fileInfo = new System.IO.FileInfo(filePath);
            FileSize = FormatFileSize(fileInfo.Length);
            FileCreationTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            FileModifiedTime = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // 使用 SkiaSharp 读取图片像素尺寸（仅解码头部，不加载完整图像）
        try
        {
            using var stream = System.IO.File.OpenRead(filePath);
            using var codec = SKCodec.Create(stream);
            if (codec != null)
            {
                PixelWidth = codec.Info.Width;
                PixelHeight = codec.Info.Height;
            }
        }
        catch
        {
            PixelWidth = 0;
            PixelHeight = 0;
        }

        ImageFileSource = LoadImage();
    }

    /// <summary>
    /// 清除所有信息
    /// </summary>
    public void Clear()
    {
        FilePath = null;
        PixelWidth = 0;
        PixelHeight = 0;
        FileCreationTime = null;
        FileModifiedTime = null;
        FileSize = null;
        ImageFileSource?.Dispose();
        ImageFileSource = null;
    }

    private Bitmap? LoadImage()
    {
        if (FilePath is null || !System.IO.File.Exists(FilePath))
            return null;
        try
        {
            return new Bitmap(FilePath);
        }
        catch
        {
            return null;
        }
    }

    private static string FormatFileSize(long bytes)
    {
        return bytes switch
        {
            >= 1_073_741_824 => $"{bytes / 1_073_741_824.0:F2} GB",
            >= 1_048_576 => $"{bytes / 1_048_576.0:F2} MB",
            >= 1024 => $"{bytes / 1024.0:F2} KB",
            _ => $"{bytes} B"
        };
    }
}