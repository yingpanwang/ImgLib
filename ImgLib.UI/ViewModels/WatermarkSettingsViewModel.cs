using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ImgLib;
using ImgLib.Models;
using ImgLib.UI.Models;
using ImgLib.UI.Services;
using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using System.Text;
using System.ComponentModel;

namespace ImgLib.UI.ViewModels;

public partial class WatermarkSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial WatermarkSettings Settings { get; private set; }

    [ObservableProperty]
    public partial ObservableCollection<ExifInfoNode> ExifInfoTree { get; private set; } = new();

    [ObservableProperty]
    public partial ExifInfo? ExifInfo { set; get; }

    // 图片基础信息
    [ObservableProperty]
    public partial ImgFileDescViewModel ImageInfo { get; set; } = new();

    // ═══ 圆角预设 ═══
    private static readonly float[] CornerRadiusPresets = { 0, 15, 45, 80, 120 };

    [ObservableProperty]
    public partial int CornerRadiusPresetIndex { get; set; } = 2;

    [ObservableProperty]
    public partial bool IsCornerRadiusCustom { get; set; }

    partial void OnCornerRadiusPresetIndexChanged(int value)
    {
        if (value >= 0 && value < CornerRadiusPresets.Length)
        {
            IsCornerRadiusCustom = false;
            Settings.CornerRadius = CornerRadiusPresets[value];
        }
        else
        {
            IsCornerRadiusCustom = true;
        }
    }

    // ═══ 虚化预设 ═══
    private static readonly float[] BlurSigmaPresets = { 0, 10, 25, 50, 80 };

    [ObservableProperty]
    public partial int BlurSigmaPresetIndex { get; set; } = 2;

    [ObservableProperty]
    public partial bool IsBlurSigmaCustom { get; set; }

    partial void OnBlurSigmaPresetIndexChanged(int value)
    {
        if (value >= 0 && value < BlurSigmaPresets.Length)
        {
            IsBlurSigmaCustom = false;
            Settings.BlurSigma = BlurSigmaPresets[value];
        }
        else
        {
            IsBlurSigmaCustom = true;
        }
    }

    // ═══ 阴影偏移预设 ═══
    private static readonly (float X, float Y)[] ShadowOffsetPresets =
        { (0, 0), (20, 20), (50, 50), (80, 80) };

    [ObservableProperty]
    public partial int ShadowOffsetPresetIndex { get; set; } = 2;

    [ObservableProperty]
    public partial bool IsShadowOffsetCustom { get; set; }

    partial void OnShadowOffsetPresetIndexChanged(int value)
    {
        if (value >= 0 && value < ShadowOffsetPresets.Length)
        {
            IsShadowOffsetCustom = false;
            (Settings.ShadowOffsetX, Settings.ShadowOffsetY) = ShadowOffsetPresets[value];
        }
        else
        {
            IsShadowOffsetCustom = true;
        }
    }

    // ═══ 文字阴影偏移预设 ═══
    private static readonly (float X, float Y)[] TextShadowOffsetPresets =
        { (0, 0), (1, 1), (2, 2), (4, 4), (8, 8) };

    [ObservableProperty]
    public partial int TextShadowOffsetPresetIndex { get; set; } = 2;

    [ObservableProperty]
    public partial bool IsTextShadowOffsetCustom { get; set; }

    partial void OnTextShadowOffsetPresetIndexChanged(int value)
    {
        if (value >= 0 && value < TextShadowOffsetPresets.Length)
        {
            IsTextShadowOffsetCustom = false;
            (Settings.WatermarkShadowOffsetX, Settings.WatermarkShadowOffsetY) = TextShadowOffsetPresets[value];
        }
        else
        {
            IsTextShadowOffsetCustom = true;
        }
    }

    // ═══ 文字阴影模糊预设 ═══
    private static readonly float[] TextShadowSigmaPresets = { 0, 2, 5, 10, 15 };

    [ObservableProperty]
    public partial int TextShadowSigmaPresetIndex { get; set; } = 2;

    [ObservableProperty]
    public partial bool IsTextShadowSigmaCustom { get; set; }

    partial void OnTextShadowSigmaPresetIndexChanged(int value)
    {
        if (value >= 0 && value < TextShadowSigmaPresets.Length)
        {
            IsTextShadowSigmaCustom = false;
            Settings.WatermarkShadowSigma = TextShadowSigmaPresets[value];
        }
        else
        {
            IsTextShadowSigmaCustom = true;
        }
    }

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

    // ═══ EXIF 字段选择 ═══
    /// <summary>
    /// 可选的 EXIF 字段列表（从配置文件加载显示名称映射）
    /// </summary>
    public ObservableCollection<ExifFieldItem> ExifFieldItems { get; } = new();

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

        LoadExifFieldItems();

        UpdatePreviewText();
        UpdateColorBrushes();

        // 监听 Settings 属性变化
        _currentSettings = Settings;
        _currentSettings.PropertyChanged += OnSettingsPropertyChanged;

        // 根据当前设置值初始化预设索引
        InitializePresetIndices();
    }

    /// <summary>
    /// 从配置文件加载 EXIF 字段项
    /// </summary>
    private void LoadExifFieldItems()
    {
        var mappings = ExifFieldConfigService.LoadMappings();
        ExifFieldItems.Clear();
        foreach (var kvp in mappings)
        {
            ExifFieldItems.Add(new ExifFieldItem(kvp.Key, kvp.Value));
        }
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
            InitializePresetIndices();
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

    private void OnSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // 更新预览文本和颜色画笔
        UpdatePreviewText();
        UpdateColorBrushes();

        var previewSettings = SystemSettingsService.Current.PreviewSettings;
        // 自动预览
        System.Diagnostics.Debug.WriteLine($"[WatermarkSettingsViewModel] 属性变化: {e.PropertyName}, AutoPreview={previewSettings.AutoPreview}");
        if (previewSettings.AutoPreview)
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

    /// <summary>
    /// 根据当前 Settings 值检测匹配的预设，初始化预设索引和自定义模式标志
    /// </summary>
    private void InitializePresetIndices()
    {
        // 圆角
        int crIdx = Array.IndexOf(CornerRadiusPresets, Settings.CornerRadius);
        CornerRadiusPresetIndex = crIdx >= 0 ? crIdx : CornerRadiusPresets.Length;
        IsCornerRadiusCustom = crIdx < 0;

        // 虚化
        int bsIdx = Array.IndexOf(BlurSigmaPresets, Settings.BlurSigma);
        BlurSigmaPresetIndex = bsIdx >= 0 ? bsIdx : BlurSigmaPresets.Length;
        IsBlurSigmaCustom = bsIdx < 0;

        // 阴影偏移 (需要同时匹配 X 和 Y)
        int soIdx = -1;
        for (int i = 0; i < ShadowOffsetPresets.Length; i++)
        {
            if (Math.Abs(ShadowOffsetPresets[i].X - Settings.ShadowOffsetX) < 0.01f &&
                Math.Abs(ShadowOffsetPresets[i].Y - Settings.ShadowOffsetY) < 0.01f)
            {
                soIdx = i;
                break;
            }
        }
        ShadowOffsetPresetIndex = soIdx >= 0 ? soIdx : ShadowOffsetPresets.Length;
        IsShadowOffsetCustom = soIdx < 0;

        // 文字阴影偏移
        int tsoIdx = -1;
        for (int i = 0; i < TextShadowOffsetPresets.Length; i++)
        {
            if (Math.Abs(TextShadowOffsetPresets[i].X - Settings.WatermarkShadowOffsetX) < 0.01f &&
                Math.Abs(TextShadowOffsetPresets[i].Y - Settings.WatermarkShadowOffsetY) < 0.01f)
            {
                tsoIdx = i;
                break;
            }
        }
        TextShadowOffsetPresetIndex = tsoIdx >= 0 ? tsoIdx : TextShadowOffsetPresets.Length;
        IsTextShadowOffsetCustom = tsoIdx < 0;

        // 文字阴影模糊
        int tssIdx = Array.IndexOf(TextShadowSigmaPresets, Settings.WatermarkShadowSigma);
        TextShadowSigmaPresetIndex = tssIdx >= 0 ? tssIdx : TextShadowSigmaPresets.Length;
        IsTextShadowSigmaCustom = tssIdx < 0;
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

/// <summary>
/// 可选的 EXIF 字段项，供右键菜单选择插入到模板。
/// Key = ExifInfo 属性名（如 "Model"），Placeholder = {"{Key}"}，DisplayName 来自配置文件。
/// </summary>
public class ExifFieldItem
{
    /// <summary>ExifInfo 属性名，如 "Model"、"FNumber"</summary>
    public string Key { get; }

    /// <summary>模板占位符，如 "{Model}"</summary>
    public string Placeholder { get; }

    /// <summary>显示名称，来自配置文件映射，如 "相机型号"</summary>
    public string DisplayName { get; }

    public ExifFieldItem(string key, string displayName)
    {
        Key = key;
        DisplayName = displayName;
        Placeholder = $"{{{key}}}";
    }

    public override string ToString() => $"{DisplayName}  →  {Placeholder}";
}