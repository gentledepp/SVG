using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CoreGraphics;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using UIKit;
using PointF = Svg.Interfaces.PointF;

namespace Svg.Editor.iOS
{
    /// <summary>
    /// see: https://developer.xamarin.com/guides/ios/application_fundamentals/touch/touch_in_ios/
    /// </summary>
    public class TouchInputEventDetector : IInputEventDetector, IDisposable
    {
        private readonly UIView _owner;
        private readonly Subject<UserInputEvent> _gestureSubject = new Subject<UserInputEvent>();
        private UIPinchGestureRecognizer _zoomRecognizer;
        private UIRotationGestureRecognizer _rotationRecognizer;

        private readonly Dictionary<UITouch, PointF> _pointerDownPositions = new Dictionary<UITouch, PointF>();
        private readonly Dictionary<UITouch, PointF> _previousPointerPositions = new Dictionary<UITouch, PointF>();
        private float _scaleFactor;
        private float _previousRotation = 0;
        private float _scaleStart;
        private double _previousScale;

        public TouchInputEventDetector(UIView owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            _owner = owner;
            _owner.UserInteractionEnabled = true;
            _owner.MultipleTouchEnabled = true;

            _rotationRecognizer = new UIRotationGestureRecognizer(this.OnRotate);
            _rotationRecognizer.CancelsTouchesInView = false;
            _rotationRecognizer.ShouldRecognizeSimultaneously += (r1, r2) => true;

            _zoomRecognizer = new UIPinchGestureRecognizer(this.OnZoom);
            _zoomRecognizer.CancelsTouchesInView = false;
            _zoomRecognizer.ShouldRecognizeSimultaneously += (r1, r2) => true;
            
            _owner.AddGestureRecognizer(_zoomRecognizer);
            _owner.AddGestureRecognizer(_rotationRecognizer);

            _scaleFactor = (float)UIScreen.MainScreen.Scale;
        }

        private void OnZoom(UIPinchGestureRecognizer r)
        {
            var state = r.State;

            var focus = r.LocationInView(_owner).ToPointF()*_scaleFactor;

            if (state == UIGestureRecognizerState.Began)
            {
                _scaleStart = (float) r.Scale/_scaleFactor;
                _previousScale = 1;

                var s = new ScaleEvent(ScaleStatus.Start, 1, focus.X, focus.Y);
                System.Diagnostics.Debug.WriteLine($"Zoom Begin: {s}");
                _gestureSubject.OnNext(s);
            }
            else if (state == UIGestureRecognizerState.Changed)
            {
                var scale = (float) r.Scale/_scaleFactor;
                var diff = 1 - _scaleStart;
                scale += diff;
                var relativeScale = (float)(1 + (scale - _previousScale));

                _previousScale = scale;

                var c = new ScaleEvent(ScaleStatus.Scaling, relativeScale, focus.X, focus.Y);
                System.Diagnostics.Debug.WriteLine($"Zooming: {c}");
                _gestureSubject.OnNext(c);
                
            }
            else if( state == UIGestureRecognizerState.Cancelled ||
                state == UIGestureRecognizerState.Ended ||
                state ==UIGestureRecognizerState.Recognized)
            {
                var scale = (float) r.Scale/_scaleFactor;
                var diff = 1 - _scaleStart;
                scale += diff;
                var relativeScale = (float)(1 + (scale - _previousScale));

                var e = new ScaleEvent(ScaleStatus.End, relativeScale, focus.X, focus.Y);
                System.Diagnostics.Debug.WriteLine($"Zoom End: {e}");
                _gestureSubject.OnNext(e);
            }
        }

        private void OnRotate(UIRotationGestureRecognizer r)
        {
            var state = r.State;

            if (state == UIGestureRecognizerState.Began)
            {
                var rotation = RadianToDegree(r.Rotation);
                var s = new RotateEvent(rotation, rotation, RotateStatus.Start, NumberOfActivePointers);
                System.Diagnostics.Debug.WriteLine($"Rotate Begin: {s} ({NumberOfActivePointers})");
                _gestureSubject.OnNext(s);

                _previousRotation = rotation;
            }
            else if (state == UIGestureRecognizerState.Changed)
            {
                var rotation = RadianToDegree(r.Rotation);
                var s = new RotateEvent(rotation - _previousRotation, rotation, RotateStatus.Rotating, NumberOfActivePointers);
                System.Diagnostics.Debug.WriteLine($"Rotating: {s} ({NumberOfActivePointers})");
                _gestureSubject.OnNext(s);

                _previousRotation = rotation;
            }
            else if (state == UIGestureRecognizerState.Cancelled ||
                state == UIGestureRecognizerState.Ended ||
                state == UIGestureRecognizerState.Recognized)
            {
                var rotation = RadianToDegree(r.Rotation);
                var s = new RotateEvent(rotation - _previousRotation, rotation, RotateStatus.End, NumberOfActivePointers);
                System.Diagnostics.Debug.WriteLine($"Rotate End: {s} ({NumberOfActivePointers})");
                _gestureSubject.OnNext(s);
            }
        }

