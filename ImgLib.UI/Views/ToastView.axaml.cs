using Avalonia.Controls;
using Avalonia.Interactivity;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI.Views;

public partial class ToastView : UserControl
{
    public ToastView()
    {
        InitializeComponent();
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is ToastMessage message &&
            DataContext is ToastViewModel viewModel)
        {
            viewModel.CloseCommand.Execute(message);
        }
    }
}