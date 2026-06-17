using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImgLib.UI.Models;

/// <summary>
/// 单个水印文本项的 UI 绑定模型，实现 <see cref="INotifyPropertyChanged"/>。
/// 与 <see cref="ImgLib.WatermarkTextItem"/> 对应，负责双向绑定。
/// </summary>
public class WatermarkTextItemSettings : INotifyPropertyChanged
{
    private string _template = "{Model} | {LensModel} | f/{FNumber} | ISO {ISO} | {ExposureTime}";
    public string Template
    {
        get => _template;
        set { _template = value; OnPropertyChanged(); }
    }

    private string _colorHex = "#FFFFFF";
    public string ColorHex
    {
        get => _colorHex;
        set { _colorHex = value; OnPropertyChanged(); }
    }

    private float _fontSizeRatio = 0.03f;
    public float FontSizeRatio
    {
        get => _fontSizeRatio;
        set { _fontSizeRatio = value; OnPropertyChanged(); }
    }

    private bool _bold = true;
    public bool Bold
    {
        get => _bold;
        set { _bold = value; OnPropertyChanged(); }
    }

    private float _lineSpacing = 1.2f;
    public float LineSpacing
    {
        get => _lineSpacing;
        set { _lineSpacing = value; OnPropertyChanged(); }
    }

    private bool _autoFitFont = false;
    public bool AutoFitFont
    {
        get => _autoFitFont;
        set { _autoFitFont = value; OnPropertyChanged(); }
    }

    private float _verticalPosition = 0.5f;
    public float VerticalPosition
    {
        get => _verticalPosition;
        set { _verticalPosition = value; OnPropertyChanged(); }
    }

    private string _horizontalAlignment = "Center";
    public string HorizontalAlignment
    {
        get => _horizontalAlignment;
        set { _horizontalAlignment = value; OnPropertyChanged(); }
    }

    // ─── 投影参数 ───
    private float _shadowOffsetX = 2f;
    public float ShadowOffsetX
    {
        get => _shadowOffsetX;
        set { _shadowOffsetX = value; OnPropertyChanged(); }
    }

    private float _shadowOffsetY = 2f;
    public float ShadowOffsetY
    {
        get => _shadowOffsetY;
        set { _shadowOffsetY = value; OnPropertyChanged(); }
    }

    private float _shadowSigma = 5f;
    public float ShadowSigma
    {
        get => _shadowSigma;
        set { _shadowSigma = value; OnPropertyChanged(); }
    }

    private string _shadowColorHex = "#80000000";
    public string ShadowColorHex
    {
        get => _shadowColorHex;
        set { _shadowColorHex = value; OnPropertyChanged(); }
    }

    // ─── 调试边框 ───
    private bool _showBorder = false;
    public bool ShowBorder
    {
        get => _showBorder;
        set { _showBorder = value; OnPropertyChanged(); }
    }

    private string _borderColorHex = "#00FF00";
    public string BorderColorHex
    {
        get => _borderColorHex;
        set { _borderColorHex = value; OnPropertyChanged(); }
    }

    private float _borderWidth = 2f;
    public float BorderWidth
    {
        get => _borderWidth;
        set { _borderWidth = value; OnPropertyChanged(); }
    }

    // ─── 显示标签 ───
    /// <summary>列表中的显示名称（模板截取或自定义名称）</summary>
    public string DisplayLabel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_template))
            {
                // 截取前 40 字符作为预览，去除换行
                var preview = _template.Replace('\n', ' ').Replace('\r', ' ');
                return preview.Length > 40 ? preview[..40] + "..." : preview;
            }
            return "(空水印)";
        }
    }

    /// <summary>从 <see cref="ImgLib.WatermarkTextItem"/> 复制值</summary>
    public void FromWatermarkTextItem(WatermarkTextItem item)
    {
        Template = item.Template;
        ColorHex = item.ColorHex;
        FontSizeRatio = item.FontSizeRatio;
        Bold = item.Bold;
        LineSpacing = item.LineSpacing;
        AutoFitFont = item.AutoFitFont;
        VerticalPosition = item.VerticalPosition;
        HorizontalAlignment = item.HorizontalAlignment;
        ShadowOffsetX = item.ShadowOffsetX;
        ShadowOffsetY = item.ShadowOffsetY;
        ShadowSigma = item.ShadowSigma;
        ShadowColorHex = item.ShadowColorHex;
        ShowBorder = item.ShowBorder;
        BorderColorHex = item.BorderColorHex;
        BorderWidth = item.BorderWidth;
    }

    /// <summary>转换为 <see cref="ImgLib.WatermarkTextItem"/></summary>
    public WatermarkTextItem ToWatermarkTextItem()
    {
        return new WatermarkTextItem
        {
            Template = Template,
            ColorHex = ColorHex,
            FontSizeRatio = FontSizeRatio,
            Bold = Bold,
            LineSpacing = LineSpacing,
            AutoFitFont = AutoFitFont,
            VerticalPosition = VerticalPosition,
            HorizontalAlignment = HorizontalAlignment,
            ShadowOffsetX = ShadowOffsetX,
            ShadowOffsetY = ShadowOffsetY,
            ShadowSigma = ShadowSigma,
            ShadowColorHex = ShadowColorHex,
            ShowBorder = ShowBorder,
            BorderColorHex = BorderColorHex,
            BorderWidth = BorderWidth,
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        // Template 变更时也刷新 DisplayLabel
        if (propertyName == nameof(Template))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayLabel)));

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>手动刷新 DisplayLabel 绑定</summary>
    public void RefreshDisplayLabel()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayLabel)));
    }
}
