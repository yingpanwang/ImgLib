using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ImgLib.UI.Views;

public partial class WatermarkPresetsWindow : Window
{
    public WatermarkPresetsWindow()
    {
        InitializeComponent();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
