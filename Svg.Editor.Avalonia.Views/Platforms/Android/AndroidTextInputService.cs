using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Svg;
using Svg.Editor.Droid.Services;
using Svg.Editor.Interfaces;
using Svg.Editor.Tools;

[assembly: SvgService(typeof(ITextInputService), typeof(AndroidTextInputService))]
namespace Svg.Editor.Droid.Services
{
    public class AndroidTextInputService : ITextInputService
    {

        public async Task<TextTool.TextProperties> GetUserInput(string title, string textValue = "",
            IEnumerable<string> textSizeOptions = null, int textSizeSelected = 0, int maxTextLength = -1)
        {
            var tcs = new TaskCompletionSource<TextTool.TextProperties>();

            var cp = SvgEngine.Resolve<IContextProvider>();
            var context = cp.Context;
            var result = new TextTool.TextProperties();

            AlertDialog.Builder builder = new AlertDialog.Builder(context);
            builder.SetTitle(title);

            var view = new LinearLayout(context) { Orientation = Orientation.Horizontal };

            view.SetPadding(40, 12, 40, 0);

            // setup spinner for stroke width
            var editTextLayout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent) { Weight = 1 }
            };
            var inputLabel = new TextView(context) { Text = "Text" };
            editTextLayout.AddView(inputLabel);
            // Set up the input
            var input = new EditText(context)
            {
                Text = textValue,
                InputType = InputTypes.TextFlagMultiLine,
                ImeOptions = ImeAction.None
            };
            input.SetSingleLine(false);
            input.SelectAll();
            editTextLayout.AddView(input);
            view.AddView(editTextLayout);

            // setup spinner for dash style
            var spinnerLayout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical
            };
            var spinner2Label = new TextView(context) { Text = "Font size" };
            spinnerLayout.AddView(spinner2Label);
            var spinner2 = new Spinner(context)
            {
                Adapter =
                    new ArrayAdapter(context, Android.Resource.Layout.SimpleSpinnerDropDownItem, textSizeOptions.ToArray())
            };
            spinner2.ItemSelected += (sender, args) => result.FontSizeIndex = args.Position;
            spinner2.SetSelection(textSizeSelected);
            spinnerLayout.AddView(spinner2);
            view.AddView(spinnerLayout);

            builder.SetView(view);

            builder.SetPositiveButton("OK", (sender, args) =>
            {
                result.Text = input.Text;
                tcs.TrySetResult(result);
            });

            builder.SetNegativeButton("Cancel", (sender, args) =>
            {
                result.Text = textValue;
                tcs.TrySetResult(result);
            });

            builder.SetCancelable(false);
            builder.Show();

            // delay showing soft keyboard
            await Task.Delay(150);
            ShowKeyboard(input);

            return await tcs.Task;
        }

        public static void ShowKeyboard(View pView)
        {
            pView.RequestFocus();

            var inputMethodManager = (InputMethodManager) Application.Context.GetSystemService(Context.InputMethodService);
            inputMethodManager.ShowSoftInput(pView, ShowFlags.Implicit);
        }
    }
}