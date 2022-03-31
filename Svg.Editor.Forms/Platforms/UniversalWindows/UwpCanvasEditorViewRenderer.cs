using System;
using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Svg.Editor.Forms.UWP;
using Svg.Editor.Views.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(SvgCanvasEditorView), typeof(UwpCanvasEditorViewRenderer))]
namespace Svg.Editor.Forms.UWP
{
    public class UwpCanvasEditorViewRenderer : UwpCanvasViewRendererBase<SvgCanvasEditorView, SkiaSharp.Views.UWP.SKXamlCanvasX>
    {
        private UwpGestureRecognizer _gestureRecognizer;
        private IDisposable _sub;

        protected override void OnElementChanged(ElementChangedEventArgs<SvgCanvasEditorView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                _gestureRecognizer = new UwpGestureRecognizer(Control);
                _sub = _gestureRecognizer.UserInputEvents.Subscribe(async uie => await Element.DrawingCanvas.OnEvent(uie));
                Element.DrawingCanvas.GestureRecognizer = _gestureRecognizer;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _sub.Dispose();
            _gestureRecognizer.Dispose();
        }
    }
}
