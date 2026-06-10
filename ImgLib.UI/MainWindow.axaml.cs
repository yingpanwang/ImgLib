using Avalonia.Controls;
using ImgLib.UI.Services;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm;
    public MainWindow()
    {
        InitializeComponent();

        _vm = new MainWindowViewModel(GetTopLevel(this)!.StorageProvider)
        {
            ParentWindow = this
        };

        DataContext = _vm;

        _vm.ImgListBoxViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_vm.ImgListBoxViewModel.SelectedImgItem))
            {
                if (_vm.ImgListBoxViewModel.SelectedImgItem != null)
                {
                    _vm.WatermarkDesignViewModel.Reset();
                    _vm.WatermarkDesignViewModel.PreviewFilePath = _vm.ImgListBoxViewModel.SelectedImgItem.FilePath;
                }
            }
        };
    }
}