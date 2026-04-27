using Avalonia.Media.Imaging;

namespace ImgLib.UI.ViewModels;

public sealed partial class ImgFileDescViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string? FilePath { get; set; }

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

    public Bitmap? ImageFileSource { get; set; }

    public ImgFileDescViewModel(string? filePath = null)
    {
        FilePath = filePath;

        ImageFileSource = LoadImage();
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
            // Handle exceptions (e.g., file not found, unsupported format)
            return null;
        }
    }
}