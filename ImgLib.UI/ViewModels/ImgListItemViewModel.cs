using Avalonia.Media.Imaging;
using ImgLib.Models;
using SkiaSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ImgLib.UI.ViewModels;

public partial class ImgListItemViewModel(ImageFile sourceFile) : ViewModelBase, IDisposable
{
    public ImageFile SourceFile { get; init; } = sourceFile;

    [ObservableProperty]
    public partial string? FilePath { get; private set; } = sourceFile.Path;

    public string? FileName => FilePath is not null ? System.IO.Path.GetFileName(FilePath) : null;

    public string? FileExtension => FilePath is not null ? System.IO.Path.GetExtension(FilePath) : null;

    public string? FileCreationTime => SourceFile.Created.ToString("yyyy-MM-dd HH:mm:ss");

    public string? FileSize
    {
        get
        {
            if (FilePath is null || !System.IO.File.Exists(FilePath))
                return null;
            var fileInfo = new System.IO.FileInfo(FilePath);
            return $"{fileInfo.Length / 1024.0:F2} KB"; // Size in KB
        }
    }

    public Task<Bitmap?> ImageSource => LoadImageAsync();

    private async Task<Bitmap?> LoadImageAsync()
    {
        try
        {
            return await Task.Run(LoadImage);
        }
        catch (Exception)
        {
            // Handle exceptions (e.g., file not found, unsupported format)
            return default;
        }
    }

    private Bitmap? LoadImage()
    {
        if (FilePath is null || !System.IO.File.Exists(FilePath))
            return null;
        try
        {
            return LoadThumbnail(FilePath, 200, 120);
            //return new Bitmap(FilePath);
        }
        catch
        {
            // Handle exceptions (e.g., file not found, unsupported format)
            return null;
        }

        /// <summary>
        /// 使用 SkiaSharp 加载缩略图并返回 Avalonia Bitmap。
        /// </summary>
        /// <param name="path">原始图像路径</param>
        /// <param name="maxWidth">最大宽度（为 0 表示不限制）</param>
        /// <param name="maxHeight">最大高度（为 0 表示不限制）</param>
        /// <returns>Avalonia Bitmap（缩略图）</returns>
        static Bitmap LoadThumbnail(string path, int maxWidth = 200, int maxHeight = 0)
        {
            using var inputStream = File.OpenRead(path);
            using var original = SKBitmap.Decode(inputStream);

            if (original == null)
                throw new InvalidDataException("无法解码图像：" + path);

            // 计算缩放比例
            int width = original.Width;
            int height = original.Height;

            float scaleX = maxWidth > 0 ? (float)maxWidth / width : 1f;
            float scaleY = maxHeight > 0 ? (float)maxHeight / height : 1f;

            float scale = Math.Min(scaleX, scaleY);
            if (scale > 1f) scale = 1f; // 不放大

            int resizedWidth = (int)(width * scale);
            int resizedHeight = (int)(height * scale);

            using var resized = original.Resize(
                new SKImageInfo(resizedWidth, resizedHeight),
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None)
                );
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Png, 90);
            using var memStream = new MemoryStream();

            data.SaveTo(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            return new Bitmap(memStream);
        }
    }

    public static IEnumerable<ImgListItemViewModel> Create(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return [];

        if (!System.IO.Directory.Exists(folderPath))
            return [];

        var files = System.IO.Directory.GetFiles(folderPath, "*.*", System.IO.SearchOption.TopDirectoryOnly)
            .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".NEF", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".ARW", StringComparison.OrdinalIgnoreCase))
            .Select(file => new ImgListItemViewModel(
                    file.EndsWith(".NEF", StringComparison.InvariantCultureIgnoreCase)
                        ? new RAWFile(file)
                        : new JpegFile(file)
                ))
            .ToArray();

        return files;
    }

    public void Dispose()
    {
        Debug.WriteLine($"{FileName} disposed !");
    }
}