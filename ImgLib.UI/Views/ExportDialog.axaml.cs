using Avalonia.Controls;
using Avalonia.Interactivity;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI.Views;

public partial class ExportDialog : Window
{
    public ExportDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 用户点击"导出 (N)"按钮
    /// </summary>
    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ExportDialogViewModel vm)
        {
            vm.Confirm();
            Close(vm.SelectedPaths);
        }
    }

    /// <summary>
    /// 用户点击"取消"按钮
    /// </summary>
    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ExportDialogViewModel vm)
        {
            vm.Cancel();
        }
        Close(null);
    }
}