        private int NumberOfActivePointers => _pointerDownPositions.Count;

        private static float RadianToDegree(double angle)
        {
            return (float)(angle * (180.0 / Math.PI));
        }

        public IObservable<UserInputEvent> UserInputEvents => _gestureSubject.AsObservable();

        internal void OnBegin(UITouch[] events)
        {
            for (int i = events.Length - 1; i >= 0; i--)
            {
                var e = events[i];
                var point = e.LocationInView(_owner);
                
                if (!_owner.Frame.Contains(point))
                    return;
                var pointF = point.ToPointF() * _scaleFactor;

                _pointerDownPositions[e] = pointF;
                _previousPointerPositions[e] = pointF;

                var pe = new PointerEvent(EventType.PointerDown, pointF, pointF, pointF, NumberOfActivePointers);
                _gestureSubject.OnNext(pe);

                if (_pointerDownPositions.Count == 1)
                    System.Diagnostics.Debug.WriteLine($"Down: {pe}  (prev: {_previousPointerPositions[e]} | down: {_pointerDownPositions[e]})");
            }
        }

        internal void OnMove(UITouch[] events)
        {
            for (int i = events.Length - 1; i >= 0; i--)
            {
                var e = events[i];
                var point = e.LocationInView(_owner);

                if (!_owner.Frame.Contains(point))
                    return;
                var pointF = point.ToPointF() * _scaleFactor;
                var delta = (pointF - _previousPointerPositions[e]) / _scaleFactor;

                var pe = new MoveEvent(_pointerDownPositions[e], _previousPointerPositions[e], pointF, delta, NumberOfActivePointers);
                _gestureSubject.OnNext(pe);

                if(_pointerDownPositions.Count == 1)
                    System.Diagnostics.Debug.WriteLine($"Move: {pe}  (prev: {_previousPointerPositions[e]} | down: {_pointerDownPositions[e]})");

                _previousPointerPositions[e] = pointF;
            }
        }

        internal void OnEnd(UITouch[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i];
                var point = e.LocationInView(_owner);

                // we do want to handle the events even if they happened outside of our owner control in case we are currently tracking pointers
                if (!_owner.Frame.Contains(point) && _pointerDownPositions.Count == 0)
                    return;

                var pointF = point.ToPointF() * _scaleFactor;

                var pe = new PointerEvent(EventType.PointerUp, _pointerDownPositions[e], _previousPointerPositions[e], pointF, NumberOfActivePointers);
                _gestureSubject.OnNext(pe);

                if (_pointerDownPositions.Count == 1)
                    System.Diagnostics.Debug.WriteLine($"End: {pe}  (prev: {_previousPointerPositions[e]} | down: {_pointerDownPositions[e]})");
            }

            foreach(var e in events)
            {
                _pointerDownPositions.Remove(e);
                _previousPointerPositions.Remove(e);
            }
        }

        internal void OnCancel(UITouch[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i];
                var point = e.LocationInView(_owner);

                // we do want to handle the events even if they happened outside of our owner control in case we are currently tracking pointers
                if (!_owner.Frame.Contains(point) && _pointerDownPositions.Count == 0)
                    return;
                var pointF = point.ToPointF() * _scaleFactor;

                var pe = new PointerEvent(EventType.Cancel, _pointerDownPositions[e], _previousPointerPositions[e], pointF, NumberOfActivePointers);
                _gestureSubject.OnNext(pe);

                if (_pointerDownPositions.Count == 1)
                    System.Diagnostics.Debug.WriteLine($"Cancel: {pe} (prev: {_previousPointerPositions[e]} | down: {_pointerDownPositions[e]})");
            }

            foreach (var e in events)
            {
                _pointerDownPositions.Remove(e);
                _previousPointerPositions.Remove(e);
            }
        }

        public void Dispose()
        {
            _gestureSubject.Dispose();
        }
    }

    internal static class CGPointExtensions
    {
        public static PointF ToPointF(this CGPoint point)
        {
            return PointF.Create((float)point.X, (float)point.Y);
        }
    }
}

