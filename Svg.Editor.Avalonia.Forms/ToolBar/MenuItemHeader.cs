using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;

namespace Svg.Editor.Avalon.Forms.ToolBar;

public class MenuItemHeader
{
    public MenuItemHeader(DrawingGroup icon)
    {
        Icon = new DrawingImage(icon);
    }
    public MenuItemHeader(string title, DrawingGroup icon)
    {
        Title = title;
        Icon = new DrawingImage(icon);
    }

    public string Title { get; set; }
    public DrawingImage Icon { get; set; }
}