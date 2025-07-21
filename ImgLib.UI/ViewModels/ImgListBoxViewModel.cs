using DynamicData;
using System.Collections.ObjectModel;

namespace ImgLib.UI.ViewModels;

public partial class ImgListBoxViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string? Path { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ImgListItemViewModel>? ImgListItems { get; set; } = [];

    [ObservableProperty]
    public partial ImgListItemViewModel? SelectedImgItem { get; set; }

    partial void OnPathChanged(string? oldValue, string? newValue)
    {
        if (!string.IsNullOrEmpty(Path))
        {
            ImgListItems?.Clear();

            ImgListItems?.AddRange(ImgListItemViewModel.Create(Path));
        }
    }
}
