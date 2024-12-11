using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Android.Widget;
using Svg;
using Svg.Editor.Droid.Services;
using Svg.Editor.Tools;

[assembly: SvgService(typeof(IStrokeStyleOptionsInputService), typeof(AndroidStrokeStyleOptionsInputService))]
namespace Svg.Editor.Droid.Services
{
    public class AndroidStrokeStyleOptionsInputService : IStrokeStyleOptionsInputService
    {
        public Task<StrokeStyleTool.StrokeStyleOptions> GetUserInput(string title, IEnumerable<string> strokeDashOptions, int strokeDashSelected, IEnumerable<string> strokeWidthOptions, int strokeWidthSelected)
        {
            var cp = SvgEngine.Resolve<IContextProvider>();
            var context = cp.Context;

            var builder = new AlertDialog.Builder(context);
            var tcs = new TaskCompletionSource<StrokeStyleTool.StrokeStyleOptions>();
            var strokeStyleOptions = new StrokeStyleTool.StrokeStyleOptions();

            // setup builder
            builder.SetTitle(title);

            var view = new LinearLayout(context) { Orientation = Orientation.Horizontal };

            view.SetPadding(40, 12, 40, 0);

            // setup spinner for stroke width
            var spinner1Layout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            var spinner1Label = new TextView(context) { Text = "Stroke width" };
            spinner1Layout.AddView(spinner1Label);
            var spinner1 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem, strokeWidthOptions.ToArray())
            };
            spinner1.ItemSelected += (sender, args) => strokeStyleOptions.StrokeWidthIndex = args.Position;
            spinner1.SetSelection(strokeWidthSelected);
            spinner1Layout.AddView(spinner1);
            view.AddView(spinner1Layout);

            // setup spinner for dash style
            var spinner2Layout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            var spinner2Label = new TextView(context) { Text = "Dash style" };
            spinner2Layout.AddView(spinner2Label);
            var spinner2 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem, strokeDashOptions.ToArray())
            };
            spinner2.ItemSelected += (sender, args) => strokeStyleOptions.StrokeDashIndex = args.Position;
            spinner2.SetSelection(strokeDashSelected);
            spinner2Layout.AddView(spinner2);
            view.AddView(spinner2Layout);

            builder.SetView(view);

            builder.SetPositiveButton("OK", (sender, args) => tcs.TrySetResult(strokeStyleOptions));

            // show dialog
            builder.Show();

            return tcs.Task;
        }
    }
}