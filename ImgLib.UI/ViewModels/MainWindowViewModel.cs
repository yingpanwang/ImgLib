namespace ImgLib.UI.ViewModels;

public sealed partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ImgListItemViewModel? ImgListBoxSelectedItem { get; set; }
}
