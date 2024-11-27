using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Svg.Interfaces;
using Avalonia.Media;

namespace Svg.Editor.Avalonia.Forms
{
    public class ToolCommandsToToolbarItemsConverter : IValueConverter
    {
        private Lazy<IImageSourceProvider> _imageSourceProvider = new Lazy<IImageSourceProvider>(SvgEngine.TryResolve<IImageSourceProvider>);
        private Lazy<IToolbarIconSizeProvider> _toolbarIconSizeProvider = new Lazy<IToolbarIconSizeProvider>(SvgEngine.TryResolve<IToolbarIconSizeProvider>);       
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int shownActions;
            if (parameter is int intParam)
                shownActions = intParam;
            else
            {
                shownActions = 3;
            }

            var imageProvider = _imageSourceProvider.Value;
            var iconDimension = _toolbarIconSizeProvider.Value?.GetSize();
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
                    var menuItem = new MenuItem
                    {
                        Header = command.Name,
                        //Icon = new 
                        //{
                        //    Source = imageProvider.GetImage(command.IconName, iconDimension)
                        //}
                    };
                    menuItem.Click += (s, e) => command.Execute(null);
                    menuItems.Add(menuItem);
                }
                // multiple commands => create a submenu
                else
                {
                    var cmd = cmds.First();

                    var groupMenuItem = new MenuItem
                    {
                        Header = cmd.GroupName,
                        //Icon = new Image()
                        //{
                        //    Source = imageProvider.GetImage(cmd.IconName, iconDimension)
                        //}
                    };

                    // Add submenu items
                    foreach (var subCommand in cmds)
                    {
                        var subMenuItem = new MenuItem
                        {
                            Header = subCommand.Name,
                            //Icon = new Image()
                            //{
                            //   Source = imageProvider.GetImage(subCommand.IconName, iconDimension)
                            //}
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

    }
}