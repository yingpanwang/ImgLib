using System.Collections.ObjectModel;

namespace ImgLib.UI.ViewModels;

public sealed partial class ImgListViewModel(string? folder = null) : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<ImgListItemViewModel> ImgListItems { get; set; } =
        string.IsNullOrEmpty(folder)
        ? []
        : new ObservableCollection<ImgListItemViewModel>(
            ImgListItemViewModel.Create(folder)
        );
}
