using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Interactivity;
using CoreGraphics;
using Svg.Editor.Avalon.Views;
using Svg.Editor.Events;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Editor.iOS
{
    /// <summary>
    /// see: https://developer.xamarin.com/guides/ios/application_fundamentals/touch/touch_in_ios/
    /// </summary>
    public class TouchInputEventDetector : IGestureRecognizer, IDisposable
    {
        private readonly SKCanvasView _owner;
        private readonly Subject<UserInputEvent> _gestureSubject = new Subject<UserInputEvent>();
        private readonly PinchGestureRecognizer _pinchGestureRecognizer;

        private Dictionary<int, PointF> _pointerDownPositions = new Dictionary<int, PointF>();
        private Dictionary<int, PointF> _previousPointerPositions = new Dictionary<int, PointF>();

        private float _scaleFactor;
        private float _previousRotation = 0;
        private float _scaleStart;
        private double _previousScale;
        private readonly ZoomGestureRecognizer _pinchGesture;

        public TouchInputEventDetector(SKCanvasView owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            _pinchGesture = new ZoomGestureRecognizer();
            _owner.GestureRecognizers.Add(_pinchGesture);

            // Add pointer event handlers
            _owner.PointerPressed += OnPointerPressed;
            _owner.PointerReleased += OnPointerReleased;
            _owner.PointerMoved += OnPointerMoved;
            _owner.PointerCaptureLost += OnPointerCancelled;
            
            _pinchGesture.OnPointerPressed += OnZoomStart;
            _pinchGesture.OnPointerMoved += OnZoom;
            _pinchGesture.OnPointerReleased += OnZoomEnd;

        }

        private void OnZoom(object? sender, PinchEventArgs e)
        { 
            var s = new ScaleEvent(ScaleStatus.Scaling,(float)e.Scale, (float)e.ScaleOrigin.X, (float)e.ScaleOrigin.Y);
            _gestureSubject.OnNext(s);
            _previousScale =(float)e.Scale;
            SvgEngine.Logger.Warn("Sclaing to " + e.Scale);
        }

        private void OnZoomEnd(object? sender, PointerReleasedEventArgs e)
        {
            if(_previousScale != null){
                var point = e.GetPosition(_owner);
                var x = (float)point.X;
                var y = (float)point.Y;
                var s = new ScaleEvent(ScaleStatus.End, (float)_previousScale, x, y);
                _gestureSubject.OnNext(s);
            }
        }

        private void OnZoomStart(object? sender, PointerPressedEventArgs e)
        {
            _previousScale = 1;
            var point = e.GetPosition(_owner);
            var s = new ScaleEvent(ScaleStatus.Start, 1, (float)point.X, (float)point.Y);
            _gestureSubject.OnNext(s);
        }
        private void OnPinchDelta(object? sender, PinchEventArgs e)
        {

            //    var focus = e.ScaleOrigin
            //    if (_scaleStart == 0)
            //    {
            //        _scaleStart = (float)e.Scale/_scaleFactor;
            //        _previousScale = 1;

            //        var s = new ScaleEvent(ScaleStatus.Start, 1, focus.X, focus.Y);
            //        System.Diagnostics.Debug.WriteLine($"Zoom Begin: {s}");
            //        _gestureSubject.OnNext(s);
            //    }
            //    else if (state == UIGestureRecognizerState.Changed)
            //    {
            //        var scale = (float)r.Scale/_scaleFactor;
            //        var diff = 1 - _scaleStart;
            //        scale += diff;
            //        var relativeScale = (float)(1 + (scale - _previousScale));

            //        _previousScale = scale;

            //        var c = new ScaleEvent(ScaleStatus.Scaling, relativeScale, focus.X, focus.Y);
            //        System.Diagnostics.Debug.WriteLine($"Zooming: {c}");
            //        _gestureSubject.OnNext(c);

            //    }
            //    else if (state == UIGestureRecognizerState.Cancelled ||
            //        state == UIGestureRecognizerState.Ended ||
            //        state ==UIGestureRecognizerState.Recognized)
            //    {
            //        var scale = (float)r.Scale/_scaleFactor;
            //        var diff = 1 - _scaleStart;
            //        scale += diff;
            //        var relativeScale = (float)(1 + (scale - _previousScale));

            //        var e = new ScaleEvent(ScaleStatus.End, relativeScale, focus.X, focus.Y);
            //        System.Diagnostics.Debug.WriteLine($"Zoom End: {e}");
            //        _gestureSubject.OnNext(e);
            //    }
            //    _scaleFactor = (float)e.Scale;
            }

            private void OnPointerPressed(object sender, PointerEventArgs e)
            {
                var point = e.GetPosition(_owner);
                var pointF = PointF.Create((float)point.X * _scaleFactor, (float)point.Y * _scaleFactor);

                // Use pointer ID instead of UITouch
                int pointerId = e.Pointer.Id;

                _pointerDownPositions[pointerId] = pointF;
                _previousPointerPositions[pointerId] = pointF;

                var pe = new PointerEvent(
                    EventType.PointerDown,
                    pointF,
                    pointF,
                    pointF,
                    _pointerDownPositions.Count
                );

                _gestureSubject.OnNext(pe);
                System.Diagnostics.Debug.WriteLine($"Down: {pe}");
            }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(_owner);
            var pointF = PointF.Create((float)point.X * _scaleFactor, (float)point.Y * _scaleFactor);
            int pointerId = e.Pointer.Id;

            // Check if we're tracking this pointer
            if (!_pointerDownPositions.ContainsKey(pointerId))
                return;

            var previousPoint = _previousPointerPositions[pointerId];
            var delta = PointF.Create(
                (pointF.X - previousPoint.X) / _scaleFactor,
                (pointF.Y - previousPoint.Y) / _scaleFactor
            );

            var moveEvent = new MoveEvent(
                _pointerDownPositions[pointerId],
                previousPoint,
                pointF,
                delta,
                _pointerDownPositions.Count
            );

            _gestureSubject.OnNext(moveEvent);
            System.Diagnostics.Debug.WriteLine($"Move: {moveEvent}");

            _previousPointerPositions[pointerId] = pointF;
        }

        private void OnPointerReleased(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(_owner);
            var pointF = PointF.Create((float)point.X * _scaleFactor, (float)point.Y * _scaleFactor);
            int pointerId = e.Pointer.Id;

            if (!_pointerDownPositions.ContainsKey(pointerId))
                return;

            var pointerEvent = new PointerEvent(
                EventType.PointerUp,
                _pointerDownPositions[pointerId],
                _previousPointerPositions[pointerId],
                pointF,
                _pointerDownPositions.Count
            );

            _gestureSubject.OnNext(pointerEvent);
            System.Diagnostics.Debug.WriteLine($"Up: {pointerEvent}");

            // Remove the pointer from tracking
            _pointerDownPositions.Remove(pointerId);
            _previousPointerPositions.Remove(pointerId);
        }

        private void OnPointerCancelled(object sender, PointerCaptureLostEventArgs e)
        {
            //var point = e.GetPosition(_owner);
            //var pointF = PointF.Create(float)point.X * _scaleFactor, (float)point.Y * _scaleFactor);
            int pointerId = e.Pointer.Id;

            //if (!_pointerDownPositions.ContainsKey(pointerId))
            //    return;

            //var pointerEvent = new PointerEvent(
            //    EventType.Cancel,
            //    _pointerDownPositions[pointerId],
            //    _previousPointerPositions[pointerId],
            //    pointF,
            //    _pointerDownPositions.Count
            //);

            //_gestureSubject.OnNext(pointerEvent);
            //System.Diagnostics.Debug.WriteLine($"Cancelled: {pointerEvent}");

            // Remove the pointer from tracking
            _pointerDownPositions.Remove(pointerId);
            _previousPointerPositions.Remove(pointerId);
        }

        // Pinch and Rotation events would require additional handlers in Avalonia
        // These are more complex and depend on specific gesture recognizer implementations

        public IObservable<UserInputEvent> UserInputEvents => _gestureSubject.AsObservable();

        public IObservable<UserGesture> RecognizedGestures => throw new NotImplementedException();

        public void Dispose()
        {
            // Remove handlers
            _owner.PointerPressed -= OnPointerPressed;
            _owner.PointerReleased -= OnPointerReleased;
            _owner.PointerMoved -= OnPointerMoved;
            _owner.PointerCaptureLost -= OnPointerCancelled;

            _pinchGesture.OnPointerPressed -= OnZoomStart;
            _pinchGesture.OnPointerMoved -= OnZoom;
            _pinchGesture.OnPointerReleased -= OnZoomEnd;

            _gestureSubject.Dispose();
        }

        public void OnNext(UserInputEvent e)
        {
        }

        //    private void OnZoom(UIPinchGestureRecognizer r)
        //    {
        //        var state = r.State;

        //        var focus = r.LocationInView(_owner).ToPointF()*_scaleFactor;

        //        if (state == UIGestureRecognizerState.Began)
        //        {
        //            _scaleStart = (float) r.Scale/_scaleFactor;
        //            _previousScale = 1;

        //            var s = new ScaleEvent(ScaleStatus.Start, 1, focus.X, focus.Y);
        //            System.Diagnostics.Debug.WriteLine($"Zoom Begin: {s}");
        //            _gestureSubject.OnNext(s);
        //        }
        //        else if (state == UIGestureRecognizerState.Changed)
        //        {
        //            var scale = (float) r.Scale/_scaleFactor;
        //            var diff = 1 - _scaleStart;
        //            scale += diff;
        //            var relativeScale = (float)(1 + (scale - _previousScale));

        //            _previousScale = scale;

        //            var c = new ScaleEvent(ScaleStatus.Scaling, relativeScale, focus.X, focus.Y);
        //            System.Diagnostics.Debug.WriteLine($"Zooming: {c}");
        //            _gestureSubject.OnNext(c);

        //        }
        //        else if( state == UIGestureRecognizerState.Cancelled ||
        //            state == UIGestureRecognizerState.Ended ||
        //            state ==UIGestureRecognizerState.Recognized)
        //        {
        //            var scale = (float) r.Scale/_scaleFactor;
        //            var diff = 1 - _scaleStart;
        //            scale += diff;
        //            var relativeScale = (float)(1 + (scale - _previousScale));

        //            var e = new ScaleEvent(ScaleStatus.End, relativeScale, focus.X, focus.Y);
        //            System.Diagnostics.Debug.WriteLine($"Zoom End: {e}");
        //            _gestureSubject.OnNext(e);
        //        }
        //    }

        //    private void OnRotate(UIRotationGestureRecognizer r)
        //    {
        //        var state = r.State;

        //        if (state == UIGestureRecognizerState.Began)
        //        {
        //            var rotation = RadianToDegree(r.Rotation);
        //            var s = new RotateEvent(rotation, rotation, RotateStatus.Start, NumberOfActivePointers);
        //            System.Diagnostics.Debug.WriteLine($"Rotate Begin: {s} ({NumberOfActivePointers})");
        //            _gestureSubject.OnNext(s);

        //            _previousRotation = rotation;
        //        }
        //        else if (state == UIGestureRecognizerState.Changed)
        //        {
        //            var rotation = RadianToDegree(r.Rotation);
        //            var s = new RotateEvent(rotation - _previousRotation, rotation, RotateStatus.Rotating, NumberOfActivePointers);
        //            System.Diagnostics.Debug.WriteLine($"Rotating: {s} ({NumberOfActivePointers})");
        //            _gestureSubject.OnNext(s);

        //            _previousRotation = rotation;
        //        }
        //        else if (state == UIGestureRecognizerState.Cancelled ||
        //            state == UIGestureRecognizerState.Ended ||
        //            state == UIGestureRecognizerState.Recognized)
        //        {
        //            var rotation = RadianToDegree(r.Rotation);
        //            var s = new RotateEvent(rotation - _previousRotation, rotation, RotateStatus.End, NumberOfActivePointers);
        //            System.Diagnostics.Debug.WriteLine($"Rotate End: {s} ({NumberOfActivePointers})");
        //            _gestureSubject.OnNext(s);
        //        }
        //    }

        //    private int NumberOfActivePointers => _pointerDownPositions.Count;

        //    private static float RadianToDegree(double angle)
        //    {
        //        return (float)(angle * (180.0 / Math.PI));
        //    }

        //    public IObservable<UserInputEvent> UserInputEvents => _gestureSubject.AsObservable();

        //    internal void OnBegin(UITouch[] events)
        //    {
        //        for (int i = events.Length - 1; i >= 0; i--)
        //        {
        //            var e = events[i];
        //            var point = e.LocationInView(_owner);

        //            if (!_owner.Frame.Contains(point))
        //                return;
        //            var pointF = point.ToPointF() * _scaleFactor;

        //            _pointerDownPositions[e] = pointF;
        //            _previousPointerPositions[e] = pointF;

        //            var pe = new PointerEvent(EventType.PointerDown, pointF, pointF, pointF, NumberOfActivePointers);
        //            _gestureSubject.OnNext(pe);

        //            if (_pointerDownPositions.Count == 1)
        //                System.Diagnostics.Debug.WriteLine($"Down: {pe}  (prev: {_previousPointerPositions[e]} | down: {_pointerDownPositions[e]})");
        //        }
        //    }

        //    internal void OnMove(UITouch[] events)
        //    {
        //        for (int i = events.Length - 1; i >= 0; i--)
        //        {
        //            var e = events[i];
        //            var point = e.LocationInView(_owner);

        //            if (!_owner.Frame.Contains(point))
        //                return;
        //            var pointF = point.ToPointF() * _scaleFactor;
        //            var delta = (pointF - _previousPointerPositions[e]) / _scaleFactor;

        //            var pe = new MoveEvent(_pointerDownPositions[e], _previousPointerPositions[e], pointF, delta, NumberOfActivePointers);
        //            _gestureSubject.OnNext(pe);

        //            if(_pointerDownPositions.Count == 1)
        //                System.Diagnostics.Debug.WriteLine($"Move: {pe}  (prev: {_previousPointerPositions[e]} | down: {_pointerDownPositions[e]})");

        //            _previousPointerPositions[e] = pointF;
        //        }
        //    }

        //    internal void OnEnd(UITouch[] events)
        //    {
        //        for (int i = 0; i < events.Length; i++)
        //        {
        //            var e = events[i];
        //            var point = e.LocationInView(_owner);

        //            // we do want to handle the events even if they happened outside of our owner control in case we are currently tracking pointers
        //            if (!_owner.Frame.Contains(point) && _pointerDownPositions.Count == 0)
        //                return;

        //            var pointF = point.ToPointF() * _scaleFactor;

        //            var pe = new PointerEvent(EventType.PointerUp, _pointerDownPositions[e], _previousPointerPositions[e], pointF, NumberOfActivePointers);
        //            _gestureSubject.OnNext(pe);

        //            if (_pointerDownPositions.Count == 1)
        //                System.Diagnostics.Debug.WriteLine($"End: {pe}  (prev: {_previousPointerPositions[e]} | down: {_pointerDownPositions[e]})");
        //        }

        //        foreach(var e in events)
        //        {
        //            _pointerDownPositions.Remove(e);
        //            _previousPointerPositions.Remove(e);
        //        }
        //    }

        //    internal void OnCancel(UITouch[] events)
        //    {
        //        for (int i = 0; i < events.Length; i++)
        //        {
        //            var e = events[i];
        //            var point = e.LocationInView(_owner);

        //            // we do want to handle the events even if they happened outside of our owner control in case we are currently tracking pointers
        //            if (!_owner.Frame.Contains(point) && _pointerDownPositions.Count == 0)
        //                return;
        //            var pointF = point.ToPointF() * _scaleFactor;

        //            var pe = new PointerEvent(EventType.Cancel, _pointerDownPositions[e], _previousPointerPositions[e], pointF, NumberOfActivePointers);
        //            _gestureSubject.OnNext(pe);

        //            if (_pointerDownPositions.Count == 1)
        //                System.Diagnostics.Debug.WriteLine($"Cancel: {pe} (prev: {_previousPointerPositions[e]} | down: {_pointerDownPositions[e]})");
        //        }

        //        foreach (var e in events)
        //        {
        //            _pointerDownPositions.Remove(e);
        //            _previousPointerPositions.Remove(e);
        //        }
        //    }

        //    public void Dispose()
        //    {
        //        _gestureSubject.Dispose();
        //    }
        //}

        //internal static class CGPointExtensions
        //{
        //    public static PointF ToPointF(this CGPoint point)
        //    {
        //        return PointF.Create((float)point.X, (float)point.Y);
        //    }
        //}
    }
}

