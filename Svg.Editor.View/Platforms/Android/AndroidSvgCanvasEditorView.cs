using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using SkiaSharp;
using SkiaSharp.Views;
using SkiaSharp.Views.Android;
using Svg.Editor.Droid.Services;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;

namespace Svg.Editor.Views.Droid
{
    public class AndroidSvgCanvasEditorView : SKCanvasView, IPaintSurface
    {
        private AndroidInputEventDetector _detector;
        private ISvgDrawingCanvas _drawingCanvas;

        public bool IsFormsMode { get; set; }

        public ISvgDrawingCanvas DrawingCanvas
        {
            get { return _drawingCanvas; }
            set
            {
                _drawingCanvas = value;
                if (value == null) return;
                _detector?.Dispose();
                _detector = new AndroidInputEventDetector(Context);
                _detector.UserInputEvents.Subscribe(async uie => await DrawingCanvas.OnEvent(uie));

                RegisterCallbacks();
            }
        }

        public AndroidSvgCanvasEditorView(Context context, IAttributeSet attr) : base(context, attr)
        {
        }

        public override bool OnTouchEvent(MotionEvent ev)
        {
            // this is intentionally not awaited
            _detector.OnTouch(ev);
            
            return true;
        }

        protected override async void OnDraw(Canvas canvas)
        {
            if (IsFormsMode)
            {
                base.OnDraw(canvas);
                return;
            }

            if (DrawingCanvas == null)
                return;

	        using (var bitmap =
		        Android.Graphics.Bitmap.CreateBitmap(canvas.Width, canvas.Height, Android.Graphics.Bitmap.Config.Argb8888))
	        {
		        try
		        {
			        using (var surface = SKSurface.Create(canvas.Width, canvas.Height, SKColorType.Rgba8888, SKAlphaType.Premul,
				        bitmap.LockPixels(), canvas.Width * 4))
			        {
				        await DrawingCanvas.OnDraw(new SKCanvasRenderer(surface, canvas.Width, canvas.Height));
			        }
		        }
		        finally
		        {
			        bitmap.UnlockPixels();
		        }

		        canvas.DrawBitmap(bitmap, 0, 0, null);
	        }
        }

        protected override void OnAttachedToWindow()
        {
            AndroidContextProvider._context = Context;

            base.OnAttachedToWindow();
            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            if (_drawingCanvas != null)
            {
                _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
                _drawingCanvas.CanvasInvalidated += OnCanvasInvalidated;
                _drawingCanvas.ToolCommandsChanged -= OnToolCommandsChanged;
                _drawingCanvas.ToolCommandsChanged += OnToolCommandsChanged;
            }
        }

        protected override void OnDetachedFromWindow()
        {
            if(_drawingCanvas != null)
            { 
                _drawingCanvas.CanvasInvalidated -= OnCanvasInvalidated;
                _drawingCanvas.ToolCommandsChanged -= OnToolCommandsChanged;
            }
            base.OnDetachedFromWindow();

            AndroidContextProvider._context = null;
        }

        private void OnCanvasInvalidated(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnToolCommandsChanged(object sender, EventArgs e)
        {
            ((Activity)Context).InvalidateOptionsMenu();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!IsFormsMode)
                {
                    DrawingCanvas?.Dispose();
                    _detector?.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}