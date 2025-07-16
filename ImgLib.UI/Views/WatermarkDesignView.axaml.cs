using Avalonia;
using Avalonia.Controls;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class WatermarkDesignView : UserControl
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

    public WatermarkDesignView()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == nameof(Path) && !string.IsNullOrEmpty(Path))
        {
            DataContext = new WatermarkDesignViewModel(Path);
        }
    }
}