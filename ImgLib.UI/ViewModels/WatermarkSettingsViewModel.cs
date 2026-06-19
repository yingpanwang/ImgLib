using Avalonia.Platform.Storage;
using System.Windows.Input;
namespace ImgLib.UI.ViewModels;

public partial class WatermarkSettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial WatermarkSettings Settings { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ExifInfoNode> ExifInfoTree { get; private set; } = new();

    [ObservableProperty]
    public partial ExifInfo? ExifInfo { set; get; }

    // 图片基础信息
    [ObservableProperty]
    public partial ImgFileDescViewModel ImageInfo { get; set; } = new();

    // ═══ 圆角预设 ═══
    private static readonly float[] CornerRadiusPresets = [0, 15, 45, 80, 120];

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
        [(0, 0), (20, 20), (50, 50), (80, 80)];

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
        [(0, 0), (1, 1), (2, 2), (4, 4), (8, 8)];

    [ObservableProperty]
    public partial int TextShadowOffsetPresetIndex { get; set; } = 2;

    [ObservableProperty]
    public partial bool IsTextShadowOffsetCustom { get; set; }

    partial void OnTextShadowOffsetPresetIndexChanged(int value)
    {
        if (value >= 0 && value < TextShadowOffsetPresets.Length)
        {
            IsTextShadowOffsetCustom = false;
            var (x, y) = TextShadowOffsetPresets[value];
            // 同步到选中的水印项
            if (SelectedWatermarkText != null)
            {
                SelectedWatermarkText.ShadowOffsetX = x;
                SelectedWatermarkText.ShadowOffsetY = y;
            }
        }
        else
        {
            IsTextShadowOffsetCustom = true;
        }
    }

    // ═══ 文字阴影模糊预设 ═══
    private static readonly float[] TextShadowSigmaPresets = [0, 2, 5, 10, 15];

    [ObservableProperty]
    public partial int TextShadowSigmaPresetIndex { get; set; } = 2;

    [ObservableProperty]
    public partial bool IsTextShadowSigmaCustom { get; set; }

