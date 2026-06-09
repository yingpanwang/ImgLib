using CommunityToolkit.Mvvm.Input;
using System.Threading;

namespace ImgLib.UI.ViewModels;

public sealed partial class ExportProgressViewModel : ViewModelBase
{
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    public partial bool IsVisible { get; set; }

    [ObservableProperty]
    public partial bool IsExporting { get; set; }

    [ObservableProperty]
    public partial int TotalCount { get; set; }

    [ObservableProperty]
    public partial int CompletedCount { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFailures))]
    public partial int FailedCount { get; set; }

    public bool HasFailures => FailedCount > 0;

    /// <summary>
    /// 当前正在并行处理的文件数
    /// </summary>
    [ObservableProperty]
    public partial int ActiveCount { get; set; }

    /// <summary>
    /// 并行度（最大并发数）
    /// </summary>
    [ObservableProperty]
    public partial int MaxConcurrency { get; set; }

    [ObservableProperty]
    public partial string CurrentFileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double Progress { get; set; }

    /// <summary>
    /// 重置进度状态，准备新一轮导出
    /// </summary>
    public void BeginExport(int totalCount, CancellationTokenSource cts, int maxConcurrency)
    {
        _cts = cts;
        TotalCount = totalCount;
        CompletedCount = 0;
        FailedCount = 0;
        ActiveCount = 0;
        MaxConcurrency = maxConcurrency;
        CurrentFileName = string.Empty;
        Progress = 0;
        IsExporting = true;
        IsVisible = true;
        StatusText = $"准备导出 {totalCount} 张图片（并行 {maxConcurrency}）...";
    }

    /// <summary>
    /// 更新导出进度（线程安全）
    /// </summary>
    public void ReportProgress(int completed, int failed, int active)
    {
        CompletedCount = completed;
        FailedCount = failed;
        ActiveCount = active;
        int done = completed + failed;
        Progress = TotalCount > 0 ? (double)done / TotalCount : 0;

        if (active > 0)
            StatusText = $"导出中... {done}/{TotalCount}（并行 {active}）";
        else
            StatusText = $"导出中... {done}/{TotalCount}";
    }

    /// <summary>
    /// 导出全部完成
    /// </summary>
    public void Complete()
    {
        IsExporting = false;
        ActiveCount = 0;
        if (FailedCount > 0)
            StatusText = $"导出完成：{CompletedCount} 成功，{FailedCount} 失败";
        else
            StatusText = $"导出完成：共 {CompletedCount} 张图片";
    }

    /// <summary>
    /// 导出被取消
    /// </summary>
    public void Cancel()
    {
        IsExporting = false;
        ActiveCount = 0;
        StatusText = $"已取消（已完成 {CompletedCount + FailedCount}/{TotalCount}）";
    }

    [RelayCommand]
    public void CancelExport()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    public void Dismiss()
    {
        if (IsExporting)
            return; // 导出中不允许关闭
        IsVisible = false;
    }
}
