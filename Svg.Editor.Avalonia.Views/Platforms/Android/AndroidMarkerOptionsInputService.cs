using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Android.Widget;
using Svg;
using Svg.Editor.Droid.Services;
using Svg.Editor.Tools;

[assembly: SvgService(typeof(IMarkerOptionsInputService), typeof(AndroidMarkerOptionsInputService))]
namespace Svg.Editor.Droid.Services
{
    public class AndroidMarkerOptionsInputService : IMarkerOptionsInputService
    {
        public Task<int[]> GetUserInput(string title, IEnumerable<string> markerStartOptions, int markerStartSelected, IEnumerable<string> markerEndOptions, int markerEndSelected)
        {
            var cp = SvgEngine.Resolve<IContextProvider>();
            var context = cp.Context;

            var builder = new AlertDialog.Builder(context);
            var tcs = new TaskCompletionSource<int[]>();
            var result = new int[2];

            // setup builder
            builder.SetTitle(title);

            // setup spinner for start marker
            var view = new LinearLayout(context) { Orientation = Orientation.Horizontal };

            view.SetPadding(40, 12, 40, 0);

            var spinner1Layout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            var spinner1Label = new TextView(context) { Text = "Marker start" };
            spinner1Layout.AddView(spinner1Label);
            var spinner1 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                        markerStartOptions.ToArray())
            };
            spinner1.ItemSelected += (sender, args) => result[0] = args.Position;
            spinner1.SetSelection(markerStartSelected);
            spinner1Layout.AddView(spinner1);
            view.AddView(spinner1Layout);

            // setup spinner for end marker
            var spinner2Layout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            var spinner2Label = new TextView(context) { Text = "Marker end" };
            spinner2Layout.AddView(spinner2Label);
            var spinner2 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                        markerEndOptions.ToArray())
            };
            spinner2.ItemSelected += (sender, args) => result[1] = args.Position;
            spinner2.SetSelection(markerEndSelected);
            spinner2Layout.AddView(spinner2);
            view.AddView(spinner2Layout);

            builder.SetView(view);

            builder.SetPositiveButton("OK", (sender, args) => tcs.TrySetResult(result));

            // show dialog
            builder.Show();

            return tcs.Task;
        }
    }
}