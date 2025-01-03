using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;

namespace Svg.Editor.Avalon.Forms.ToolBar;

public class MenuItemHeaderTemplate : IDataTemplate
{
    public Control? Build(object? param)
    {
        var size = FormsToolBarIconSizeProvider.GetSize();
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                new Avalonia.Controls.Image()
                {
                    Width = size.Width,
                    Height = size.Height,
                    [!Avalonia.Controls.Image.SourceProperty] = new Binding("Icon")
                },
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding("Title"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    public bool Match(object data)
    {
        return data is MenuItemHeader;
    }
}