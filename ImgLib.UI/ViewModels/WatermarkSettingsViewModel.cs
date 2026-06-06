using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ImgLib;
using ImgLib.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
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

    // 直方图 Path 数据属性 (用于美化显示)
    [ObservableProperty]
    public partial string HistogramRPath { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string HistogramGPath { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string HistogramBPath { get; private set; } = string.Empty;

    // 用于存储图片路径，当图片路径改变时计算直方图
    [ObservableProperty]
    public partial string? ImageFilePath { get; set; }

    // 控制是否显示直方图
    [ObservableProperty]
    public partial bool ShowHistogram { get; set; } = false;

    // 直方图加载状态
    [ObservableProperty]
    public partial bool IsHistogramLoading { get; private set; } = false;

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

    private CancellationTokenSource? _histogramCts;

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

    partial void OnImageFilePathChanged(string? value)
    {
        if (string.IsNullOrEmpty(value) || !File.Exists(value))
        {
            HistogramRPath = string.Empty;
            HistogramGPath = string.Empty;
            HistogramBPath = string.Empty;
            return;
        }

        // 取消之前的计算任务
        _histogramCts?.Cancel();
        _histogramCts = new CancellationTokenSource();

        // 立即清空旧数据，避免暂留感
        HistogramRPath = string.Empty;
        HistogramGPath = string.Empty;
        HistogramBPath = string.Empty;
        IsHistogramLoading = true;

        Task.Run(() => CalculateHistogram(value!, _histogramCts.Token), _histogramCts.Token);
    }

    private void CalculateHistogram(string filePath, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            using var bitmap = SKBitmap.Decode(stream);

            if (bitmap == null || cancellationToken.IsCancellationRequested)
                return;

            // 初始化直方图数组 (0-255)
            int[] histR = new int[256];
            int[] histG = new int[256];
            int[] histB = new int[256];

            // 计算直方图 - 使用采样提高性能
            int step = Math.Max(1, Math.Min(bitmap.Width, bitmap.Height) / 1000); // 自适应采样步长

            for (int y = 0; y < bitmap.Height; y += step)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                for (int x = 0; x < bitmap.Width; x += step)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    histR[pixel.Red]++;
                    histG[pixel.Green]++;
                    histB[pixel.Blue]++;
                }
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            // 找到最大值用于归一化 (使用所有通道的最大值，保持一致的比例)
            int maxValue = new[] { histR.Max(), histG.Max(), histB.Max() }.Max();

            // 生成 Path 数据 (归一化到 0-1000 的坐标空间)
            string pathR = GenerateHistogramPath(histR, maxValue);
            string pathG = GenerateHistogramPath(histG, maxValue);
            string pathB = GenerateHistogramPath(histB, maxValue);

            if (cancellationToken.IsCancellationRequested)
                return;

            // 更新 UI
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    HistogramRPath = pathR;
                    HistogramGPath = pathG;
                    HistogramBPath = pathB;
                    IsHistogramLoading = false;
                }
            });
        }
        catch
        {
            // 处理异常
        }
        finally
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                IsHistogramLoading = false;
            });
        }
    }

    private string GenerateHistogramPath(int[] histogram, int maxValue)
    {
        if (maxValue == 0)
            return string.Empty;

        const double width = 1000;
        const double height = 1000;

        var sb = new StringBuilder();

        // 起始点 (左下角)
        sb.Append($"M 0,{height} ");

        // 生成路径点
        for (int i = 0; i < 256; i++)
        {
            double x = (i / 255.0) * width;
            double y = height - (histogram[i] / (double)maxValue) * height;
            sb.Append($"L {x:F1},{y:F1} ");
        }

        // 闭合路径到右下角
        sb.Append($"L {width},{height} Z");

        return sb.ToString();
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