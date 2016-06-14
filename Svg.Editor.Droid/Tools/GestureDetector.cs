using System;
using Android.Content;
using Android.Views;
using Svg.Core.Events;

namespace Svg.Droid.Editor.Tools
{
    public class GestureDetector
    {
        private readonly Action<UserInputEvent> _callback;
        public const int InvalidPointerId = -1;
        public int ActivePointerId = InvalidPointerId;
        
        public float LastTouchX;
        public float LastTouchY;

        private bool _scaleInProgress = false;
        private readonly ScaleGestureDetector _scaleDetector;

        public GestureDetector(Context ctx, Action<UserInputEvent> callback)
        {
            _callback = callback;
            _scaleDetector = new ScaleGestureDetector(ctx, new ZoomDetector(this));
        }

        public void OnTouch(MotionEvent ev)
        {
            // detectors always have priority
            _scaleDetector.OnTouchEvent(ev);

            if (_scaleInProgress)
                return;

            UserInputEvent uie = null;

            var x = ev.GetX();
            var y = ev.GetY();

            int action = (int)ev.Action;
            switch (action & (int)MotionEventActions.Mask)
            {
                case (int)MotionEventActions.Down:
                    uie = new PointerEvent(EventType.PointerDown, Svg.Factory.Instance.CreatePointF(LastTouchX, LastTouchY), Svg.Factory.Instance.CreatePointF(x, y));
                    LastTouchX = x;
                    LastTouchY = y;
                    ActivePointerId = ev.GetPointerId(0);
                    break;

                case (int)MotionEventActions.Up:
                    ActivePointerId = InvalidPointerId;
                    uie = new PointerEvent(EventType.PointerUp, Svg.Factory.Instance.CreatePointF(LastTouchX, LastTouchY), Svg.Factory.Instance.CreatePointF(x, y));
                    break;

                case (int)MotionEventActions.Cancel:
                    ActivePointerId = InvalidPointerId;
                    uie = null;
                    break;

                case (int)MotionEventActions.Move:
                    // Only move if the ScaleGestureDetector isn't processing a gesture.
                    //if (!SharedMasterTool.Instance.IsScaleDetectorInProgress())
                    //{
                        var pointerIndex = ev.FindPointerIndex(ActivePointerId);
                        x = ev.GetX(pointerIndex);
                        y = ev.GetY(pointerIndex);
                    
                        var absoluteDeltaX = x - LastTouchX;
                        var absoluteDeltaY = y - LastTouchY;

                        System.Diagnostics.Debug.WriteLine($"{absoluteDeltaX}:{absoluteDeltaY}");

                        uie = new MoveEvent(Svg.Factory.Instance.CreatePointF(LastTouchX, LastTouchY), Svg.Factory.Instance.CreatePointF(x, y), Svg.Factory.Instance.CreatePointF(absoluteDeltaX, absoluteDeltaY));

                        //SharedMasterTool.Instance.CanvasTranslatedPosX += dx / ZoomTool.ScaleFactor;
                        //SharedMasterTool.Instance.CanvasTranslatedPosY += dy / ZoomTool.ScaleFactor;

                    //svgWorkspace.Invalidate();

                        LastTouchX = x;
                        LastTouchY = y;
                    //}
                    //else
                    //{
                    //    var gx = SharedMasterTool.Instance.ScaleDetector.FocusX;
                    //    var gy = SharedMasterTool.Instance.ScaleDetector.FocusY;

                    //    var gdx = gx - SharedMasterTool.Instance.LastGestureX;
                    //    var gdy = gy - SharedMasterTool.Instance.LastGestureY;

                    //    SharedMasterTool.Instance.CanvasTranslatedPosX += gdx / ZoomTool.ScaleFactor;
                    //    SharedMasterTool.Instance.CanvasTranslatedPosY += gdy / ZoomTool.ScaleFactor;

                    //    svgWorkspace.Invalidate();

                    //    SharedMasterTool.Instance.LastGestureX = gx;
                    //    SharedMasterTool.Instance.LastGestureY = gy;
                    //}

                    break;

                case (int)MotionEventActions.PointerUp:

                    int pointerIndex2 = ((int)ev.Action & (int)MotionEventActions.PointerIndexMask)
                            >> (int)MotionEventActions.PointerIndexShift;

                    int pointerId = ev.GetPointerId(pointerIndex2);
                    if (pointerId == ActivePointerId)
                    {
                        // This was our active pointer going up. Choose a new
                        // active pointer and adjust accordingly.
                        int newPointerIndex = pointerIndex2 == 0 ? 1 : 0;
                        x = ev.GetX(newPointerIndex);
                        y = ev.GetY(newPointerIndex);
                        uie = new PointerEvent(EventType.PointerUp, Svg.Factory.Instance.CreatePointF(LastTouchX, LastTouchY), Svg.Factory.Instance.CreatePointF(x, y));

                        LastTouchX = x;
                        LastTouchY = y;
                        ActivePointerId = ev.GetPointerId(newPointerIndex);
                    }
                    else
                    {
                        int tempPointerIndex = ev.FindPointerIndex(ActivePointerId);
                        x = ev.GetX(tempPointerIndex);
                        y = ev.GetY(tempPointerIndex);
                        uie = new PointerEvent(EventType.PointerUp, Svg.Factory.Instance.CreatePointF(LastTouchX, LastTouchY), Svg.Factory.Instance.CreatePointF(x, y));

                        LastTouchX = ev.GetX(tempPointerIndex);
                        LastTouchY = ev.GetY(tempPointerIndex);
                    }

                    break;
            }

            if (uie != null)
                _callback(uie);
        }

        public void Reset()
        {
            LastTouchX = 0;
            LastTouchY = 0;

            ActivePointerId = InvalidPointerId;
        }

        private class ZoomDetector : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            private readonly GestureDetector _owner;

            public ZoomDetector(GestureDetector owner)
            {
                _owner = owner;
            }

            public override bool OnScaleBegin(ScaleGestureDetector detector)
            {
                _owner._scaleInProgress = true;
                var uie = new ScaleEvent(ScaleStatus.Start, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                _owner._callback(uie);
                return true;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                var uie = new ScaleEvent(ScaleStatus.Scaling, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                _owner._callback(uie);

                return true;
            }

            public override void OnScaleEnd(ScaleGestureDetector detector)
            {
                var uie = new ScaleEvent(ScaleStatus.End, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                _owner._callback(uie);
                _owner._scaleInProgress = false;
            }
        }
    }
}