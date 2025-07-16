using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.Linq;

namespace ImgLib.UI.ViewModels;

public sealed partial class ImgListItemViewModel(string filePath) : ViewModelBase
{
    [ObservableProperty]
    public partial string? FilePath { get; set; } = filePath;

    public string? FileName => FilePath is not null ? System.IO.Path.GetFileName(FilePath) : null;

    public string? FileExtension => FilePath is not null ? System.IO.Path.GetExtension(FilePath) : null;

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

    public Bitmap? ImageSource => LoadImage();

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
            // Handle exceptions (e.g., file not found, unsupported format)
            return null;
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
                           file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
            .Select(file => new ImgListItemViewModel(file))
            .ToArray();

        return files;
    }
}
