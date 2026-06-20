using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ImgLib.UI.Converters;

/// <summary>
/// 导航栏选中态背景转换器
/// </summary>
public static class NavConverters
{
    public static readonly IValueConverter BoolToSelectedBg = new FuncValueConverter<bool, IBrush>(
        isSelected => isSelected
            ? Brush.Parse("#E8ECFB")
            : Brushes.Transparent);
}
