using Avalonia.Controls;
using Avalonia.Input;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class WatermarkDesignView : UserControl
{
    private bool _isDragging;
    private double _dragStartX;
    private double _dragStartY;
    private double _initialTranslateX;
    private double _initialTranslateY;
    private bool _initialPositionSet;
    private IPointer? _capturedPointer;

    public WatermarkDesignView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        PreviewGrid.LayoutUpdated += OnPreviewGridLayoutUpdated;
    }

    protected override void OnDetachedFromVisualTree(global::Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        PreviewGrid.LayoutUpdated -= OnPreviewGridLayoutUpdated;
    }

    private void OnPreviewGridLayoutUpdated(object? sender, System.EventArgs e)
    {
        if (_initialPositionSet)
        {
            // 初始位置已设置，取消订阅以节省开销
            PreviewGrid.LayoutUpdated -= OnPreviewGridLayoutUpdated;
            return;
        }

        if (DataContext is not WatermarkDesignViewModel vm)
            return;

        var grid = PreviewGrid;
        if (grid.Bounds.Width <= 0 || grid.Bounds.Height <= 0)
            return;

        // 直方图尺寸 200x140，保留 12px 边距
        const double histogramWidth = 200;
        const double histogramHeight = 140;
        const double margin = 12;

        vm.HistogramTranslateX = grid.Bounds.Width - histogramWidth - margin * 2;
        vm.HistogramTranslateY = grid.Bounds.Height - histogramHeight - margin * 2;
        _initialPositionSet = true;

        PreviewGrid.LayoutUpdated -= OnPreviewGridLayoutUpdated;
    }

    /// <summary>
    /// 将直方图重置到右下角默认位置
    /// </summary>
    public void ResetHistogramPosition()
    {
        _initialPositionSet = false;
        if (DataContext is WatermarkDesignViewModel vm && PreviewGrid.Bounds.Width > 0)
        {
            const double histogramWidth = 200;
            const double histogramHeight = 140;
            const double margin = 12;

            vm.HistogramTranslateX = PreviewGrid.Bounds.Width - histogramWidth - margin * 2;
            vm.HistogramTranslateY = PreviewGrid.Bounds.Height - histogramHeight - margin * 2;
            _initialPositionSet = true;
        }
    }

    public void OnHistogramPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is not WatermarkDesignViewModel vm)
            return;

        var point = e.GetPosition(PreviewGrid);
        _dragStartX = point.X;
        _dragStartY = point.Y;
        _initialTranslateX = vm.HistogramTranslateX;
        _initialTranslateY = vm.HistogramTranslateY;
        _isDragging = true;
        vm.IsDraggingHistogram = true;

        _capturedPointer = e.Pointer;
        _capturedPointer.Capture(HistogramOverlay);
        e.Handled = true;
    }

    public void OnHistogramPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || DataContext is not WatermarkDesignViewModel vm)
            return;

        var point = e.GetPosition(PreviewGrid);
        var deltaX = point.X - _dragStartX;
        var deltaY = point.Y - _dragStartY;

        vm.HistogramTranslateX = _initialTranslateX + deltaX;
        vm.HistogramTranslateY = _initialTranslateY + deltaY;
    }

    public void OnHistogramPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        EndDrag();
    }

    public void OnHistogramPointerReleased(object? sender, PointerCaptureLostEventArgs e)
    {
        EndDrag();
    }

    private void EndDrag()
    {
        if (!_isDragging)
            return;

        _isDragging = false;

        if (DataContext is WatermarkDesignViewModel vm)
        {
            vm.IsDraggingHistogram = false;
        }

        _capturedPointer?.Capture(null);
        _capturedPointer = null;
    }
}
