namespace ImgLib.UI.ViewModels;

/// <summary>
/// 水印设置文件列表 ViewModel。
/// 管理已保存的水印设置文件条目，支持加载、移除、刷新操作。
/// 文件统一存储在系统 AppData 目录下的 WatermarkPresets 文件夹中。
/// </summary>
public partial class WatermarkSettingListViewModel : ViewModelBase
{
    /// <summary>水印预设文件存储目录</summary>
    public static readonly string PresetsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ImgLib",
        "WatermarkPresets");

    /// <summary>已保存的水印设置文件列表</summary>
    [ObservableProperty]
    public partial ObservableCollection<WatermarkSettingFileEntry> SavedFiles { get; set; } = [];

    /// <summary>是否有已保存的文件（用于空状态 UI 显示）</summary>
    public bool HasFiles => SavedFiles.Count > 0;

    public WatermarkSettingListViewModel()
    {
        // 启动时扫描预设目录
        ScanPresetsDirectory();

        // 注册消息：水印设置保存成功 → 更新文件列表
        WeakReferenceMessenger.Default.Register<SettingsFileSavedMessage>(this, (r, m) =>
        {
            AddSavedFile(m.FilePath);
        });
    }

    /// <summary>
    /// 扫描预设目录，刷新文件列表。
    /// </summary>
    public void ScanPresetsDirectory()
    {
        SavedFiles.Clear();

        if (!Directory.Exists(PresetsDirectory))
            return;

        try
        {
            var jsonFiles = Directory.GetFiles(PresetsDirectory, "*.json")
                .OrderByDescending(f => new FileInfo(f).LastWriteTime);

            foreach (var filePath in jsonFiles)
            {
                SavedFiles.Add(new WatermarkSettingFileEntry
                {
                    FilePath = filePath,
                    SavedAt = new FileInfo(filePath).LastWriteTime
                });
            }
        }
        catch
        {
            // 目录访问失败则忽略
        }

        OnPropertyChanged(nameof(HasFiles));
    }

    /// <summary>手动刷新预设列表</summary>
    [RelayCommand]
    private void RefreshList()
    {
        ScanPresetsDirectory();
        ToastService.ShowSuccess("已刷新水印预设列表");
    }

    /// <summary>
    /// 添加已保存的文件到列表。按路径去重，重复路径仅更新 <see cref="WatermarkSettingFileEntry.SavedAt"/>。
    /// </summary>
    public void AddSavedFile(string filePath)
    {
        var existing = SavedFiles.FirstOrDefault(f =>
            string.Equals(f.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            // 重复路径：更新时间戳
            existing.SavedAt = DateTime.Now;
        }
        else
        {
            SavedFiles.Insert(0, new WatermarkSettingFileEntry
            {
                FilePath = filePath,
                SavedAt = DateTime.Now
            });
        }

        OnPropertyChanged(nameof(HasFiles));
    }

    /// <summary>加载选中的水印设置文件</summary>
    [RelayCommand]
    private async Task LoadWatermarkSetting(WatermarkSettingFileEntry? entry)
    {
        if (entry == null)
            return;

        if (!File.Exists(entry.FilePath))
        {
            ToastService.ShowError("文件不存在，可能已被移动或删除");
            // 刷新列表以移除无效条目
            ScanPresetsDirectory();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(entry.FilePath);
            var settings = JsonSerializer.Deserialize(json, ImgLibUIJsonContext.Default.WatermarkSettings);

            if (settings != null)
            {
                WeakReferenceMessenger.Default.Send(new LoadWatermarkSettingsMessage(settings));
            }
            else
            {
                ToastService.ShowError("水印设置文件格式无效");
            }
        }
        catch (JsonException)
        {
            ToastService.ShowError("水印设置文件格式无效，无法解析 JSON");
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"加载水印设置失败: {ex.Message}");
        }
    }

    /// <summary>从列表中移除条目并删除磁盘文件</summary>
    [RelayCommand]
    private void RemoveEntry(WatermarkSettingFileEntry? entry)
    {
        if (entry == null)
            return;

        try
        {
            if (File.Exists(entry.FilePath))
                File.Delete(entry.FilePath);
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"删除文件失败: {ex.Message}");
            return;
        }

        SavedFiles.Remove(entry);
        OnPropertyChanged(nameof(HasFiles));
    }
}
