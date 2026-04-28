using Avalonia;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Xaml.Interactivity;

namespace ImgLib.UI.Behaviors;

internal class ResetOnSourceChangedBehavior : Behavior<ZoomBorder>
{
    public static readonly StyledProperty<object?> SourceProperty =
                    AvaloniaProperty.Register<ResetOnSourceChangedBehavior, object?>(nameof(Source));

    /// <summary>
    /// Gets or sets the IsCapable property. This StyledProperty
    /// indicates ....
    /// </summary>
    public object? Source
    {
        get => this.GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        this.GetObservable(SourceProperty)
            .Subscribe(_ =>
        {
            if (AssociatedObject != null)
            {
                AssociatedObject?.ResetMatrix();
            }
        });
    }
}