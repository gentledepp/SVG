using Avalonia;
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
using Svg.Interfaces;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Reflection;
using System.Text;

namespace Svg.Editor.Avalon.Forms
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
                    if(bmp == null)
                        continue;
                    var menuItem = new MenuItem
                    {
                        Header = command.Name,
                        Icon = new Avalonia.Controls.Image
                        {
                            Source = bmp
                        }
                    };
                    menuItem.Click += (s, e) => command.Execute(null);
                    menuItems.Add(menuItem);
                }
                // multiple commands => create a submenu
                else
                {
                    var cmd = cmds.First();
                    var bmp = GetIconBitmap(cmd.IconName);
                    if(bmp == null)
                        continue;
                    var groupMenuItem = new MenuItem
                    {
                        Header = cmd.GroupName,
                        Icon = new Avalonia.Controls.Image
                        {
                            Source = bmp
                        },
                     
                    };

                    // Add submenu items
                    foreach (var subCommand in cmds)
                    {
                        var bmp2 = GetIconBitmap(subCommand.IconName);
                        if(bmp2 == null)
                            continue;
                        var subMenuItem = new MenuItem
                        {
                            Header = subCommand.Name,
                            Icon = new Avalonia.Controls.Image
                            {
                                Source = bmp2
                            }
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

        private Avalonia.Media.Imaging.Bitmap GetIconBitmap(string iconName)
        {

            var iconPath = _imageSourceProvider.Value.GetImage(iconName);

            var name = Assembly.GetExecutingAssembly().GetName().Name;

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{name}.Resources.svg.{iconName}");

            if (stream == null)
                return null;
            
            var svg = SvgDocument.Open<SvgDocument>(stream);
            using var bmp = svg.DrawDocument();

            var iconFileName = iconName + ".png";

            using var fileS = File.OpenWrite(iconFileName);
            bmp.SavePng(fileS);
            fileS.Close();
            return new Avalonia.Media.Imaging.Bitmap(iconFileName);
        }
    }
}
