using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ImgLib.UI.Services;

/// <summary>
/// 缩略图磁盘缓存服务。
/// 以文件路径 + 最后写入时间为键，避免文件更新后使用过期缓存。
/// </summary>
public static class ThumbnailCacheService
{
    private static readonly string CacheDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ImgLib",
        "thumbnails");

    /// <summary>
    /// 缩略图缓存目录路径
    /// </summary>
    public static string CacheDirectory => CacheDir;

    private static string CacheKey(string filePath, DateTime lastWriteTimeUtc)
    {
        var raw = $"{filePath}|{lastWriteTimeUtc.Ticks}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash)[..16];
    }

    /// <summary>
    /// 尝试从缓存获取缩略图。未命中返回 null。
    /// </summary>
    public static Bitmap? TryGet(string filePath, DateTime lastWriteTimeUtc)
    {
        var cacheFile = Path.Combine(CacheDir, CacheKey(filePath, lastWriteTimeUtc) + ".png");
        if (File.Exists(cacheFile))
        {
            try
            {
                return new Bitmap(cacheFile);
            }
            catch
            {
                // 缓存文件损坏，删除并返回 null
                TryDelete(cacheFile);
            }
        }
        return null;
    }

    /// <summary>
    /// 将缩略图 PNG 数据写入缓存。使用临时文件 + Move 保证原子写入。
    /// </summary>
    public static void Save(string filePath, DateTime lastWriteTimeUtc, byte[] pngData)
    {
        try
        {
            Directory.CreateDirectory(CacheDir);

            var cacheFile = Path.Combine(CacheDir, CacheKey(filePath, lastWriteTimeUtc) + ".png");
            var tmpFile = cacheFile + ".tmp";

            File.WriteAllBytes(tmpFile, pngData);
            File.Move(tmpFile, cacheFile, overwrite: true);
        }
        catch
        {
            // 缓存写入失败不影响主流程
        }
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { }
    }
}
