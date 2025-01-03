using Avalonia.Media;

namespace Svg.Editor.Avalon.Forms.ToolBar;

public class MenuItemHeader
{
    public MenuItemHeader(string title, StreamGeometry icon)
    {
        Title = title;
        Icon = icon;
    }

    public string Title { get; set; }
    public StreamGeometry Icon { get; set; }
}