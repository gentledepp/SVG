namespace Svg.Editor.Samples.Forms
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

            var otbi = page.ToolbarItems;
            var ntbi = newValue as IList<ToolbarItem>;

            if (otbi is null && ntbi is null)
                return;

            if (ntbi is null)
            {
                page.ToolbarItems.Clear();
                return;
            }

            string Identify(ToolbarItem tbi) => $"{tbi.Text}_{tbi.IconImageSource}_{tbi.Order}";

            try
            {
                if (otbi != null)
                {
                    if (otbi.Select(Identify).SequenceEqual(ntbi.Select(Identify)))
                        return;

                    foreach (var toDelete in otbi.Where(o => ntbi.All(n => Identify(n) != Identify(o))).ToList())
                        page.ToolbarItems.Remove(toDelete);
                    
                    foreach(var toAdd in ntbi.Where(n => otbi.All(o => Identify(n) != Identify(o))))
                        page.ToolbarItems.Add(toAdd);

                    // re-sort items
                    var index = 0;
                    foreach (var tbi in ntbi)
                    {
                        var existing = page.ToolbarItems.Single(o => Identify(o) == Identify(tbi));
                        // toolbaritems are sorted by "priority" - this seems to speed up the sorting process (as opposed to removing and inserting at the new index)
                        existing.Priority = index++;
                        
                        existing.Order = tbi.Order;
                        existing.IsEnabled = tbi.Command.CanExecute(null);
                    }
                }
                else {
                    foreach (var item in ntbi)
                        page.ToolbarItems.Add(item);
                }
            }
            catch (Exception)
            {
                // here we just clear the ToolbarItems - if we have an error, we just get an empty toolbar
                page.ToolbarItems.Clear();
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
