namespace ImgLib.UI.Models;

public sealed record class ImgFile
{
    public string FileName { get; set; }

    public string Extension { get; set; }
    public bool IsRawFile { get; set; }
}
