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


            try
            {
                if (otbi != null)
                {
                    if (otbi.Select(o => o.Text).SequenceEqual(ntbi.Select(n => n.Text)))
                        return;

                    foreach (var toDelete in otbi.Where(o => ntbi.All(n => n.Text != o.Text)))
                        page.ToolbarItems.Remove(toDelete);
                    
                    var toAdd = ntbi.Where(n => otbi.All(o => o.Text != n.Text)).ToHashSet();

                    for (int i = 0; i < ntbi.Count; i++)
                    {
                        if (toAdd.Contains(ntbi[i]))
                            page.ToolbarItems.Insert(i, ntbi[i]);
                        // item exists and must be moved
                        else if (otbi.Count > i && otbi[i].Text != ntbi[i].Text)
                            continue;
                        else
                        {
                            var old = otbi.Single(o => o.Text == ntbi[i].Text);
                            page.ToolbarItems.Remove(old);
                            page.ToolbarItems.Insert(i, old);
                        }
                    }
                }
                else {
                    foreach (var item in ntbi)
                        page.ToolbarItems.Add(item);
                }
            }
            catch (Exception x)
            {
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
