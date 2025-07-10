using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.Tools;
using Xamarin.Forms;

namespace Svg.Editor.Forms
{
    public class ToolCommandsToToolbarItemsConverter : IValueConverter
    {
        private Lazy<IImageSourceProvider> _imageSourceProvider = new Lazy<IImageSourceProvider>(SvgEngine.TryResolve<IImageSourceProvider>);
        private Lazy<IToolbarIconSizeProvider> _toolbarIconSizeProvider = new Lazy<IToolbarIconSizeProvider>(SvgEngine.TryResolve<IToolbarIconSizeProvider>);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int shownActions;
            if (parameter is int)
                shownActions = (int) parameter;
            else
            {
                shownActions = 3;
            }

            var imageProvider = _imageSourceProvider.Value;
            var iconDimension = _toolbarIconSizeProvider.Value?.GetSize();

            var commandLists = (value as IEnumerable<IEnumerable<IToolCommand>>);
            var items = new List<ToolbarItem>();
            foreach (var commands in commandLists.Where(l => l.Any()).OrderBy(l => l.Min(li => li.Sort)))
            {
                var cmds = commands.Where(c => c.CanExecute(null)).ToArray();
                if (cmds.Length == 0)
                    continue;

                var itemOrder = items.Count >= shownActions ? ToolbarItemOrder.Secondary : ToolbarItemOrder.Primary;
                
                // single command => show as toolbaritem
                if (cmds.Length == 1)
                {
                    var command = cmds.Single();
                    items.Add(new ToolbarItem
                    {
                        Text = command.Name,
                        AutomationId = command.Name,
                        Icon = imageProvider.GetImage(command.IconName, iconDimension),
                        Command = new Command(() => command.Execute(null), () => command.CanExecute(null)),
                        Order = itemOrder
                    });
                }
                // multiple commands => create action menu
                else
                {
                    var cmd = cmds.First();
                    items.Add(new ToolbarItem
                    {
                        Text = cmd.GroupName,
                        AutomationId = cmd.GroupName,
                        Icon = imageProvider.GetImage(cmd.GroupIconName, iconDimension),
                        Command = new Command(async () =>
                        {
                            var cs = cmds;
                            var tags = cs.Select(c => c.Name).ToArray();

                            var result = await Application.Current.MainPage.DisplayActionSheet(cmd.GroupName, "cancel", null, tags);

                            var selectedCommand = cs.FirstOrDefault(c => c.Name == result);
                            selectedCommand?.Execute(null);
                        }),
                        Order = itemOrder
                    });

                }
            }

            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
