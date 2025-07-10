using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Threading;

namespace ImgLib.UI;

public partial class DebugeView : UserControl
{
    /// <summary>
    /// IsCapable StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<string> DebugTextProperty =
        AvaloniaProperty.Register<DebugeView, string>(nameof(DebugText));

    /// <summary>
    /// Gets or sets the IsCapable property. This StyledProperty
    /// indicates ....
    /// </summary>
    public string DebugText
    {
        get => this.GetValue(DebugTextProperty);
        set => SetValue(DebugTextProperty, value);
    }


    public DebugeView()
    {
        InitializeComponent();

        this.GetObservable(DebugTextProperty).Subscribe(text =>
        {
            string x = text;
            dtb.Text = x;
        });

        Task.Run(() =>
        {
            while (true)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    DebugText = $"Debug Text: {DateTime.Now:HH:mm:ss.fff}";

                });

                Thread.Sleep(1000);
            }
        });
    }
}