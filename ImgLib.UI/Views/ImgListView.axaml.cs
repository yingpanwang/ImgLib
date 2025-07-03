using Avalonia.Controls;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class ImgListView : UserControl
{
    public ImgListView()
    {
        InitializeComponent();

        DataContext = new ImgListViewModel(@"C:\Users\Administrator\Desktop\后期临时\2024-09-22西湖公园");

        //ImageService.Generate(@"C:\Users\Administrator\Desktop\后期临时\DSC_343120240714000111.JPG", @"C:\Users\Administrator\Desktop\test\a.jpg");
    }
}