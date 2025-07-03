using Avalonia.Controls;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class WatermarkDesignView : UserControl
{
    public WatermarkDesignView()
    {
        InitializeComponent();
        DataContext = new WatermarkDesignViewModel();
    }
}