using Avalonia;
using Avalonia.Controls;
using ImgLib.UI.ViewModels;
namespace ImgLib.UI;

public sealed partial class ImgFileDescView : UserControl
{

    public static StyledProperty<string> ImgFilePathProperty =
        AvaloniaProperty.Register<ImgFileDescView, string>(nameof(ImgFilePath));

    public string ImgFilePath
    {
        get => GetValue(ImgFilePathProperty);
        set => SetValue(ImgFilePathProperty, value);
    }

    public ImgFileDescView()
    {
        InitializeComponent();

        DataContext = new ImgFileDescViewModel();

        this.GetObservable<string>(ImgFilePathProperty)
            .Subscribe(path =>
            {
                DataContext = new ImgFileDescViewModel(path);
                // You can add additional logic here if needed when ImgFilePath changes
            });
    }

}