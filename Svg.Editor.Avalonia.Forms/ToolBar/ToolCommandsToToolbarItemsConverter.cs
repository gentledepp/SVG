using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Svg.Editor.Avalon.Views.CustomGestureRecognizer;

namespace Svg.Editor.Avalon.Forms.ToolBar;

public class ToolCommandsToToolbarItemsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var commandLists = value as IEnumerable<IEnumerable<IToolCommand>>;

        var menuItems = new List<MenuItem>();

        foreach (var commands in commandLists.Where(l => l.Any()).OrderBy(l => l.Min(li => li.Sort)))
        {
            var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
            if (cmds.Length == 0)
                continue;

            // single command => create a menu item
            if (cmds.Length == 1)
            {
                var command = cmds.Single();
                var icon = GetDrawingGroup(command, command.IconName);

                var menuItem = GetGroupMenuItem(command.Name, icon);
                menuItem.Click += (s, e) => command.Execute(null);
                menuItem.GestureRecognizers.Add(new LongPressTipGestureRecognizer(menuItem ,command.Description));
                menuItems.Add(menuItem);
            }
            // multiple commands => create a submenu
            else
            {
                var cmd = cmds.First();
                var icon = GetDrawingGroup(cmd, cmd.GroupIconName);

                var selectedCommand = commands.ToArray().FirstOrDefault(c => c.Name == cmd.GroupName);

                var groupMenuItem = GetGroupMenuItem(cmd.GroupName, icon);

                if (selectedCommand != null)
                {
                    groupMenuItem.GestureRecognizers.Add(
                        new LongPressTipGestureRecognizer(groupMenuItem, selectedCommand.Description));
                    var selectedMenuItem = new MenuItem()
                    {
                        Header = new MenuItemHeader(selectedCommand.Name, icon)
                    };
                    selectedMenuItem.Click += (sender, args) => selectedCommand.Execute(null);

                    groupMenuItem.Items.Add(selectedMenuItem);
                }
                else
                {
                    groupMenuItem.GestureRecognizers.Add(new LongPressTipGestureRecognizer(groupMenuItem, cmd.Description));
                }


                // Add submenu items
                foreach (var subCommand in cmds)
                {
                    var subIcon = GetDrawingGroup(subCommand, subCommand.IconName);

                    var subMenuItem = new MenuItem
                    {
                        Header = new MenuItemHeader(subCommand.Name, subIcon),
                    };
                    subMenuItem.Click += (s, e) => subCommand.Execute(null);

                    groupMenuItem.Items.Add(subMenuItem);
                }

                menuItems.Add(groupMenuItem);
            }
        }
        return menuItems;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private MenuItem GetGroupMenuItem(string name, DrawingGroup? icon)
    {
        // Mobile does not need text on group icons since it would be to wide to work with
        if (Application.Current.ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            return new MenuItem
            {
                Header = new MenuItemHeader(icon),
            };
        }

        return new MenuItem
        {
            Header = new MenuItemHeader(name, icon),
        };
    }

    private DrawingGroup? GetDrawingGroup(IToolCommand command,string iconName)
    {
        if(iconName == null)
            return null;
        
        if(Application.Current.TryGetResource(Path.GetFileNameWithoutExtension(iconName), out var value))
        {
            var image = value as DrawingGroup;
            PrepareIcons(command, image, iconName);

            return image;
        }
        return null;
    }

    private void PrepareIcons(IToolCommand command, DrawingGroup? icon, string iconName)
    {
        if (icon == null)
            return;

        // set default brush color for icons
        foreach (var drawing in icon.Children
                     .Concat(icon.Children.OfType<DrawingGroup>()
                         .SelectMany(group => group.Children
                             .OfType<GeometryDrawing>()))
                     .OfType<GeometryDrawing>().ToArray())
        {
            drawing.Brush = new SolidColorBrush(Colors.White);
        }

        if (command.Tool is ColorTool colorTool)
        {
            var element = icon?.Children.OfType<GeometryDrawing>().First();
            if (element != null)
            {
                element.Brush = new SolidColorBrush(Color.Parse(colorTool.HexColor));
            }
        }

        if (command.Tool is PolygonTool polygonTool || iconName == "ic_polygon.svg")
        {
            var element = icon?.Children.OfType<GeometryDrawing>().First();
            if (element != null)
            {
                element.Brush = new SolidColorBrush(Colors.Transparent);
                element.Pen = new Avalonia.Media.Pen()
                {
                    Brush = new SolidColorBrush(Colors.White),
                    Thickness = 2
                };
            }
        }
    }
}