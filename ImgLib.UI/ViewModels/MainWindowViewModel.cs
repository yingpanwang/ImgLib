using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using ImgLib.Models;
using ImgLib.UI.Services;
using ImgLib.UI.Views;
using System.Collections.Generic;
using System.Threading;

namespace ImgLib.UI.ViewModels;

public partial class MainWindowViewModel(IStorageProvider storageProvider) : ViewModelBase
{
    [ObservableProperty]
    public partial ImgListBoxViewModel ImgListBoxViewModel { get; set; } = new();

    private readonly ToastViewModel _toastViewModel = new();
    private readonly ExportProgressViewModel _exportProgressViewModel = new();
    private WatermarkDesignViewModel _watermarkDesignViewModel = null!;

    /// <summary>
    /// 主窗口引用，用于弹出对话框
    /// </summary>
    public Window? ParentWindow { get; set; }

    [ObservableProperty]
    public partial string CurrentRootFolder { get; set; }

    public ExportProgressViewModel ExportProgressViewModel => _exportProgressViewModel;

    public WatermarkDesignViewModel WatermarkDesignViewModel
    {
        get
        {
            if (_watermarkDesignViewModel == null)
            {
                _watermarkDesignViewModel = new();
                ToastService.Initialize(_toastViewModel); // 初始化全局 Toast 服务
            }
            return _watermarkDesignViewModel;
        }
    }

    public ToastViewModel ToastViewModel => _toastViewModel;

    [RelayCommand]
    public async Task OpenRootFolder()
    {
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
        });

        if (folders == null || folders.Count == 0)
            return;

        CurrentRootFolder = folders[0].Path.LocalPath;
    }

    partial void OnCurrentRootFolderChanged(string value)
    {
        ImgListBoxViewModel.Path = value;
    }

    /// <summary>
    /// 批量导出：弹出缩略图选择对话框 → 用户勾选 → 选择输出目录 → 并行导出
    /// </summary>
    [RelayCommand]
    public async Task ExportBatchAsync()
    {
        var allPaths = ImgListBoxViewModel.AllFilePaths;
        if (allPaths == null || allPaths.Count == 0)
        {
            ToastService.ShowWarning("没有可导出的图片，请先打开图片目录");
            return;
        }

        if (ParentWindow == null)
        {
            ToastService.ShowError("无法弹出导出对话框：主窗口未初始化");
            return;
        }

        // 1. 弹出缩略图选择对话框
        var dialogVm = new ExportDialogViewModel();
        dialogVm.Initialize(allPaths);

        var dialog = new ExportDialog { DataContext = dialogVm };
        var selectedPaths = await dialog.ShowDialog<IReadOnlyList<string>?>(ParentWindow);

        if (selectedPaths == null || selectedPaths.Count == 0)
            return; // 用户取消或未选择任何图片

        // 2. 选择输出目录
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "选择导出目录",
        });

        if (folders == null || folders.Count == 0)
            return;

        var outputDir = folders[0].Path.LocalPath;

        // 3. 获取当前水印设置
        var options = WatermarkDesignViewModel.WatermarkSettingsViewModel.Settings.ToImageGenerateOption();

        // 4. 并行导出
        var cts = new CancellationTokenSource();
        var maxConcurrency = Math.Max(1, Environment.ProcessorCount - 1);
        ExportProgressViewModel.BeginExport(selectedPaths.Count, cts, maxConcurrency);

        var completed = 0;
        var failed = 0;
        var active = 0;
        var token = cts.Token;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxConcurrency,
            CancellationToken = token
        };

        await Task.Run(async () =>
        {
            try
            {
                await Parallel.ForEachAsync(selectedPaths, parallelOptions, async (filePath, ct) =>
                {
                    Interlocked.Increment(ref active);

                    // 更新 UI 进度
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ExportProgressViewModel.ReportProgress(
                            Volatile.Read(ref completed),
                            Volatile.Read(ref failed),
                            Volatile.Read(ref active));
                    });

                    try
                    {
                        await ExportSingleImageAsync(filePath, outputDir, options, ct);
                        Interlocked.Increment(ref completed);
                    }
                    catch (OperationCanceledException)
                    {
                        throw; // 让 Parallel.ForEachAsync 处理取消
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref failed);
                        System.Diagnostics.Debug.WriteLine($"[Export] 导出失败: {filePath}, 错误: {ex.Message}");
                    }
                    finally
                    {
                        Interlocked.Decrement(ref active);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                // 用户取消，正常流程
            }

            // 最终进度更新
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ExportProgressViewModel.ReportProgress(
                    Volatile.Read(ref completed),
                    Volatile.Read(ref failed),
                    0);

                if (token.IsCancellationRequested)
                    ExportProgressViewModel.Cancel();
                else
                    ExportProgressViewModel.Complete();

                if (failed > 0)
                    ToastService.ShowWarning($"导出完成：{completed} 成功，{failed} 失败");
                else
                    ToastService.ShowSuccess($"导出完成：共 {completed} 张图片");
            });
        }, token);
    }

    /// <summary>
    /// 导出当前预览的单张图片
    /// </summary>
    [RelayCommand]
    public async Task ExportCurrentAsync()
    {
        var previewPath = WatermarkDesignViewModel.PreviewFilePath;
        if (string.IsNullOrWhiteSpace(previewPath))
        {
            ToastService.ShowWarning("请先在左侧列表中选择一张图片");
            return;
        }

        // 选择保存路径
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "导出水印图片",
            DefaultExtension = ".jpg",
            FileTypeChoices = [new FilePickerFileType("JPEG 图片") { Patterns = ["*.jpg", "*.jpeg"] }],
            SuggestedFileName = System.IO.Path.GetFileNameWithoutExtension(previewPath) + "_水印",
        });

        if (file == null)
            return;

        var outputPath = file.Path.LocalPath;
        var options = WatermarkDesignViewModel.WatermarkSettingsViewModel.Settings.ToImageGenerateOption();

        try
        {
            await Task.Run(() => ExportSingleImageAsync(previewPath, outputPath, options, CancellationToken.None, isDirectPath: true));
            ToastService.ShowSuccess("导出完成");
        }
        catch (Exception ex)
        {
            ToastService.ShowError($"导出失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出一张图片到指定目录（自动命名）或指定路径。
    /// 调用方应确保在后台线程执行。
    /// </summary>
    private static async Task ExportSingleImageAsync(
        string sourcePath,
        string outputDirOrPath,
        ImgLib.ImageGenerateOption options,
        CancellationToken ct,
        bool isDirectPath = false)
    {
        ct.ThrowIfCancellationRequested();

        var imageFile = ImageFile.GetImageFile(sourcePath);

        // 确保 EXIF 元数据已加载
        if (imageFile.Exif != null)
        {
            await imageFile.Exif.EnsureMetadataLoadedAsync();
        }

        ct.ThrowIfCancellationRequested();

        var exifInfo = imageFile.Exif is NikonExifInfo nef
            ? nef
            : imageFile.Exif;

        string outputPath;
        if (isDirectPath)
        {
            outputPath = outputDirOrPath;
        }
        else
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
            outputPath = System.IO.Path.Combine(outputDirOrPath, $"{fileName}_水印.jpg");
        }

        using var inputStream = imageFile.GetSourceStream();
        using var outputStream = System.IO.File.Create(outputPath);

        ImageService.GenerateWithOptions(
            inputStream,
            outputStream,
            options,
            exifInfo,
            isPreview: false);
    }

    [RelayCommand]
    public void TestToast()
    {
        _toastViewModel.ShowMessage("测试通知", ToastType.Info);
    }
}
