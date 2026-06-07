using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ImgLib;
using ImgLib.Models;
using ImgLib.UI.Models;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using System.Text;

namespace ImgLib.UI.ViewModels;

public partial class WatermarkSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial WatermarkSettings Settings { get; private set; }

    [ObservableProperty]
    public partial ObservableCollection<ExifInfoNode> ExifInfoTree { get; private set; } = new();

    [ObservableProperty]
    public partial ExifInfo? ExifInfo { set; get; }

    // 控制是否显示直方图
    [ObservableProperty]
    public partial bool ShowHistogram { get; set; } = false;

    // 图片基础信息
    [ObservableProperty]
    public partial ImgFileDescViewModel ImageInfo { get; set; } = new();

    // 自动预览开关
    [ObservableProperty]
    public partial bool AutoPreview { get; set; } = false;

    // 启用预览降采样
    [ObservableProperty]
    public partial bool EnablePreviewDownsampling { get; set; } = true;

    // 预览降采样模式：true=按百分比，false=按固定像素值
    [ObservableProperty]
    public partial bool UsePreviewPercentMode { get; set; } = false;

    // 预览降采样最大边长（像素，固定模式）
    [ObservableProperty]
    public partial int PreviewMaxDimension { get; set; } = 1200;

    // 预览降采样百分比（百分比模式）
    [ObservableProperty]
    public partial float PreviewMaxPercent { get; set; } = 50f;

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

    // 水印行间距系数
    [ObservableProperty]
    public partial float WatermarkLineSpacing { get; set; } = 1.2f;

    // 自动缩放字体以适应水印区域
    [ObservableProperty]
    public partial bool WatermarkAutoFitFont { get; set; } = false;

    // 水平对齐索引
    [ObservableProperty]
    public partial int HorizontalAlignIndex { get; set; } = 1;

    // 预览命令（由外部注入）
    public ICommand? PreviewCommand { get; set; }

    // 预览触发事件（用于自动预览）
    public event EventHandler? PreviewRequested;

    private WatermarkSettings? _currentSettings;

    public WatermarkSettingsViewModel(ImageGenerateOption? option = null, ExifInfo? exifInfo = null)
    {
        Settings = new WatermarkSettings();

        if (option != null)
        {
            Settings.FromImageGenerateOption(option);
        }

        ExifInfo = exifInfo;

        UpdatePreviewText();
        UpdateColorBrushes();

        // 监听 Settings 属性变化
        _currentSettings = Settings;
        _currentSettings.PropertyChanged += OnSettingsPropertyChanged;
    }

    partial void OnSettingsChanged(WatermarkSettings oldValue, WatermarkSettings newValue)
    {
        // 移除旧对象的监听
        if (_currentSettings != null)
        {
            _currentSettings.PropertyChanged -= OnSettingsPropertyChanged;
        }

        // 监听新对象
        if (newValue != null)
        {
            _currentSettings = newValue;
            newValue.PropertyChanged += OnSettingsPropertyChanged;
        }
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

    partial void OnHorizontalAlignIndexChanged(int value)
    {
        Settings.WatermarkHorizontalAlignment = value switch
        {
            0 => "Left",
            1 => "Center",
            2 => "Right",
            _ => "Center"
        };
    }

    partial void OnEnablePreviewDownsamplingChanged(bool value)
    {
        // 启用/禁用预览降采样时触发预览
        if (AutoPreview)
        {
            PreviewRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    partial void OnUsePreviewPercentModeChanged(bool value)
    {
        // 切换降采样模式时触发预览
        if (AutoPreview)
        {
            PreviewRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    partial void OnPreviewMaxDimensionChanged(int value)
    {
        // 降采样参数变化时触发预览
        if (AutoPreview)
        {
            PreviewRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    partial void OnPreviewMaxPercentChanged(float value)
    {
        // 百分比参数变化时触发预览
        if (AutoPreview)
        {
            PreviewRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // 更新预览文本和颜色画笔
        UpdatePreviewText();
        UpdateColorBrushes();

        // 自动预览
        System.Diagnostics.Debug.WriteLine($"[WatermarkSettingsViewModel] 属性变化: {e.PropertyName}, AutoPreview={AutoPreview}");
        if (AutoPreview)
        {
            System.Diagnostics.Debug.WriteLine($"[WatermarkSettingsViewModel] 触发 PreviewRequested 事件");
            PreviewRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdatePreviewText()
    {
        var option = Settings.ToImageGenerateOption();
        PreviewWatermarkText = option.ParseWatermarkTemplate(ExifInfo);
    }

    private void UpdateColorBrushes()
    {
        WatermarkColorBrush = ParseColorBrush(Settings.WatermarkColor);
        WatermarkShadowColorBrush = ParseColorBrush(Settings.WatermarkShadowColor);
        WatermarkBorderColorBrush = ParseColorBrush(Settings.WatermarkBorderColor);
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