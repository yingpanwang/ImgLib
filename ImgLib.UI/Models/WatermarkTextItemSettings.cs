namespace ImgLib.UI.Models;

/// <summary>
/// 单个水印文本项的 UI 绑定模型
/// </summary>
public partial class WatermarkTextItemSettings : ObservableObject
{
    [ObservableProperty]
    public partial string Template { get; set; } = "{Model} | {LensModel} | f/{FNumber} | ISO {ISO} | {ExposureTime}";

    partial void OnTemplateChanged(string value) => OnPropertyChanged(nameof(DisplayLabel));

    [ObservableProperty]
    public partial string ColorHex { get; set; } = "#FFFFFF";

    [ObservableProperty]
    public partial float FontSizeRatio { get; set; } = 0.03f;

    [ObservableProperty]
    public partial bool Bold { get; set; } = true;

    [ObservableProperty]
    public partial float LineSpacing { get; set; } = 1.2f;

    [ObservableProperty]
    public partial bool AutoFitFont { get; set; } = false;

    [ObservableProperty]
    public partial float VerticalPosition { get; set; } = 0.5f;

    [ObservableProperty]
    public partial string HorizontalAlignment { get; set; } = "Center";

    // ─── 投影参数 ───
    [ObservableProperty]
    public partial float ShadowOffsetX { get; set; } = 2f;

    [ObservableProperty]
    public partial float ShadowOffsetY { get; set; } = 2f;

    [ObservableProperty]
    public partial float ShadowSigma { get; set; } = 5f;

    [ObservableProperty]
    public partial string ShadowColorHex { get; set; } = "#80000000";

    // ─── 调试边框 ───
    [ObservableProperty]
    public partial bool ShowBorder { get; set; } = false;

    [ObservableProperty]
    public partial string BorderColorHex { get; set; } = "#00FF00";

    [ObservableProperty]
    public partial float BorderWidth { get; set; } = 2f;

    // ─── 显示标签 ───
    /// <summary>列表中的显示名称（模板截取或自定义名称）</summary>
    public string DisplayLabel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Template))
            {
                // 截取前 40 字符作为预览，去除换行
                var preview = Template.Replace('\n', ' ').Replace('\r', ' ');
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

    /// <summary>
    /// 手动刷新 DisplayLabel 绑定
    /// </summary>
    public void RefreshDisplayLabel()
    {
        OnPropertyChanged(nameof(DisplayLabel));
    }
}
