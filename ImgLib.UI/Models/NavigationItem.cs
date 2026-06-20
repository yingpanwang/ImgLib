namespace ImgLib.UI.Models;

/// <summary>
/// 左侧导航栏条目模型
/// </summary>
public partial class NavigationItem : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    /// <summary>导航键（唯一标识）</summary>
    public string Key { get; }

    /// <summary>显示名称</summary>
    public string Name { get; }

    /// <summary>图标字符（如 "🎨"）</summary>
    public string Icon { get; }

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isSelected;

    public NavigationItem(string key, string name, string icon)
    {
        Key = key;
        Name = name;
        Icon = icon;
    }
}
