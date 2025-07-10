using System;
using SkiaSharp.Views.Forms;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;

namespace Svg.Editor.Forms
{
    public class SvgCanvasEditorView : SKCanvasViewX
    {
        public ISvgDrawingCanvas DrawingCanvas
        {
            get { return BindingContext as ISvgDrawingCanvas; }
            set { BindingContext = value; }
        }

        protected override void OnPropertyChanging(string propertyName = null)
        {
            if (propertyName == "BindingContext")
            {
                UnregisterCallbacks();
            }

            base.OnPropertyChanging(propertyName);
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            if (propertyName == "BindingContext")
            {
                RegisterCallbacks();
            }

            base.OnPropertyChanged(propertyName);
        }

        private void UnregisterCallbacks()
        {
            var canvas = DrawingCanvas;
            if (canvas == null)
                return;

            canvas.CanvasInvalidated -= DrawingCanvas_CanvasInvalidated;
            canvas.ToolCommandsChanged -= DrawingCanvas_ToolCommandsChanged;
        }

        private void RegisterCallbacks()
        {
            var canvas = DrawingCanvas;
            if (canvas == null)
                return;

            canvas.CanvasInvalidated += DrawingCanvas_CanvasInvalidated;
            canvas.ToolCommandsChanged += DrawingCanvas_ToolCommandsChanged;
        }

        private void DrawingCanvas_ToolCommandsChanged(object sender, System.EventArgs e)
        {
        }

        private void DrawingCanvas_CanvasInvalidated(object sender, System.EventArgs e)
        {
            InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            DrawingCanvas?.OnDraw(new SKCanvasRenderer(e.Surface, e.Info.Width, e.Info.Height));
        }
    }
}
