using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Avalon.Forms.ToolBar;

public class MenuItemHeaderTemplate : IDataTemplate
{
    private StackPanel _panel;

    public Control? Build(object? param)
    { 
        var sizeProvider = SvgEngine.TryResolve<IToolbarIconSizeProvider>();
        var size = sizeProvider != null ? sizeProvider.GetSize() : FormsToolBarIconSizeProvider.GetSize() ;

        _panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                new Avalonia.Controls.Image()
                {
                    Width = size.Width,
                    Height = size.Height,

                    [!Avalonia.Controls.Image.SourceProperty] = new Binding("Icon"),
                },
                new TextBlock
                {
                    [!TextBlock.TextProperty] = new Binding("Title"),
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };

        return _panel;
    }


    public bool Match(object data)
    {
        return data is MenuItemHeader;
    }

    
}