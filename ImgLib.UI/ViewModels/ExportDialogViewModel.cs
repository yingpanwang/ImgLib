using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ImgLib.UI.ViewModels;

public partial class ExportDialogViewModel : ViewModelBase
{
    /// <summary>
    /// 用户确认导出（true）还是取消（false）
    /// </summary>
    public bool Confirmed { get; private set; }

    /// <summary>
    /// 用户最终选中的文件路径列表
    /// </summary>
    public IReadOnlyList<string> SelectedPaths =>
        Items.Where(i => i.IsSelected).Select(i => i.FilePath).ToList();

    /// <summary>
    /// 导出选择列表
    /// </summary>
    public ObservableCollection<ExportDialogItemViewModel> Items { get; } = [];

    /// <summary>
    /// 已选中数量
    /// </summary>
    [ObservableProperty]
    public partial int SelectedCount { get; set; }

    /// <summary>
    /// 总数
    /// </summary>
    [ObservableProperty]
    public partial int TotalCount { get; set; }

    /// <summary>
    /// 全选 / 取消全选 状态（null = 部分选中）
    /// </summary>
    [ObservableProperty]
    public partial bool? IsAllSelected { get; private set; }

    /// <summary>
    /// 状态文本（如 "已选 5 / 20 张"）
    /// </summary>
    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// 从文件路径列表初始化选择项（默认全部选中）
    /// </summary>
    public void Initialize(IReadOnlyList<string> filePaths)
    {
        Items.Clear();
        TotalCount = filePaths.Count;

        foreach (var path in filePaths)
        {
            var item = new ExportDialogItemViewModel(path)
            {
                IsSelected = false
            };
            item.PropertyChanged += OnItemPropertyChanged;
            Items.Add(item);
        }

        RefreshState();
    }

    /// <summary>
    /// 切换全选 / 取消全选
    /// </summary>
    [RelayCommand]
    public void ToggleSelectAll()
    {
        bool newValue = IsAllSelected != true; // 当前非全选 → 设为全选

        foreach (var item in Items)
        {
            item.IsSelected = newValue;
        }

        RefreshState();
    }

    /// <summary>
    /// 确认导出
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        Confirmed = true;
    }

    /// <summary>
    /// 取消导出
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        Confirmed = false;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExportDialogItemViewModel.IsSelected))
        {
            RefreshState();
        }
    }

    private void RefreshState()
    {
        int selected = Items.Count(i => i.IsSelected);
        SelectedCount = selected;

        if (selected == 0)
            IsAllSelected = false;
        else if (selected == Items.Count)
            IsAllSelected = true;
        else
            IsAllSelected = null;

        StatusText = $"已选 {selected} / {TotalCount} 张";
    }
}
