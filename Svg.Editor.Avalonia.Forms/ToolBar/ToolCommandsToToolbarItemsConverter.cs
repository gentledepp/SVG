using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using System;
using Avalonia.Media;
using Path = Avalonia.Controls.Shapes.Path;

namespace Svg.Editor.Avalon.Forms.ToolBar;

public class ToolCommandsToToolbarItemsConverter : IValueConverter
{
    private Lazy<IImageSourceProvider> _imageSourceProvider = new(SvgEngine.TryResolve<IImageSourceProvider>);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        int shownActions;
        if (parameter is int intParam)
            shownActions = intParam;
        else
            shownActions = 3;

        var commandLists = value as IEnumerable<IEnumerable<IToolCommand>>;

        var menuItems = new List<MenuItem>();

        foreach (var commands in commandLists)
        {
            var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
            if (cmds.Length == 0)
                continue;

            // single command => create a menu item
            if (cmds.Length == 1)
            {
                var command = cmds.Single();

                var bmp = GetIconBitmap(command.IconName);
                if (bmp == null)
                    continue;
                var menuItem = new MenuItem
                {
                    Header = new MenuItemHeader(command.Name, bmp),
                };
                menuItem.Click += (s, e) => command.Execute(null);
                menuItems.Add(menuItem);
            }
            // multiple commands => create a submenu
            else
            {
                var cmd = cmds.First();
                var bmp = GetIconBitmap(cmd.IconName);
                if (bmp == null)
                    continue;
                var groupMenuItem = new MenuItem
                {
                    Header = new MenuItemHeader(cmd.GroupName, bmp),
                };

                // Add submenu items
                foreach (var subCommand in cmds)
                {
                    var bmp2 = GetIconBitmap(subCommand.IconName);
                    if (bmp2 == null)
                        continue;
                    var subMenuItem = new MenuItem
                    {
                        Header = new MenuItemHeader(subCommand.Name, bmp2),
                    };
                    subMenuItem.Click += (s, e) => subCommand.Execute(null);
                    groupMenuItem.Items.Add(subMenuItem);
                }

                menuItems.Add(groupMenuItem);
            }
        }

        return menuItems;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }

    private StreamGeometry? GetIconBitmap(string iconName)
    {
        if(iconName == null)
            return null;

        if(ToolBarGeometryIcons.Icons.TryGetValue(System.IO.Path.GetFileNameWithoutExtension(iconName), out var value))
            return value;
        return null;
    }
}