using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImgLib.UI.ViewModels;

/// <summary>
/// RGB直方图数据模型
/// </summary>
public class HistogramData
{
    public string RPath { get; set; } = string.Empty;
    public string GPath { get; set; } = string.Empty;
    public string BPath { get; set; } = string.Empty;
    public bool IsEmpty => string.IsNullOrEmpty(RPath) && string.IsNullOrEmpty(GPath) && string.IsNullOrEmpty(BPath);
}

/// <summary>
/// 直方图视图模型
/// </summary>
public partial class HistogramViewModel : ViewModelBase
{
    [ObservableProperty]
    private HistogramData histogramData = new();

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string? imageFilePath;

    private CancellationTokenSource? _cts;

    partial void OnImageFilePathChanged(string? value)
    {
        LoadHistogram(value);
    }

    /// <summary>
    /// 加载直方图
    /// </summary>
    public void LoadHistogram(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            HistogramData = new HistogramData();
            return;
        }

        // 取消之前的计算任务
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        // 清空旧数据
        HistogramData = new HistogramData();
        IsLoading = true;

        Task.Run(() => CalculateHistogram(filePath, _cts.Token), _cts.Token);
    }

    /// <summary>
    /// 计算直方图
    /// </summary>
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
            int step = Math.Max(1, Math.Min(bitmap.Width, bitmap.Height) / 1000);

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

            // 找到最大值用于归一化
            int maxValue = new[] { histR.Max(), histG.Max(), histB.Max() }.Max();

            // 生成 Path 数据
            var data = new HistogramData
            {
                RPath = GenerateHistogramPath(histR, maxValue),
                GPath = GenerateHistogramPath(histG, maxValue),
                BPath = GenerateHistogramPath(histB, maxValue)
            };

            if (cancellationToken.IsCancellationRequested)
                return;

            // 更新 UI
            Dispatcher.UIThread.Post(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    HistogramData = data;
                    IsLoading = false;
                }
            });
        }
        catch
        {
            // 处理异常
        }
        finally
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsLoading = false;
            });
        }
    }

    /// <summary>
    /// 生成直方图 SVG 路径字符串
    /// </summary>
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