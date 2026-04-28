using DynamicData;

namespace ImgLib.UI.ViewModels.Design;

public sealed partial class Design_ImgListBoxViewModel : ImgListBoxViewModel
{
    public Design_ImgListBoxViewModel()
    {
        this.Path = @"C:\Users\Administrator\Desktop\后期临时";

        ImgListItems?.AddRange(ImgListItemViewModel.Create(Path));
    }
}