using Avalonia;
using Avalonia.Controls;
using ImgLib.UI.ViewModels;
using System.Collections.ObjectModel;

namespace ImgLib.UI;

public partial class ImgListBoxView : UserControl
{
    /// <summary>
    /// Path StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<string> PathProperty =
        AvaloniaProperty.Register<ImgListBoxView, string>(
            nameof(Path),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
            );

    /// <summary>
    /// SelectedItem StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<ImgListItemViewModel?> SelectedImgItemProperty =
        AvaloniaProperty.Register<ImgListBoxView, ImgListItemViewModel?>(nameof(SelectedImgItem),
            defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
            );

    /// <summary>
    /// Gets or sets the SelectedItem property. This StyledProperty
    /// indicates ....
    /// </summary>
    public ImgListItemViewModel? SelectedImgItem
    {
        get => this.GetValue(SelectedImgItemProperty);
        set => SetValue(SelectedImgItemProperty, value);
    }

    /// <summary>
    /// Gets or sets the Path property. This StyledProperty
    /// indicates ....
    /// </summary>
    public string Path
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    public ObservableCollection<ImgListItemViewModel>? ImgListItems { get; set; }

    public ImgListBoxView()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == nameof(Path) && !string.IsNullOrEmpty(Path))
        {
            ImgListItems = new ObservableCollection<ImgListItemViewModel>(
                ImgListItemViewModel.Create(Path));
            this.DataContext = this;
        }
        else if (change.Property.Name == nameof(SelectedImgItem))
        {
            int i = 1;
        }
    }
}