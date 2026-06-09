using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImgLib.UI.ViewModels;

public partial class ImgListBoxViewModel : ViewModelBase
{
    private const int PageSize = 30;

    private CancellationTokenSource? _loadCts;
    private List<string>? _allPaths;
    private int _loadedCount;
    private bool _isLoadingPage;
    private int _loadGeneration; // 每次 Path 切换递增，防止旧页结果污染新列表

    [ObservableProperty]
    public partial string? Path { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ImgListItemViewModel>? ImgListItems { get; set; } = [];

    [ObservableProperty]
    public partial ImgListItemViewModel? SelectedImgItem { get; set; }

    /// <summary>当前目录下所有图片文件路径（供导出等批量操作使用）</summary>
    public IReadOnlyList<string>? AllFilePaths => _allPaths;

    /// <summary>列表中是否有图片</summary>
    public bool HasItems => _allPaths is { Count: > 0 };

    /// <summary>是否还有更多条目可加载</summary>
    public bool HasMoreItems => _allPaths is not null && _loadedCount < _allPaths.Count;

    /// <summary>列表标题，含已加载数量与总数</summary>
    public string HeaderText => _allPaths is { Count: > 0 } all
        ? $"图片列表 ({_loadedCount}/{all.Count})"
        : "图片列表";

    partial void OnPathChanged(string? oldValue, string? newValue)
    {
        // 取消上一次加载
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;

        // 重置分页状态
        _allPaths = null;
        _loadedCount = 0;
        _isLoadingPage = false;
        _loadGeneration++;

        ImgListItems?.Clear();
        OnPropertyChanged(nameof(HeaderText));
        OnPropertyChanged(nameof(HasMoreItems));

        if (string.IsNullOrEmpty(Path))
            return;

        _loadCts = new CancellationTokenSource();
        var token = _loadCts.Token;

        _ = LoadAsync(Path, token);
    }

    /// <summary>
    /// 扫描全部文件路径 → 加载首页。
    /// </summary>
    private async Task LoadAsync(string folderPath, CancellationToken ct)
    {
        try
        {
            // Phase 1: 后台扫描全部文件路径（轻量操作）
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".gif", ".nef", ".arw"
            };

            var allPaths = await Task.Run(() =>
                Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => extensions.Contains(System.IO.Path.GetExtension(f)))
                    .ToList(),
                ct);

            if (ct.IsCancellationRequested)
                return;

            _allPaths = allPaths;
            OnPropertyChanged(nameof(HeaderText));

            // Phase 2: 加载首页
            await LoadPageAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // 被取消，静默处理
        }
    }

    /// <summary>
    /// 加载下一页。由滚动检测触发，可多次调用（内部有防并发保护）。
    /// </summary>
    public async Task LoadNextPageAsync()
    {
        if (_isLoadingPage)
            return;

        if (!HasMoreItems)
            return;

        // 使用独立的 CancellationToken，不受 Path 切换的 _loadCts 控制
        _isLoadingPage = true;
        try
        {
            await LoadPageAsync(CancellationToken.None);
        }
        finally
        {
            _isLoadingPage = false;
        }
    }

    /// <summary>
    /// 从 _allPaths 取下一页路径，在后台创建 ViewModel，追加到列表。
    /// </summary>
    private async Task LoadPageAsync(CancellationToken ct)
    {
        if (_allPaths is null)
            return;

        var generation = _loadGeneration;
        var batch = _allPaths.Skip(_loadedCount).Take(PageSize).ToList();
        if (batch.Count == 0)
            return;

        var items = await Task.Run(
            () => batch.Select(ImgListItemViewModel.CreateFromPath).ToList(),
            ct);

        // 期间 Path 已切换，丢弃旧结果
        if (_loadGeneration != generation)
            return;

        if (ct.IsCancellationRequested)
            return;

        _loadedCount += items.Count;
        ImgListItems?.AddRange(items);
        OnPropertyChanged(nameof(HeaderText));
        OnPropertyChanged(nameof(HasMoreItems));
    }
}
