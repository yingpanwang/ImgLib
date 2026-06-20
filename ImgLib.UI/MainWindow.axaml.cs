using Avalonia.Controls;
using ImgLib.UI.ViewModels;

namespace ImgLib.UI;

public partial class MainWindow : Window
{
    private ColumnDefinition? _imageListCol;
    private ColumnDefinition? _splitterCol;
    private Border? _imageListBorder;
    private GridSplitter? _contentSplitter;

    public MainWindow()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel vm)
            {
                // 缓存控件引用（OnInitialized 中还未就绪）
                _imageListBorder = this.FindControl<Border>("ImageListBorder");
                _contentSplitter = this.FindControl<GridSplitter>("ContentSplitter");

                var contentGrid = this.FindControl<Grid>("ContentGrid");
                if (contentGrid?.ColumnDefinitions.Count >= 4)
                {
                    _imageListCol = contentGrid.ColumnDefinitions[1];
                    _splitterCol = contentGrid.ColumnDefinitions[2];
                }

                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(vm.IsImageListVisible))
                        UpdateColumnWidths(vm.IsImageListVisible);
                };

                // 初始同步
                UpdateColumnWidths(vm.IsImageListVisible);
            }
        };
    }

    private void UpdateColumnWidths(bool imageListVisible)
    {
        if (_imageListBorder is not null)
            _imageListBorder.IsVisible = imageListVisible;
        if (_contentSplitter is not null)
            _contentSplitter.IsVisible = imageListVisible;

        if (_imageListCol is not null)
            _imageListCol.Width = imageListVisible
                ? new GridLength(2, GridUnitType.Star)
                : new GridLength(0);
        if (_splitterCol is not null)
            _splitterCol.Width = imageListVisible
                ? new GridLength(1)
                : new GridLength(0);
    }
}