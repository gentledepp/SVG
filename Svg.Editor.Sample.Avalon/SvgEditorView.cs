
using Avalonia.Controls;
using Avalonia.Input;
using Svg.Editor.Avalon.Forms;

namespace Svg.Editor.Sample.Avalon
{
    public class SvgEditorView : SvgCanvasEditorView
    {
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            // Check if the click is on a menu item
            if (e.Source is MenuItem)
            {
                // Allow menu interaction
                base.OnPointerPressed(e);
                return;
            }

            // Your existing logic
            base.OnPointerPressed(e);
        }
    }
}
