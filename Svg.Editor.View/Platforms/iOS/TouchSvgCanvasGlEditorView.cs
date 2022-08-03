using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Foundation;
using SkiaSharp.Views.iOS;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using UIKit;

namespace Svg.Editor.iOS
{
    [Register(nameof(TouchSvgCanvasGlEditorView))]
    public class TouchSvgCanvasGlEditorView : SKGLView
    {
        private TouchInputEventDetector _detector;
        private ISvgDrawingCanvas _drawingCanvas;
        private readonly Subject<UserInputEvent> _detectedGestures = new Subject<UserInputEvent>();

        public bool IsFormsMode { get; set; }

        public ISvgDrawingCanvas DrawingCanvas
        {
            get { return _drawingCanvas; }
            set
            {
                _drawingCanvas = value;
                if (value == null) return;
                _detector?.Dispose();
                _detector = new TouchInputEventDetector(this);
                _detector.UserInputEvents.Subscribe(async uie => await DrawingCanvas.OnEvent(uie));
                _detector.UserInputEvents.Subscribe(_detectedGestures.OnNext);
            }
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            HandleTouches(touches, (arr) => _detector.OnBegin(arr));
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            HandleTouches(touches, (arr) => _detector.OnMove(arr));
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            HandleTouches(touches, (arr) => _detector.OnEnd(arr));
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            HandleTouches(touches, (arr) => _detector.OnCancel(arr));
        }

        private static void HandleTouches(NSSet touches, Action<UITouch[]> action)
        {
            if (touches.Count == 1)
            {
                UITouch touch = touches.AnyObject as UITouch;

                if (touch != null)
                {
                    var arr = new[] { touch };
                    action(arr);
                }
            }
            else if (touches.Count > 1)
            {
                var touchesList = new List<UITouch>();
                foreach (var touch in touches)
                {
                    if (touch != null)
                    {
                        touchesList.Add((UITouch)touch);
                    }
                }
                action(touchesList.ToArray());
            }
        }
    }
}
