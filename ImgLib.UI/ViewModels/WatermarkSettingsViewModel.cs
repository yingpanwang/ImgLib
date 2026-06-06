using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ImgLib;
using ImgLib.Models;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using System.Text;

namespace ImgLib.UI.ViewModels;

public partial class WatermarkSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ImageGenerateOption ImageGenerateOption { get; private set; }

    [ObservableProperty]
    public partial ObservableCollection<ExifInfoNode> ExifInfoTree { get; private set; } = new();

    [ObservableProperty]
    public partial ExifInfo? ExifInfo { set; get; }

    // 控制是否显示直方图
    [ObservableProperty]
    public partial bool ShowHistogram { get; set; } = false;

    // 水印预览文本
    [ObservableProperty]
    public partial string PreviewWatermarkText { get; private set; } = string.Empty;

    // 水印颜色画笔
    [ObservableProperty]
    public partial IBrush? WatermarkColorBrush { get; private set; }

    // 水印阴影颜色画笔
    [ObservableProperty]
    public partial IBrush? WatermarkShadowColorBrush { get; private set; }

    // 水印边框颜色画笔
    [ObservableProperty]
    public partial IBrush? WatermarkBorderColorBrush { get; private set; }

    // 水平对齐索引
    [ObservableProperty]
    public partial int HorizontalAlignIndex { get; set; } = 1;

    public WatermarkSettingsViewModel(ImageGenerateOption? option = null, ExifInfo? exifInfo = null)
    {
        ImageGenerateOption = option ?? new ImageGenerateOption(0.89f);
        ExifInfo = exifInfo;

        UpdatePreviewText();
        UpdateColorBrushes();
    }

    public ICommand GetExifInfoCommand => new RelayCommand(
            () => BuildExifInfoTree()
        );

    public void BuildExifInfoTree()
    {
        if (ExifInfo == null)
            return;

        ExifInfoTree.Clear();

        using var exifDoc = JsonSerializer.SerializeToDocument<ExifInfo>(ExifInfo);

        var es = exifDoc.RootElement.EnumerateObject();
        foreach (var item in es)
        {
            ExifInfoTree.Add(new ExifInfoNode(item.Name, item.Value.GetString()));
        }
    }

    partial void OnExifInfoChanged(ExifInfo? value)
    {
        ExifInfoTree.Clear();
        UpdatePreviewText();
    }

    partial void OnImageGenerateOptionChanged(ImageGenerateOption value)
    {
        UpdatePreviewText();
        UpdateColorBrushes();
    }

    partial void OnHorizontalAlignIndexChanged(int value)
    {
        ImageGenerateOption.WatermarkHorizontalAlignment = value switch
        {
            0 => "Left",
            1 => "Center",
            2 => "Right",
            _ => "Center"
        };
    }

    private void UpdatePreviewText()
    {
        PreviewWatermarkText = ImageGenerateOption.ParseWatermarkTemplate(ExifInfo);
    }

    private void UpdateColorBrushes()
    {
        WatermarkColorBrush = ParseColorBrush(ImageGenerateOption.WatermarkColor);
        WatermarkShadowColorBrush = ParseColorBrush(ImageGenerateOption.WatermarkShadowColor);
        WatermarkBorderColorBrush = ParseColorBrush(ImageGenerateOption.WatermarkBorderColor);
    }

    private static IBrush? ParseColorBrush(string colorHex)
    {
        try
        {
            return new SolidColorBrush(ParseColor(colorHex));
        }
        catch
        {
            return new SolidColorBrush(Colors.White);
        }
    }

    private static Color ParseColor(string colorHex)
    {
        // 移除 #
        string hex = colorHex.TrimStart('#');

        // 处理简写格式（如 #FFF -> #FFFFFF）
        if (hex.Length == 3)
        {
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";
        }

        // 解析 ARGB 或 RGB
        byte a, r, g, b;

        if (hex.Length == 8) // ARGB
        {
            a = Convert.ToByte(hex.Substring(0, 2), 16);
            r = Convert.ToByte(hex.Substring(2, 2), 16);
            g = Convert.ToByte(hex.Substring(4, 2), 16);
            b = Convert.ToByte(hex.Substring(6, 2), 16);
        }
        else if (hex.Length == 6) // RGB，默认不透明
        {
            a = 255;
            r = Convert.ToByte(hex.Substring(0, 2), 16);
            g = Convert.ToByte(hex.Substring(2, 2), 16);
            b = Convert.ToByte(hex.Substring(4, 2), 16);
        }
        else
        {
            // 默认白色
            return Colors.White;
        }

        return Color.FromArgb(a, r, g, b);
    }
}

public struct ExifInfoNode
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