// using DynamicData;

namespace ImgLib.UI.ViewModels.Design;

public sealed partial class Design_ImgListBoxViewModel : ImgListBoxViewModel
{
    public Design_ImgListBoxViewModel() : this(new WatermarkDesignViewModel(
        new WatermarkSettingsViewModel(),
        new HistogramViewModel(),
        new PreviewSettingsViewModel(),
        new WatermarkSettingListViewModel()))
    {
    }

    public Design_ImgListBoxViewModel(WatermarkDesignViewModel wdvm) : base(wdvm)
    {
        this.Path = @"C:\Users\Administrator\Desktop\后期临时";

        if (ImgListItems is not null)
        {
            foreach (var item in ImgListItemViewModel.Create(Path))
            {
                ImgListItems.Add(item);
            }
        }
    }
}