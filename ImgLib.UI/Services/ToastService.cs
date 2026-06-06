using ImgLib.UI.ViewModels;

namespace ImgLib.UI.Services;

/// <summary>
/// 全局 Toast 服务，提供静态方法调用
/// </summary>
public static class ToastService
{
    private static ToastViewModel? _toastViewModel;

    /// <summary>
    /// 初始化 ToastService，绑定到实际的 ToastViewModel
    /// </summary>
    public static void Initialize(ToastViewModel toastViewModel)
    {
        _toastViewModel = toastViewModel;
    }

    /// <summary>
    /// 显示信息通知
    /// </summary>
    public static void ShowInfo(string message, int durationMs = 3000)
        => _toastViewModel?.ShowMessage(message, ToastType.Info, durationMs);

    /// <summary>
    /// 显示成功通知
    /// </summary>
    public static void ShowSuccess(string message, int durationMs = 3000)
        => _toastViewModel?.ShowMessage(message, ToastType.Success, durationMs);

    /// <summary>
    /// 显示警告通知
    /// </summary>
    public static void ShowWarning(string message, int durationMs = 3000)
        => _toastViewModel?.ShowMessage(message, ToastType.Warning, durationMs);

    /// <summary>
    /// 显示错误通知
    /// </summary>
    public static void ShowError(string message, int durationMs = 3000)
        => _toastViewModel?.ShowMessage(message, ToastType.Error, durationMs);
}