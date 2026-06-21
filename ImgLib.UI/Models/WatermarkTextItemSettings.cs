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

    // ═══════════════════════════════════════════
    // 高级定位
    // ═══════════════════════════════════════════

    [ObservableProperty]
    public partial bool UseAdvancedPositioning { get; set; } = false;

    partial void OnUseAdvancedPositioningChanged(bool value)
    {
        if (value)
        {
            // 开启高级模式 → 从简单值换算初始值
            PositionX = HorizontalAlignment switch
            {
                "Left" => 0.05f,
                "Right" => 0.95f,
                _ => 0.5f
            };
            PositionY = 1.0f - VerticalPosition;
            AdvancedHAlign = HorizontalAlignment;
            AdvancedVAlign = "Bottom";
        }
        else
        {
            // 关闭高级模式 → 从高级值反算简单值
            VerticalPosition = 1.0f - PositionY;
            HorizontalAlignment = AdvancedHAlign;
        }
    }

    [ObservableProperty]
    public partial float PositionX { get; set; } = 0.5f;

    [ObservableProperty]
    public partial float PositionY { get; set; } = 0.5f;

    [ObservableProperty]
    public partial string AdvancedHAlign { get; set; } = "Center";

    [ObservableProperty]
    public partial string AdvancedVAlign { get; set; } = "Bottom";

    [ObservableProperty]
    public partial bool Repeat { get; set; } = false;

    [ObservableProperty]
    public partial float RepeatSpacingX { get; set; } = 0.3f;

    [ObservableProperty]
    public partial float RepeatSpacingY { get; set; } = 0.2f;

    [ObservableProperty]
    public partial float RepeatAngle { get; set; } = 0f;

    [ObservableProperty]
    public partial bool RelativeToCanvas { get; set; } = false;

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
        UseAdvancedPositioning = item.UseAdvancedPositioning;
        PositionX = item.PositionX;
        PositionY = item.PositionY;
        AdvancedHAlign = item.AdvancedHAlign;
        AdvancedVAlign = item.AdvancedVAlign;
        Repeat = item.Repeat;
        RepeatSpacingX = item.RepeatSpacingX;
        RepeatSpacingY = item.RepeatSpacingY;
        RepeatAngle = item.RepeatAngle;
        RelativeToCanvas = item.RelativeToCanvas;
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
            UseAdvancedPositioning = UseAdvancedPositioning,
            PositionX = PositionX,
            PositionY = PositionY,
            AdvancedHAlign = AdvancedHAlign,
            AdvancedVAlign = AdvancedVAlign,
            Repeat = Repeat,
            RepeatSpacingX = RepeatSpacingX,
            RepeatSpacingY = RepeatSpacingY,
            RepeatAngle = RepeatAngle,
            RelativeToCanvas = RelativeToCanvas,
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
