using Avalonia.LogicalTree;
using Avalonia;
using Svg.Editor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Editor.iOS;
using Svg.Editor.Avalon.Views;

namespace Svg.Editor.Avalon.Forms
{
    public partial class SvgCanvasEditorView 
    { 
        private TouchInputEventDetector _detector;
        private readonly Subject<UserInputEvent> _detectedGestures = new Subject<UserInputEvent>();
        private ISvgDrawingCanvas _drawingCanvas;

        protected override void OnInitialized()
        {
            _detector?.Dispose();
            _detector = new TouchInputEventDetector(this);
            _detector.UserInputEvents.Subscribe(async uie => await DrawingCanvas.OnEvent(uie));
            _detector.UserInputEvents.Subscribe(_detectedGestures.OnNext);
            base.OnInitialized();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            _detector?.Dispose();
            _detector = new TouchInputEventDetector(this);
            _detector.UserInputEvents.Subscribe(async uie => await DrawingCanvas.OnEvent(uie));
            RegisterCallbacks();
            base.OnDataContextChanged(e);
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
            var canvas = DrawingCanvas;
            if (canvas == null)
                return;

            canvas.CanvasInvalidated -= DrawingCanvas_CanvasInvalidated;
            canvas.ToolCommandsChanged -= DrawingCanvas_ToolCommandsChanged;

            SurfaceInvalidated -= OnSurfaceInvalidated;
            GetCanvasSize -= OnGetCanvasSize;


        }

        private void RegisterCallbacks()
        {
            var canvas = DrawingCanvas;
            if (canvas == null)
                return;

            SurfaceInvalidated += OnSurfaceInvalidated;
            GetCanvasSize += OnGetCanvasSize;
            canvas.CanvasInvalidated += DrawingCanvas_CanvasInvalidated;
            canvas.ToolCommandsChanged += DrawingCanvas_ToolCommandsChanged;
        }

        private void OnSurfaceInvalidated(object sender, EventArgs eventArgs)
        {
            // repaint the native control
            InvalidateSurface();
        }

        // the user asked for the size
        private void OnGetCanvasSize(object sender, GetCanvasSizeEventArgs e)
        {
            //e.CanvasSize = this?.CanvasSize;
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