    partial void OnTextShadowSigmaPresetIndexChanged(int value)
    {
        if (value >= 0 && value < TextShadowSigmaPresets.Length)
        {
            IsTextShadowSigmaCustom = false;
            // 同步到选中的水印项
            if (SelectedWatermarkText != null)
                SelectedWatermarkText.ShadowSigma = TextShadowSigmaPresets[value];
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

    // ═══ 多水印文本 ═══
    /// <summary>
    /// 水印文本项列表（绑定到 Settings.WatermarkTextItems）
    /// </summary>
    public ObservableCollection<WatermarkTextItemSettings> WatermarkTextItems => Settings.WatermarkTextItems;

    /// <summary>
    /// 当前选中的水印文本项索引
    /// </summary>
    [ObservableProperty]
    public partial int SelectedWatermarkTextIndex { get; set; }

    /// <summary>
    /// 当前选中的水印文本项（用于编辑面板绑定）
    /// </summary>
    public WatermarkTextItemSettings? SelectedWatermarkText
    {
        get
        {
            if (SelectedWatermarkTextIndex >= 0 && SelectedWatermarkTextIndex < WatermarkTextItems.Count)
                return WatermarkTextItems[SelectedWatermarkTextIndex];
            return null;
        }
    }

    /// <summary>
    /// 是否可以删除当前水印（至少保留一项时才能删除当前项，但总数 > 1 时允许）
    /// </summary>
    public bool CanRemoveWatermarkText => WatermarkTextItems.Count > 1;

    /// <summary>
    /// 当前选中项是否可以上移
    /// </summary>
    public bool CanMoveUp => SelectedWatermarkTextIndex > 0;

    /// <summary>
    /// 当前选中项是否可以下移
    /// </summary>
    public bool CanMoveDown => SelectedWatermarkTextIndex >= 0 && SelectedWatermarkTextIndex < WatermarkTextItems.Count - 1;

    /// <summary>
    /// 存储提供程序（由 View 注入，用于弹出保存文件对话框）
    /// </summary>
    public IStorageProvider? StorageProvider { get; set; }

    private WatermarkSettings? _currentSettings;

    public WatermarkSettingsViewModel() : this(null, null)
    {
    }

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

        // 订阅水印文本项变化
        SubscribeWatermarkTextItems(Settings.WatermarkTextItems);

        // 根据当前设置值初始化预设索引
        InitializePresetIndices();

        // 初始化水印文本选中索引
        if (Settings.WatermarkTextItems.Count > 0)
            SelectedWatermarkTextIndex = 0;

        // 注册消息：加载水印设置文件
        WeakReferenceMessenger.Default.Register<LoadWatermarkSettingsMessage>(this, (r, m) =>
        {
            Settings = m.Settings;
            ToastService.ShowSuccess("水印设置已加载");
        });
    }

    /// <summary>保存当前水印设置到系统 AppData 预设目录</summary>
    [RelayCommand(AllowConcurrentExecutions = false)]
    public async Task SaveWartermarkSettings()
    {
        try
        {
            var presetsDir = WatermarkSettingListViewModel.PresetsDirectory;

            // 确保目录存在
            if (!Directory.Exists(presetsDir))
                Directory.CreateDirectory(presetsDir);

            // 自动生成文件名：水印设置_yyyyMMdd_HHmmss.json
            var fileName = $"水印设置_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(presetsDir, fileName);

            var json = JsonSerializer.Serialize(Settings, ImgLibUIJsonContext.Default.WatermarkSettings);
            await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);

            ToastService.ShowSuccess($"水印设置已保存: {fileName}");
            WeakReferenceMessenger.Default.Send(new SettingsFileSavedMessage(filePath));
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"保存水印设置失败: {ex.Message}");
        }
    }

    /// <summary>添加新的水印文本项</summary>
    [RelayCommand]
    private void AddWatermarkText()
    {
        var newItem = new WatermarkTextItemSettings();
        newItem.PropertyChanged += OnWatermarkTextItemPropertyChanged;
        WatermarkTextItems.Add(newItem);
        SelectedWatermarkTextIndex = WatermarkTextItems.Count - 1;
        RefreshCanExecuteState();
        RequestPreview();
    }

    /// <summary>删除当前选中的水印文本项</summary>
    [RelayCommand(CanExecute = nameof(CanRemoveWatermarkText))]
    private void RemoveWatermarkText()
    {
        if (!CanRemoveWatermarkText || SelectedWatermarkTextIndex < 0)
            return;

        var idx = SelectedWatermarkTextIndex;
        var item = WatermarkTextItems[idx];
        item.PropertyChanged -= OnWatermarkTextItemPropertyChanged;
        WatermarkTextItems.RemoveAt(idx);

        // 调整选中索引
        if (idx >= WatermarkTextItems.Count)
            SelectedWatermarkTextIndex = WatermarkTextItems.Count - 1;
        else
            SelectedWatermarkTextIndex = idx;

        RefreshCanExecuteState();
        RequestPreview();
    }

    /// <summary>将当前选中的水印文本项上移</summary>
    [RelayCommand(CanExecute = nameof(CanMoveUp))]
    private void MoveWatermarkTextUp()
    {
        if (!CanMoveUp) return;

        var idx = SelectedWatermarkTextIndex;
        WatermarkTextItems.Move(idx, idx - 1);
        SelectedWatermarkTextIndex = idx - 1;
        RequestPreview();
    }

    /// <summary>将当前选中的水印文本项下移</summary>
    [RelayCommand(CanExecute = nameof(CanMoveDown))]
    private void MoveWatermarkTextDown()
    {
        if (!CanMoveDown) return;

        var idx = SelectedWatermarkTextIndex;
        WatermarkTextItems.Move(idx, idx + 1);
        SelectedWatermarkTextIndex = idx + 1;
        RequestPreview();
    }

    /// <summary>刷新 CanExecute 状态（CanRemove/CanMoveUp/CanMoveDown）</summary>
    private void RefreshCanExecuteState()
    {
        OnPropertyChanged(nameof(CanRemoveWatermarkText));
        OnPropertyChanged(nameof(CanMoveUp));
        OnPropertyChanged(nameof(CanMoveDown));
        OnPropertyChanged(nameof(SelectedWatermarkText));
        RemoveWatermarkTextCommand.NotifyCanExecuteChanged();
        MoveWatermarkTextUpCommand.NotifyCanExecuteChanged();
        MoveWatermarkTextDownCommand.NotifyCanExecuteChanged();
    }

    /// <summary>水印文本项属性变化时的处理（触发预览）</summary>
    private void OnWatermarkTextItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // 模板变化时刷新预览文本
        if (e.PropertyName == nameof(WatermarkTextItemSettings.Template))
        {
            UpdatePreviewText();
        }

        // 属性变化时更新选中项绑定
        OnPropertyChanged(nameof(SelectedWatermarkText));

        RequestPreview();
    }

    /// <summary>触发预览请求（带防抖由 WatermarkDesignViewModel 处理）</summary>
    private void RequestPreview()
    {
        var previewSettings = SystemSettingsService.Current.PreviewSettings;
        if (previewSettings.AutoPreview)
        {
            WeakReferenceMessenger.Default.Send(new PreviewRequestedMessage());
        }
    }

    /// <summary>手动触发预览（按钮点击，忽略自动预览开关）</summary>
    [RelayCommand]
    private void TriggerPreview()
    {
        WeakReferenceMessenger.Default.Send(new PreviewRequestedMessage());
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
            UnsubscribeWatermarkTextItems(_currentSettings.WatermarkTextItems);
        }

        // 监听新对象
        if (newValue != null)
        {
            _currentSettings = newValue;
            newValue.PropertyChanged += OnSettingsPropertyChanged;
            SubscribeWatermarkTextItems(newValue.WatermarkTextItems);
            InitializePresetIndices();

            // 选中第一个水印项
            if (newValue.WatermarkTextItems.Count > 0)
                SelectedWatermarkTextIndex = 0;
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

        using var exifDoc = JsonSerializer.SerializeToDocument(ExifInfo, ImgLibUIJsonContext.Default.ExifInfo);

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
        var alignment = value switch
        {
            0 => "Left",
            1 => "Center",
            2 => "Right",
            _ => "Center"
        };

        // 同步到选中项
        if (SelectedWatermarkText != null)
            SelectedWatermarkText.HorizontalAlignment = alignment;
    }

    partial void OnSelectedWatermarkTextIndexChanged(int value)
    {
        UpdatePreviewText();
        UpdateColorBrushes();
        RefreshCanExecuteState();
        OnPropertyChanged(nameof(SelectedWatermarkText));

        // 将选中项的值同步到扁平 Settings（使预设系统正常工作）
        if (SelectedWatermarkText != null)
        {
            HorizontalAlignIndex = SelectedWatermarkText.HorizontalAlignment switch
            {
                "Left" => 0,
                "Right" => 2,
                _ => 1,
            };

            InitializePresetIndices();
        }
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
            System.Diagnostics.Debug.WriteLine($"[WatermarkSettingsViewModel] 触发 PreviewRequested 消息");
            WeakReferenceMessenger.Default.Send(new PreviewRequestedMessage());
        }
    }

    private void UpdatePreviewText()
    {
        // 多水印模式：预览选中的水印项
        if (SelectedWatermarkText != null && ExifInfo != null)
        {
            var result = SelectedWatermarkText.Template;
            foreach (var kvp in ExifInfo.GetTemplateReplacements())
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value ?? "N/A");
            }
            PreviewWatermarkText = result;
        }
        else if (SelectedWatermarkText != null)
        {
            PreviewWatermarkText = SelectedWatermarkText.Template;
        }
        else
        {
            PreviewWatermarkText = string.Empty;
        }
    }

    private void UpdateColorBrushes()
    {
        // 多水印模式：使用选中项的颜色
        if (SelectedWatermarkText != null)
        {
            WatermarkColorBrush = ParseColorBrush(SelectedWatermarkText.ColorHex);
            WatermarkShadowColorBrush = ParseColorBrush(SelectedWatermarkText.ShadowColorHex);
            WatermarkBorderColorBrush = ParseColorBrush(SelectedWatermarkText.BorderColorHex);
        }
        else
        {
            WatermarkColorBrush = ParseColorBrush("#FFFFFF");
            WatermarkShadowColorBrush = ParseColorBrush("#80000000");
            WatermarkBorderColorBrush = ParseColorBrush("#00FF00");
        }
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
        var selText = SelectedWatermarkText;
        int tsoIdx = -1;
        for (int i = 0; i < TextShadowOffsetPresets.Length; i++)
        {
            if (Math.Abs(TextShadowOffsetPresets[i].X - (selText?.ShadowOffsetX ?? 0)) < 0.01f &&
                Math.Abs(TextShadowOffsetPresets[i].Y - (selText?.ShadowOffsetY ?? 0)) < 0.01f)
            {
                tsoIdx = i;
                break;
            }
        }
        TextShadowOffsetPresetIndex = tsoIdx >= 0 ? tsoIdx : TextShadowOffsetPresets.Length;
        IsTextShadowOffsetCustom = tsoIdx < 0;

        // 文字阴影模糊
        int tssIdx = Array.IndexOf(TextShadowSigmaPresets, selText?.ShadowSigma ?? 0);
        TextShadowSigmaPresetIndex = tssIdx >= 0 ? tssIdx : TextShadowSigmaPresets.Length;
        IsTextShadowSigmaCustom = tssIdx < 0;
    }

    /// <summary>订阅所有水印文本项的 PropertyChanged 事件</summary>
    private void SubscribeWatermarkTextItems(ObservableCollection<WatermarkTextItemSettings> items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged += OnWatermarkTextItemPropertyChanged;
        }
    }

    /// <summary>取消订阅所有水印文本项的 PropertyChanged 事件</summary>
    private void UnsubscribeWatermarkTextItems(ObservableCollection<WatermarkTextItemSettings> items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged -= OnWatermarkTextItemPropertyChanged;
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

public partial class ExifInfoNode : ObservableObject
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