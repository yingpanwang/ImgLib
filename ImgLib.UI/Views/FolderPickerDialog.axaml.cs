using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI.Views;

public partial class FolderPickerDialog : Window
{
    public FolderPickerDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 用户点击"打开"按钮
    /// </summary>
    private void OnConfirmClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is OpenFolderDialogViewModel vm)
        {
            vm.Confirm();

            if (vm.Confirmed)
            {
                Close(vm.FolderPath);
            }
        }
    }

    /// <summary>
    /// 用户点击"取消"按钮
    /// </summary>
    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is OpenFolderDialogViewModel vm)
        {
            vm.Cancel();
        }
        Close(null);
    }

    /// <summary>
    /// TextBox 获得焦点 → 展开完整路径，方便用户编辑
    /// </summary>
    private void OnTextBoxGotFocus(object? sender, GotFocusEventArgs e)
    {
        if (DataContext is OpenFolderDialogViewModel vm)
        {
            vm.ExpandForEditing();
        }
    }

    /// <summary>
    /// TextBox 失去焦点 → 缩略显示
    /// </summary>
    private void OnTextBoxLostFocus(object? sender, RoutedEventArgs e)
    {
        if (DataContext is OpenFolderDialogViewModel vm)
        {
            vm.TruncateForDisplay();
        }
    }
}
