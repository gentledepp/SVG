using Avalonia.LogicalTree;
using Avalonia;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using System;
using Svg.Editor.Avalon.Views;
using Svg.Editor.Avalon.Views.InputDetector;

namespace Svg.Editor.Avalon.Forms
{
    public class SvgCanvasEditorView : SKCanvasView
    {
        public ISvgDrawingCanvas DrawingCanvas
        {
            get { return DataContext as ISvgDrawingCanvas; }
            set { DataContext = value; }
        }

        private IInputDetector _detector;

        protected override void OnInitialized()
        {
            RegisterCallbacks();
            base.OnInitialized();
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            UnregisterCallbacks();
            base.OnDetachedFromLogicalTree(e);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            UnregisterCallbacks();
            base.OnDetachedFromVisualTree(e);
        }

        private void UnregisterCallbacks()
        {
            _detector?.Dispose();
            var canvas = DrawingCanvas;
            if (canvas == null) return;
            canvas.CanvasInvalidated -= DrawingCanvas_CanvasInvalidated;
            canvas.ToolCommandsChanged -= DrawingCanvas_ToolCommandsChanged;
        }

        private void RegisterCallbacks()
        {
            _detector = new InputEventDetector(this);
            _detector.UserInputEvents.Subscribe(async uie => await DrawingCanvas.OnEvent(uie));

            //DrawingCanvas.GestureRecognizer = (IGestureRecognizer)_detector;
            var canvas = DrawingCanvas;
            if (canvas == null) return;
            canvas.CanvasInvalidated += DrawingCanvas_CanvasInvalidated;
            canvas.ToolCommandsChanged += DrawingCanvas_ToolCommandsChanged;
        }

        private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
        {
            // repaint the native control
            InvalidateSurface();
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