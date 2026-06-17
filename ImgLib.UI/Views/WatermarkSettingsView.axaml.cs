using Avalonia.Controls;
using ImgLib.UI.ViewModels;
using System.ComponentModel;

namespace ImgLib.UI;

public partial class WatermarkSettingsView : UserControl
{
    private WatermarkSettingsViewModel? _viewModel;

    public WatermarkSettingsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        // 取消旧 ViewModel 的订阅
        _viewModel?.PropertyChanged -= OnViewModelPropertyChanged;

        if (DataContext is not WatermarkSettingsViewModel vm)
            return;

        _viewModel = vm;
        vm.PropertyChanged += OnViewModelPropertyChanged;

        BuildContextMenu(vm);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // ExifInfo 变化（切换图片）重建菜单以显示新的实际值
        if (e.PropertyName == nameof(WatermarkSettingsViewModel.ExifInfo) && _viewModel != null)
        {
            BuildContextMenu(_viewModel);
        }
    }

    private void BuildContextMenu(WatermarkSettingsViewModel vm)
    {
        var menuFlyout = new MenuFlyout();

        var addExifItem = new MenuItem
        {
            Header = "插入 EXIF 字段"
        };

        // 动态添加 EXIF 字段菜单项（含实际值预览）
        foreach (var field in vm.ExifFieldItems)
        {
            var resolvedValue = ResolveValue(field.Placeholder, vm);

            var header = resolvedValue is { Length: > 0 }
                ? $"{field.DisplayName}: {resolvedValue}  →  {field.Placeholder}"
                : $"{field.DisplayName}  →  {field.Placeholder}";

            var exifItem = new MenuItem
            {
                Header = header
            };
            exifItem.Click += (_, _) =>
            {
                InsertAtCursor(field.Placeholder);
            };
            addExifItem.Items.Add(exifItem);
        }
        menuFlyout.Items.Add(addExifItem);

        // 分隔线和清空选项
        menuFlyout.Items.Add(new Separator());
        var clearItem = new MenuItem { Header = "清空模板" };
        clearItem.Click += (_, _) =>
        {
            if (vm.SelectedWatermarkText != null)
                vm.SelectedWatermarkText.Template = string.Empty;
        };
        menuFlyout.Items.Add(clearItem);

        TemplateTextBox.ContextFlyout = menuFlyout;
    }

    /// <summary>
    /// 解析单个占位符的实际 EXIF 值。
    /// 直接在 ExifInfo 的替换字典中查找。
    /// </summary>
    private static string? ResolveValue(string placeholder, WatermarkSettingsViewModel vm)
    {
        if (vm.ExifInfo == null) return null;

        // 从占位符提取键名: "{Model}" → "Model"
        var key = placeholder.Trim('{', '}');
        var replacements = vm.ExifInfo.GetTemplateReplacements();

        return replacements.TryGetValue(key, out var value) ? value : null;
    }

    private void InsertAtCursor(string text)
    {
        var caret = TemplateTextBox.CaretIndex;
        TemplateTextBox.Text = TemplateTextBox.Text?.Insert(caret, text) ?? text;
        TemplateTextBox.CaretIndex = caret + text.Length;
    }
}
