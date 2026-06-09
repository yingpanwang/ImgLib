using Avalonia.Media.Imaging;
using ImgLib.UI.Services;
using SkiaSharp;
using System.IO;

namespace ImgLib.UI.ViewModels;

public partial class ExportDialogItemViewModel : ViewModelBase, IDisposable
{
    /// <summary>
    /// 源文件完整路径
    /// </summary>
    public string FilePath { get; init; }

    /// <summary>
    /// 文件名（含扩展名）
    /// </summary>
    public string FileName { get; init; }

    /// <summary>
    /// 是否被选中导出
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; } = true;

    /// <summary>
    /// 缩略图（异步加载）
    /// </summary>
    public Task<Bitmap?> Thumbnail => LoadThumbnailAsync();

    public ExportDialogItemViewModel(string filePath)
    {
        FilePath = filePath;
        FileName = Path.GetFileName(filePath);
    }

    private async Task<Bitmap?> LoadThumbnailAsync()
    {
        try
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(FilePath))
                    return null;

                var lastWrite = File.GetLastWriteTimeUtc(FilePath);

                // 检查缓存
                var cached = ThumbnailCacheService.TryGet(FilePath, lastWrite);
                if (cached is not null)
                    return cached;

                // 流式解码缩略图（目标尺寸约 150x100，用于对话框选择列表）
                using var fs = File.OpenRead(FilePath);
                using var codec = SKCodec.Create(fs);
                if (codec is null) return null;

                int srcW = codec.Info.Width, srcH = codec.Info.Height;
                float scale = Math.Min(150f / srcW, 100f / srcH);
                if (scale > 1f) scale = 1f;

                int dstW = Math.Max(1, (int)(srcW * scale));
                int dstH = Math.Max(1, (int)(srcH * scale));

                using var decoded = SKBitmap.Decode(codec, new SKImageInfo(dstW, dstH));
                if (decoded is null) return null;

                using var skImg = SKImage.FromBitmap(decoded);
                using var pngData = skImg.Encode(SKEncodedImageFormat.Png, 90);

                using var ms = new MemoryStream();
                pngData.SaveTo(ms);
                var pngBytes = ms.ToArray();

                // 异步写缓存（不阻塞返回）
                _ = Task.Run(() => ThumbnailCacheService.Save(FilePath, lastWrite, pngBytes));

                return new Bitmap(new MemoryStream(pngBytes));
            });
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        // 缩略图 Bitmap 由 GC 回收时释放
    }
}
