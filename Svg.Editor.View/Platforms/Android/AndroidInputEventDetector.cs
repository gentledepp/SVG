using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Android.Content;
using Android.Views;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Editor.Droid.Services
{
    public class AndroidInputEventDetector : IInputEventDetector, IDisposable
    {
        public const int InvalidPointerId = -1;
        public int ActivePointerId = InvalidPointerId;

        private float _lastTouchX;
        private float _lastTouchY;

        private readonly ScaleGestureDetector _scaleDetector;
        private readonly RotateDetector _rotateDetector;
        private float _pointerDownX;
        private float _pointerDownY;
        private readonly Subject<UserInputEvent> _detectedGestures = new Subject<UserInputEvent>();

        public IObservable<UserInputEvent> UserInputEvents => _detectedGestures.AsObservable();

        public AndroidInputEventDetector(Context ctx)
        {
            var scaleListener = new ScaleDetector();
            scaleListener.OnEvent += (sender, ev) => _detectedGestures.OnNext(ev);
            _scaleDetector = new ScaleGestureDetector(ctx, scaleListener);
            _rotateDetector = new RotateDetector();
            _rotateDetector.OnRotate += (sender, ev) => _detectedGestures.OnNext(ev);
        }

        public void OnTouch(MotionEvent ev)
        {
            // detectors always have priority
            _scaleDetector.OnTouchEvent(ev);
            _rotateDetector.OnTouchEvent(ev);

            UserInputEvent uie = null;

            var x = 0f;
            var y = 0f;

            var pointerCount = ev.PointerCount;
            for (var i = 0; i < pointerCount; i++)
            {
                x += ev.GetX(i) / pointerCount;
                y += ev.GetY(i) / pointerCount;
            }

            var action = (int) ev.Action;
            var maskedAction = action & (int) MotionEventActions.Mask;
            switch (maskedAction)
            {
                case (int) MotionEventActions.Down:
                case (int) MotionEventActions.Pointer1Down:
                    uie = new PointerEvent(EventType.PointerDown,
                        PointF.Create(_pointerDownX, _pointerDownY),
                        PointF.Create(_lastTouchX, _lastTouchY),
                        PointF.Create(x, y), ev.PointerCount);

                    _lastTouchX = x;
                    _lastTouchY = y;

                    _pointerDownX = x;
                    _pointerDownY = y;

                    ActivePointerId = ev.GetPointerId(0);
                    break;

                case (int) MotionEventActions.Up:
                    ActivePointerId = InvalidPointerId;
                    uie = new PointerEvent(EventType.PointerUp,
                        PointF.Create(_pointerDownX, _pointerDownY),
                        PointF.Create(_lastTouchX, _lastTouchY),
                        PointF.Create(x, y), ev.PointerCount);
                    break;

                case (int) MotionEventActions.Cancel:
                    ActivePointerId = InvalidPointerId;
                    uie = new PointerEvent(EventType.Cancel,
                        PointF.Create(_pointerDownX, _pointerDownY),
                        PointF.Create(_lastTouchX, _lastTouchY),
                        PointF.Create(x, y), 1);
                    break;

                case (int) MotionEventActions.Move:
                    var relativeDeltaX = x - _lastTouchX;
                    var relativeDeltaY = y - _lastTouchY;

                    uie = new MoveEvent(PointF.Create(_pointerDownX, _pointerDownY),
                        PointF.Create(_lastTouchX, _lastTouchY),
                        PointF.Create(x, y),
                        PointF.Create(relativeDeltaX, relativeDeltaY),
                        ev.PointerCount);

                    _lastTouchX = x;
                    _lastTouchY = y;
                    break;

                case (int) MotionEventActions.PointerUp:

                    var pointerIndex2 = ((int) ev.Action & (int) MotionEventActions.PointerIndexMask)
                                        >> (int) MotionEventActions.PointerIndexShift;

                    var pointerId = ev.GetPointerId(pointerIndex2);
                    if (pointerId == ActivePointerId)
                    {
                        // This was our active pointer going up. Choose a new
                        // active pointer and adjust accordingly.
                        var newPointerIndex = pointerIndex2 == 0 ? 1 : 0;
                        x = ev.GetX(newPointerIndex);
                        y = ev.GetY(newPointerIndex);
                        uie = new PointerEvent(EventType.PointerUp,
                            PointF.Create(_pointerDownX, _pointerDownY),
                            PointF.Create(_lastTouchX, _lastTouchY),
                            PointF.Create(x, y), 1);

                        _lastTouchX = x;
                        _lastTouchY = y;
                        ActivePointerId = ev.GetPointerId(newPointerIndex);
                    }
                    else
                    {
                        var tempPointerIndex = ev.FindPointerIndex(ActivePointerId);
                        x = ev.GetX(tempPointerIndex);
                        y = ev.GetY(tempPointerIndex);
                        uie = new PointerEvent(EventType.PointerUp,
                            PointF.Create(_pointerDownX, _pointerDownY),
                            PointF.Create(_lastTouchX, _lastTouchY),
                            PointF.Create(x, y), 1);

                        _lastTouchX = ev.GetX(tempPointerIndex);
                        _lastTouchY = ev.GetY(tempPointerIndex);
                    }

                    break;
            }

            if (uie != null)
                _detectedGestures.OnNext(uie);
        }

        public void Reset()
        {
            _lastTouchX = 0;
            _lastTouchY = 0;

            ActivePointerId = InvalidPointerId;
        }

        private class ScaleDetector : ScaleGestureDetector.SimpleOnScaleGestureListener
        {
            public event EventHandler<UserInputEvent> OnEvent;

            public override bool OnScaleBegin(ScaleGestureDetector detector)
            {
                var uie = new ScaleEvent(ScaleStatus.Start, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                OnEvent?.Invoke(this, uie);
                return true;
            }

            public override bool OnScale(ScaleGestureDetector detector)
            {
                var uie = new ScaleEvent(ScaleStatus.Scaling, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                OnEvent?.Invoke(this, uie);
                return true;
            }

            public override void OnScaleEnd(ScaleGestureDetector detector)
            {
                var uie = new ScaleEvent(ScaleStatus.End, detector.ScaleFactor, detector.FocusX, detector.FocusY);
                OnEvent?.Invoke(this, uie);
            }
        }

        /// <summary>
        /// Copied from: http://stackoverflow.com/questions/10682019/android-two-finger-rotation
        /// </summary>
        private class RotateDetector
        {
            private float _fX, _fY, _sX, _sY;
            private int _ptrId1, _ptrId2;
            private float? _startAngle;
            private float? _previousAngle;
            private float _angle;

            public event EventHandler<UserInputEvent> OnRotate;

            public RotateDetector()
            {
                _ptrId1 = InvalidPointerId;
                _ptrId2 = InvalidPointerId;
            }

            public void OnTouchEvent(MotionEvent ev)
            {
                var action = (int) ev.Action;
                var ptrIndex1 = _ptrId1 != InvalidPointerId ? ev.FindPointerIndex(_ptrId1) : -1;
                var ptrIndex2 = _ptrId2 != InvalidPointerId ? ev.FindPointerIndex(_ptrId2) : -1;

                switch (action & (int) MotionEventActions.Mask)
                {
                    case (int) MotionEventActions.Down:
                        _ptrId1 = ev.GetPointerId(ev.ActionIndex);
                        break;
                    case (int) MotionEventActions.PointerDown:
                        _ptrId2 = ev.GetPointerId(ev.ActionIndex);
                        if (ptrIndex1 != -1)
                        {
                            _sX = ev.GetX(ptrIndex1);
                            _sY = ev.GetY(ptrIndex1);
                        }
                        _fX = ev.GetX(ev.ActionIndex);
                        _fY = ev.GetY(ev.ActionIndex);
                        break;
                    case (int) MotionEventActions.Move:
                        if (ptrIndex1 == -1 || ptrIndex2 == -1) break;

                        var nsX = ev.GetX(ptrIndex1);
                        var nsY = ev.GetY(ptrIndex1);
                        var nfX = ev.GetX(ptrIndex2);
                        var nfY = ev.GetY(ptrIndex2);

                        _angle = AngleBetweenLines(_fX, _fY, _sX, _sY, nfX, nfY, nsX, nsY);

                        if (_startAngle == null)
                        {
                            _startAngle = _angle;
                            var uie = new RotateEvent(0, 0, RotateStatus.Start, ev.PointerCount);
                            //System.Diagnostics.Debug.WriteLine(uie.DebuggerDisplay);
                            OnRotate?.Invoke(this, uie);
                        }
                        if (_previousAngle != null)
                        {
                            var delta = (_previousAngle.Value - _angle) % 360;
                            var absoluteDelta = (_startAngle.Value - _angle) % 360;

                            var uie = new RotateEvent(delta, absoluteDelta, RotateStatus.Rotating, ev.PointerCount);
                            //System.Diagnostics.Debug.WriteLine(uie.DebuggerDisplay);
                            OnRotate?.Invoke(this, uie);
                        }
                        _previousAngle = _angle;
                        break;
                    case (int) MotionEventActions.Up:
                        _ptrId1 = InvalidPointerId;
                        CleanUp();
                        break;
                    case (int) MotionEventActions.PointerUp:
                        _ptrId2 = InvalidPointerId;
                        CleanUp();
                        break;
                    case (int) MotionEventActions.Cancel:
                        _ptrId1 = InvalidPointerId;
                        _ptrId2 = InvalidPointerId;
                        CleanUp();
                        break;
                }
            }

            private void CleanUp()
            {
                // we have been rotating
                if (_startAngle.HasValue && _previousAngle.HasValue)
                {
                    var delta = (_previousAngle.Value - _angle) % 360;
                    var absoluteDelta = (_startAngle.Value - _angle) % 360;

                    var uie = new RotateEvent(delta, absoluteDelta, RotateStatus.End, 0);
                    //System.Diagnostics.Debug.WriteLine(uie.DebuggerDisplay);
                    OnRotate?.Invoke(this, uie);
                }

                _startAngle = null;
                _previousAngle = null;
                _angle = 0f;
            }

            private static float AngleBetweenLines(float fX, float fY, float sX, float sY, float nfX, float nfY, float nsX, float nsY)
            {
                var angle1 = (float) Math.Atan2(fY - sY, fX - sX);
                var angle2 = (float) Math.Atan2(nfY - nsY, nfX - nsX);

                var angle = (float) RadianToDegree(angle1 - angle2) % 360;
                if (angle < -180f) angle += 360.0f;
                if (angle > 180f) angle -= 360.0f;
                return angle;
            }

            private static double RadianToDegree(double angle)
            {
                return angle * (180.0 / Math.PI);
            }
        }

        public void Dispose()
        {
            _detectedGestures?.Dispose();
        }
    }
}