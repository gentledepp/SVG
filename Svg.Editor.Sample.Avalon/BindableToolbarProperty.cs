using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace Svg.Editor.Sample.Avalon
{
    public static class BindableToolbarProperty
    {
        public static readonly BindableProperty BindableToolbarItemsProperty = BindableProperty.CreateAttached("BindableToolbarItems",
           typeof(List<ToolbarItem>),
           typeof(Page),
           new List<ToolbarItem>(),
           propertyChanged: ToolbarItemsChanged);

        private static void ToolbarItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var page = bindable as Page;

            if (page == null) return;

            if (oldValue != null)
            {
                var otbi = oldValue as IEnumerable<ToolbarItem>;
                var ntbi = newValue as IEnumerable<ToolbarItem>;
                if (otbi?.Count() == ntbi?.Count())
                {
                    var ots = otbi.ToArray();
                    var nts = ntbi.ToArray();
                    var differ = false;
                    for (int i = 0; i < ots.Length; i++)
                    {
                        var ot = ots[i];
                        var nt = nts[i];

                        differ = !string.Equals(ot.Text, nt.Text) ||
                                     ot.Command.CanExecute(null) != nt.Command.CanExecute(null) ||
                                     !string.Equals(ot.Icon?.File, nt.Icon?.File);

                        if (differ)
                            break;
                    }

                    if (!differ)
                        return;
                }

                page.ToolbarItems.Clear();
            }



            var items = newValue as IEnumerable<ToolbarItem>;
            if (items != null)
            {
                foreach (var item in items)
                    page.ToolbarItems.Add(item);
            }
        }

        public static List<ToolbarItem> GetBindableToolbarItems(BindableObject bindable)
        {
            return (List<ToolbarItem>) bindable.GetValue(BindableToolbarItemsProperty);
        }

        public static void SetBindableToolbarItems(BindableObject bindable, List<ToolbarItem> value)
        {
            bindable.SetValue(BindableToolbarItemsProperty, value);
        }
    }
}
