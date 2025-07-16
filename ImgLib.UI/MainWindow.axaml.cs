using Avalonia.Controls;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
        ilv.Path = @"C:\Users\Administrator\Desktop\后期临时\2024-09-22西湖公园";
        ilv.PropertyChanged += (sender, args) =>
        {
            if (args.Property.Name == nameof(ilv.SelectedImgItem))
            {
                if (!string.IsNullOrEmpty(ilv.SelectedImgItem?.FilePath))
                {
                    wdv.DataContext = new WatermarkDesignViewModel(ilv.SelectedImgItem.FilePath);
                }
            }
        };
    }
}