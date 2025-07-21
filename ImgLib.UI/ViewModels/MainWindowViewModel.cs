using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;

namespace ImgLib.UI.ViewModels;

public sealed partial class MainWindowViewModel(IStorageProvider storageProvider) : ViewModelBase
{
    [ObservableProperty]
    public partial ImgListBoxViewModel ImgListBoxViewModel { get; set; } = new();

    [ObservableProperty]
    public partial WatermarkDesignViewModel WatermarkDesignViewModel { get; set; } = new();

    [ObservableProperty]
    public partial string CurrentRootFolder { get; set; }

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
}
