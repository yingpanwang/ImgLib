using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class ImgListBoxView : UserControl
{
    public ImgListBoxView()
    {
        InitializeComponent();

        // 模板应用后查找内部 ScrollViewer 并直接订阅 CLR 事件（最可靠的方式）
        l.TemplateApplied += (_, _) =>
        {
            var sv = l.FindDescendantOfType<ScrollViewer>();
            if (sv is not null)
            {
                sv.ScrollChanged -= OnScrollViewerScrollChanged;
                sv.ScrollChanged += OnScrollViewerScrollChanged;
            }
        };
    }

    /// <summary>
    /// XAML 附加事件 ScrollViewer.ScrollChanged 的回退处理。
    /// </summary>
    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (e.Source is ScrollViewer sv)
            CheckScroll(sv);
    }

    /// <summary>
    /// 直接订阅 ScrollViewer.ScrollChanged CLR 事件。
    /// </summary>
    private void OnScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer sv)
            CheckScroll(sv);
    }

    private void CheckScroll(ScrollViewer sv)
    {
        // 距底部 200px 时触发下一页
        if (sv.Offset.Y + sv.Viewport.Height >= sv.Extent.Height - 200)
        {
            if (DataContext is ImgListBoxViewModel vm)
            {
                _ = vm.LoadNextPageAsync();
            }
        }
    }
}
