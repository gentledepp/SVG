using System;
using SkiaSharp.Views.Forms;
using Svg.Editor.Forms;
using Svg.Editor.Views.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using SKFormsView = Svg.Editor.Forms.SvgCanvasEditorView;

[assembly: ExportRenderer(typeof(SKFormsView), typeof(TouchCanvasEditorViewRenderer))]

namespace Svg.Editor.Forms
{
    public class TouchCanvasEditorViewRenderer : TouchCanvasViewRendererBase<SKFormsView, TouchSvgCanvasEditorView>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<SKFormsView> e)
        {
            // do clean up old control
            if (Control != null)
            {
                Control.DrawingCanvas = null;
            }

            // do clean up old element
            if (Element != null)
            {
                var oldElement = (SKFormsView)Element;
                oldElement.BindingContextChanged -= OnElementBindingContextChanged;
            }

            base.OnElementChanged(e);

            // setup new element
            if (e.NewElement != null)
            {
                var newElement = e.NewElement;
                newElement.BindingContextChanged -= OnElementBindingContextChanged;
                UpdateBindings(newElement);
            }
        }

        protected override TouchSvgCanvasEditorView CreateNativeView()
        {
            var nv = base.CreateNativeView();
            nv.IsFormsMode = true;
            return nv;
        }

        private void OnElementBindingContextChanged(object sender, EventArgs e)
        {
            UpdateBindings(sender as BindableObject);
        }

        private void UpdateBindings(BindableObject fwe)
        {
            if (fwe != null)
            {
                if (Control != null)
                {
                    Control.DrawingCanvas = fwe.BindingContext as SvgDrawingCanvas;
                }
            }
        }
    }
}
