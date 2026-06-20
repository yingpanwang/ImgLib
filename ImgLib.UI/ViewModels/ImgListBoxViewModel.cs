// using DynamicData;

using ImgLib.Services;

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

    // ═══════════════════════════════════════════
    // 排序
    // ═══════════════════════════════════════════

    /// <summary>排序模式</summary>
    [ObservableProperty]
    private SortMode _currentSortMode = SortMode.FileName;

    /// <summary>是否升序</summary>
    [ObservableProperty]
    private bool _sortAscending = true;

    /// <summary>排序模式字符串列表（供 ComboBox 绑定）</summary>
    public static IReadOnlyList<SortMode> SortModes { get; } =
    [
        SortMode.FileName,
        SortMode.FileDate,
        SortMode.FileSize,
        SortMode.FileType
    ];

    /// <summary>排序方向箭头字符</summary>
    public string SortDirectionArrow => SortAscending ? "↑" : "↓";

    /// <summary>切换升序/降序</summary>
    [RelayCommand]
    private void ToggleSortDirection()
    {
        SortAscending = !SortAscending;
        OnPropertyChanged(nameof(SortDirectionArrow));
    }

    /// <summary>删除当前选中的图片文件</summary>
    [RelayCommand]
    private async Task DeleteSelectedImage()
    {
        if (SelectedImgItem?.FilePath is not { } path)
            return;

        if (!File.Exists(path))
            return;

        try
        {
            // 先在后台删除文件
            await Task.Run(() =>
            {
                ThumbnailCacheService.Remove(path);
                File.Delete(path);
            });

            // 从内存列表中移除
            _allPaths?.Remove(path);
            ImgListItems?.Remove(SelectedImgItem);

            // 清除预览
            SelectedImgItem = null;
            _watermarkDesignViewModel.Reset();

            OnPropertyChanged(nameof(HeaderText));
            OnPropertyChanged(nameof(HasMoreItems));

            ToastService.ShowSuccess("图片已删除");
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"删除失败: {ex.Message}");
        }
    }

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

    WatermarkDesignViewModel _watermarkDesignViewModel;
    public ImgListBoxViewModel(WatermarkDesignViewModel watermarkDesignViewModel)
    {
        _watermarkDesignViewModel = watermarkDesignViewModel;
    }

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

            _allPaths = SortPaths(allPaths).ToList();
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
        // ImgListItems?.AddRange(items);
        if (ImgListItems is not null)
        {
            foreach (var item in items)
                ImgListItems?.Add(item);
        }
        OnPropertyChanged(nameof(HeaderText));
        OnPropertyChanged(nameof(HasMoreItems));
    }

    partial void OnSelectedImgItemChanged(ImgListItemViewModel? value)
    {
        if (value?.FilePath is not null)
        {
            _watermarkDesignViewModel.Reset();
            _watermarkDesignViewModel.PreviewFilePath = value.FilePath;
        }
    }

    // ═══════════════════════════════════════════
    // 排序变更 → 重新排序并重载首页
    // ═══════════════════════════════════════════

    partial void OnCurrentSortModeChanged(SortMode value) => ApplySortAndReload();
    partial void OnSortAscendingChanged(bool value) => ApplySortAndReload();

    private void ApplySortAndReload()
    {
        if (_allPaths is not { Count: > 0 })
            return;

        _allPaths = SortPaths(_allPaths).ToList();

        // 重置分页并重载首页
        _loadedCount = 0;
        _loadGeneration++;
        ImgListItems?.Clear();
        OnPropertyChanged(nameof(HeaderText));
        OnPropertyChanged(nameof(HasMoreItems));

        _ = LoadPageAsync(CancellationToken.None);
    }

    private IEnumerable<string> SortPaths(IEnumerable<string> paths)
    {
        var comparer = GetComparer();
        return SortAscending
            ? paths.OrderBy(p => p, comparer)
            : paths.OrderByDescending(p => p, comparer);
    }

    private IComparer<string> GetComparer() => CurrentSortMode switch
    {
        SortMode.FileName => Comparer<string>.Create((a, b) =>
            string.Compare(
                System.IO.Path.GetFileNameWithoutExtension(a),
                System.IO.Path.GetFileNameWithoutExtension(b),
                StringComparison.CurrentCultureIgnoreCase)),

        SortMode.FileDate => Comparer<string>.Create((a, b) =>
        {
            var da = SafeGetLastWriteTime(a);
            var db = SafeGetLastWriteTime(b);
            return da.CompareTo(db);
        }),

        SortMode.FileSize => Comparer<string>.Create((a, b) =>
        {
            var sa = SafeGetFileSize(a);
            var sb = SafeGetFileSize(b);
            return sa.CompareTo(sb);
        }),

        SortMode.FileType => Comparer<string>.Create((a, b) =>
        {
            var extCmp = string.Compare(
                System.IO.Path.GetExtension(a),
                System.IO.Path.GetExtension(b),
                StringComparison.CurrentCultureIgnoreCase);
            if (extCmp != 0) return extCmp;
            return string.Compare(
                System.IO.Path.GetFileNameWithoutExtension(a),
                System.IO.Path.GetFileNameWithoutExtension(b),
                StringComparison.CurrentCultureIgnoreCase);
        }),

        _ => Comparer<string>.Default
    };

    private static DateTime SafeGetLastWriteTime(string path)
    {
        try { return File.GetLastWriteTime(path); }
        catch { return DateTime.MinValue; }
    }

    private static long SafeGetFileSize(string path)
    {
        try { return new FileInfo(path).Length; }
        catch { return 0; }
    }
}
