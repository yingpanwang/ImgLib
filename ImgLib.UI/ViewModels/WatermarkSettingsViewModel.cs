namespace ImgLib.UI.ViewModels;

public partial class WatermarkSettingsViewModel(ImageGenerateOption? option = null) : ViewModelBase
{

    [ObservableProperty]
    public partial ImageGenerateOption ImageGenerateOption { get; private set; } =
        option ??= new ImageGenerateOption(0.89f);

}
