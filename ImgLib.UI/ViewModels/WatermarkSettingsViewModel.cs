using ImgLib.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace ImgLib.UI.ViewModels;

public partial class WatermarkSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ImageGenerateOption ImageGenerateOption { get; private set; }

    [ObservableProperty]
    public partial ObservableCollection<ExifInfoNode> ExifInfoTree { get; private set; } = new();

    [ObservableProperty]
    public partial ExifInfo? ExifInfo { set; get; }

    public WatermarkSettingsViewModel(ImageGenerateOption? option = null, ExifInfo? exifInfo = null)
    {
        ImageGenerateOption =
        option ??= new ImageGenerateOption(0.89f);
        ExifInfo = exifInfo;
    }

    public void BuildExifInfoTree()
    {
        if (ExifInfo == null)
            return;

        using var exifDoc = JsonSerializer.SerializeToDocument<ExifInfo>(ExifInfo);

        var es = exifDoc.RootElement.EnumerateObject();
        foreach (var item in es)
        {
            ExifInfoTree.Add(new ExifInfoNode(item.Name, item.Value.GetString()));
        }
    }
}

public record ExifInfoNode
{
    public string Name { get; set; }
    public string DisplayName { get; set; }

    public string? Value { get; set; }

    public ObservableCollection<ExifInfoNode>? Children { get; set; } = new();

    public ExifInfoNode(string name, string? value, string? displayName = null, ObservableCollection<ExifInfoNode>? children = null)
    {
        Name = name;
        DisplayName = displayName ?? name;
        Value = value;
        Children = children;
    }
}