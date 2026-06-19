using System.Windows.Input;

namespace ImgLib.UI.ViewModels;

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}

public sealed partial class ToastViewModel : ViewModelBase
{
    public ObservableCollection<ToastMessage> Messages { get; } = new();

    public ICommand CloseCommand { get; }

    public ToastViewModel()
    {
        CloseCommand = new RelayCommand<ToastMessage>(CloseMessage);
    }

    public void ShowMessage(string message, ToastType type = ToastType.Info, int durationMs = 3000)
    {
        var toastMessage = new ToastMessage
        {
            Message = message,
            Type = type,
            Id = Guid.NewGuid()
        };

        Messages.Add(toastMessage);

        // 自动关闭
        if (durationMs > 0)
        {
            Task.Run(async () =>
            {
                await Task.Delay(durationMs);
                CloseMessage(toastMessage);
            });
        }
    }

    private void CloseMessage(ToastMessage? message)
    {
        if (message != null && Messages.Contains(message))
        {
            message.IsClosing = true;
            // 等待关闭动画完成后再移除
            Task.Delay(300).ContinueWith(_ =>
            {
                Messages.Remove(message);
            });
        }
    }
}

public sealed partial class ToastMessage : ObservableObject
{
    public Guid Id { get; init; }

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ToastType Type { get; set; } = ToastType.Info;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Opacity))]
    public partial bool IsClosing { get; set; }

    public double Opacity => IsClosing ? 0 : 1;

    public string Icon => Type switch
    {
        ToastType.Success => "✓",
        ToastType.Warning => "⚠",
        ToastType.Error => "✕",
        _ => "ℹ"
    };

    public string TypeColor => Type switch
    {
        ToastType.Success => "#10B981",
        ToastType.Warning => "#F59E0B",
        ToastType.Error => "#EF4444",
        _ => "#3B82F6"
    };
}