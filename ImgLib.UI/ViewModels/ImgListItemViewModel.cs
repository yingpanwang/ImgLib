using Avalonia.Media.Imaging;
using ImgLib.Models;
using ImgLib.UI.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ImgLib.UI.ViewModels;

public partial class ImgListItemViewModel(ImageFile sourceFile) : ViewModelBase, IDisposable
{
    public ImageFile SourceFile { get; init; } = sourceFile;

    [ObservableProperty]
    public partial string? FilePath { get; private set; } = sourceFile.Path;

    public string? FileName => FilePath is not null ? System.IO.Path.GetFileName(FilePath) : null;

    public string? FileExtension => FilePath is not null ? System.IO.Path.GetExtension(FilePath) : null;

    public string? FileCreationTime => SourceFile.Created.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>缓存文件大小，只计算一次</summary>
    private string? _cachedFileSize;
    public string? FileSize
    {
        get
        {
            if (_cachedFileSize is not null)
                return _cachedFileSize;

            if (FilePath is null || !File.Exists(FilePath))
                return null;

            var fileInfo = new FileInfo(FilePath);
            _cachedFileSize = $"{fileInfo.Length / 1024.0:F2} KB";
            return _cachedFileSize;
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
            return default;
        }
    }

    private Bitmap? LoadImage()
    {
        if (FilePath is null || !File.Exists(FilePath))
            return null;
        try
        {
            return LoadThumbnail(FilePath, 200, 120);
        }
        catch
        {
            return null;
        }

        /// <summary>
        /// 使用 SkiaSharp 加载缩略图，优先从磁盘缓存读取，
        /// 未命中时用 SKCodec 流式解码到目标尺寸并写入缓存。
        /// </summary>
        static Bitmap LoadThumbnail(string path, int maxWidth, int maxHeight)
        {
            // 1. 检查磁盘缓存
            var lastWrite = File.GetLastWriteTimeUtc(path);
            var cached = ThumbnailCacheService.TryGet(path, lastWrite);
            if (cached is not null)
                return cached;

            // 2. 流式解码到目标尺寸
            using var fs = File.OpenRead(path);
            using var codec = SKCodec.Create(fs);

            if (codec is null)
                throw new InvalidDataException("无法解码图像：" + path);

            int srcW = codec.Info.Width;
            int srcH = codec.Info.Height;

            float scaleW = maxWidth > 0 ? (float)maxWidth / srcW : 1f;
            float scaleH = maxHeight > 0 ? (float)maxHeight / srcH : 1f;
            float scale = Math.Min(scaleW, scaleH);
            if (scale > 1f) scale = 1f;

            int dstW = Math.Max(1, (int)(srcW * scale));
            int dstH = Math.Max(1, (int)(srcH * scale));

            var scaledInfo = new SKImageInfo(dstW, dstH);

            // 尝试直接解码到目标尺寸（部分 codec 不支持则回退全图）
            using var decoded = SKBitmap.Decode(codec, scaledInfo)
                ?? DecodeFullThenResize(path, ref scaledInfo);

            if (decoded is null)
                throw new InvalidDataException("无法缩放图像：" + path);

            // 3. 编码 PNG → 存缓存 → 返回 Bitmap
            using var skImage = SKImage.FromBitmap(decoded);
            using var pngData = skImage.Encode(SKEncodedImageFormat.Png, 90);

            using var memStream = new MemoryStream();
            pngData.SaveTo(memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            // 异步写入缓存（不阻塞返回）
            var pngBytes = memStream.ToArray();
            _ = Task.Run(() => ThumbnailCacheService.Save(path, lastWrite, pngBytes));

            return new Bitmap(new MemoryStream(pngBytes));
        }

        /// <summary>
        /// 回退方案：全图解码后缩放（用于 SKCodec 不支持缩放的格式）
        /// </summary>
        static SKBitmap DecodeFullThenResize(string path, ref SKImageInfo targetInfo)
        {
            using var fs = File.OpenRead(path);
            using var full = SKBitmap.Decode(fs);
            if (full is null)
                throw new InvalidDataException("无法解码图像：" + path);

            int w = targetInfo.Width > 0 ? targetInfo.Width : full.Width;
            int h = targetInfo.Height > 0 ? targetInfo.Height : full.Height;

            return full.Resize(
                new SKImageInfo(w, h),
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        }
    }

    /// <summary>
    /// 从单个文件路径创建列表项。
    /// </summary>
    public static ImgListItemViewModel CreateFromPath(string filePath)
    {
        var ext = System.IO.Path.GetExtension(filePath);
        var isRaw = ext.Equals(".nef", StringComparison.OrdinalIgnoreCase)
                 || ext.Equals(".arw", StringComparison.OrdinalIgnoreCase);

        return new ImgListItemViewModel(
            isRaw ? new RAWFile(filePath) : new JpegFile(filePath));
    }

    /// <summary>
    /// 从文件夹扫描所有支持的图片文件。
    /// 使用 EnumerateFiles 流式枚举，支持 CancellationToken 取消。
    /// </summary>
    public static IEnumerable<ImgListItemViewModel> Create(string folderPath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            yield break;

        if (!Directory.Exists(folderPath))
            yield break;

        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".nef", ".arw"
        };

        foreach (var file in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly))
        {
            ct.ThrowIfCancellationRequested();

            if (!extensions.Contains(System.IO.Path.GetExtension(file)))
                continue;

            yield return CreateFromPath(file);
        }
    }

    public void Dispose()
    {
        Debug.WriteLine($"{FileName} disposed !");
    }
}
