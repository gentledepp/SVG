using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Svg;
using Svg.Editor.Droid.Services;
using Svg.Editor.Tools;

[assembly:SvgService(typeof(IColorInputService), typeof(AndroidColorInputService))]

namespace Svg.Editor.Droid.Services
{
    public class AndroidColorInputService : IColorInputService
    {
        public Task<int> GetIndexFromUserInput(string title, string[] items, string[] colors, int defaultIndex = 0)
        {

            var cp = SvgEngine.Resolve<IContextProvider>();
            var context = cp.Context;

            var builder = new AlertDialog.Builder(context);
            var tcs = new TaskCompletionSource<int>();
            builder.SetTitle(title);
            builder.SetAdapter(new ColorListAdapter(items, colors, context), (sender, args) =>
            {
                tcs.SetResult(args.Which);
            });
            builder.Show();

            return tcs.Task;
        }


    }

    internal class ColorListAdapter : BaseAdapter
    {
        protected string[] Items { get; }
        protected string[] Colors { get; }
        protected Context Context { get; }

        public ColorListAdapter(string[] items, string[] colors, Context context)
        {
            Items = items;
            Colors = colors;
            Context = context;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return new ColorItem { Title = Items[position], Color = Colors[position] };
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var layoutInflater = Context.GetSystemService(Context.LayoutInflaterService) as LayoutInflater;
            var view = convertView ?? layoutInflater?.Inflate(Android.Resource.Layout.SimpleListItem1, parent, false);

            if (view == null) return null;

            var text = view.FindViewById<TextView>(Android.Resource.Id.Text1);

            text.Text = Items[position];

            var bgColor = Android.Graphics.Color.ParseColor(Colors[position]);
            view.SetBackgroundColor(bgColor);

            var brightness = bgColor.R * .299 + bgColor.G * .587 + bgColor.B * .114;
            text.SetTextColor(brightness > 128 ? Android.Graphics.Color.Black : Android.Graphics.Color.White);

            return view;
        }

        public override int Count => Items.Length;

        protected class ColorItem : Java.Lang.Object
        {
            public string Title { get; set; }
            public string Color { get; set; }
        }
    }
}