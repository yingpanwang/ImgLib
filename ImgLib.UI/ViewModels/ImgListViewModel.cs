using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ImgLib.UI.ViewModels;

public sealed partial class ImgListViewModel(string? folder = null) : ViewModelBase
{
    [ObservableProperty]
    public partial string Path { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ImgListItemViewModel> ImgListItems { get; set; } =
        string.IsNullOrEmpty(folder)
        ? []
        : new ObservableCollection<ImgListItemViewModel>(
            ImgListItemViewModel.Create(folder)
        );

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(Path))
        {
            this.ImgListItems = [.. ImgListItemViewModel.Create(e.PropertyName)];
        }
    }
}
