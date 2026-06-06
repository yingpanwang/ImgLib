using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using ImgLib.UI.Services;

namespace ImgLib.UI.ViewModels;

public partial class MainWindowViewModel(IStorageProvider storageProvider) : ViewModelBase
{
    [ObservableProperty]
    public partial ImgListBoxViewModel ImgListBoxViewModel { get; set; } = new();

    private readonly ToastViewModel _toastViewModel = new();
    private WatermarkDesignViewModel _watermarkDesignViewModel = null!;

    [ObservableProperty]
    public partial string CurrentRootFolder { get; set; }

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

    [RelayCommand]
    public void TestToast()
    {
        _toastViewModel.ShowMessage("测试通知", ToastType.Info);
    }
}