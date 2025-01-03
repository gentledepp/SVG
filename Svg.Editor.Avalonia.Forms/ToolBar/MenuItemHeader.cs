namespace Svg.Editor.Avalon.Forms.ToolBar;

public class MenuItemHeader
{
    public MenuItemHeader(string title, Avalonia.Media.Imaging.Bitmap icon)
    {
        Title = title;
        Icon = icon;
    }

    public string Title { get; set; }
    public Avalonia.Media.Imaging.Bitmap Icon { get; set; }
}