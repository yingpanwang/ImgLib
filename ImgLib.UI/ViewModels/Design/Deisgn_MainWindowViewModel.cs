using Avalonia.Platform.Storage;

namespace ImgLib.UI.ViewModels.Design;

public sealed partial class Deisgn_MainWindowViewModel : MainWindowViewModel
{
    public Deisgn_MainWindowViewModel(IStorageProvider storageProvider, WatermarkDesignViewModel wdvm)
        : base(storageProvider, null!, null!, null!, null!, null!, null!, null!)
    {
        ImgListBoxViewModel = new Design_ImgListBoxViewModel(wdvm) { Path = @"C:\Users\Administrator\Desktop\后期临时" };
    }
}