using Avalonia;
using Avalonia.Controls;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class ImgListView : UserControl
{

    /// <summary>
    /// Path StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<string> PathProperty =
        AvaloniaProperty.Register<ImgListView, string>(
            nameof(Path),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
            );

    /// <summary>
    /// Gets or sets the Path property. This StyledProperty
    /// indicates ....
    /// </summary>
    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == PathProperty)
        {
            DataContext = new ImgListViewModel(Path);
        }
    }

    public ImgListView()
    {
        InitializeComponent();

        DataContext = new ImgListViewModel(@"C:\Users\Administrator\Desktop\后期临时\2024-09-22西湖公园");

        //ImageService.Generate(@"C:\Users\Administrator\Desktop\后期临时\DSC_343120240714000111.JPG", @"C:\Users\Administrator\Desktop\test\a.jpg");
    }
}