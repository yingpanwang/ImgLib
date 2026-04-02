using Avalonia.Platform.Storage;

namespace ImgLib.UI.ViewModels.Design;

internal sealed partial class Deisgn_MainWindowViewModel : MainWindowViewModel
{
    public Deisgn_MainWindowViewModel(IStorageProvider storageProvider) : base(storageProvider)
    {
        ImgListBoxViewModel = new Design_ImgListBoxViewModel();
    }
}
