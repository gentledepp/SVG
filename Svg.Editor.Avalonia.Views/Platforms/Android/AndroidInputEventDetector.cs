using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using Svg.Editor.Events;
using Svg.Editor.Interfaces;
using Svg.Interfaces;
using Avalonia.Controls;
using Svg.Editor.Gestures;
using Svg.Editor.Avalon.Views;
using Avalonia.Input.GestureRecognizers;

namespace Svg.Editor.Droid.Services
{
    public class AndroidInputEventDetector : IGestureRecognizer, IDisposable
    {

        public const int InvalidPointerId = -1;
        public int ActivePointerId = InvalidPointerId;
        private float _lastTouchX;
        private float _lastTouchY;

        private float _pointerDownX;
        private float _pointerDownY;
        private int _scaleFactor;
        private object _previousScale;
        private int _scaleStart;
        private readonly Subject<UserInputEvent> _detectedGestures = new Subject<UserInputEvent>();

        private readonly SKCanvasView _owner;
        private readonly ZoomGestureRecognizer _pinchGesture;

        public IObservable<UserInputEvent> UserInputEvents => _detectedGestures.AsObservable();

        public IObservable<UserGesture> RecognizedGestures => throw new NotImplementedException();

        public AndroidInputEventDetector(SKCanvasView owner)
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
            _detectedGestures.OnNext(s);
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
                _detectedGestures.OnNext(s);
            }
        }

        private void OnZoomStart(object? sender, PointerPressedEventArgs e)
        {
            _previousScale = 1;
            var point = e.GetPosition(_owner);
            var s = new ScaleEvent(ScaleStatus.Start, 1, (float)point.X, (float)point.Y);
            _detectedGestures.OnNext(s);
        }

        private void OnPointerPressed(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(_owner);
            var x = (float)point.X;
            var y = (float)point.Y;

            

            var uie = new PointerEvent(
                EventType.PointerDown,
                PointF.Create(_pointerDownX, _pointerDownY),
                PointF.Create(_lastTouchX, _lastTouchY),
                PointF.Create(x, y),
                1 // Avalonia doesn't provide direct pointer count like Android
            );


            _lastTouchX = x;
            _lastTouchY = y;

            _pointerDownX = x;
            _pointerDownY = y;

            ActivePointerId = e.Pointer.Id;

            _detectedGestures.OnNext(uie);
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(_owner);
            var x = (float)point.X;
            var y = (float)point.Y;

            var relativeDeltaX = x - _lastTouchX;
            var relativeDeltaY = y - _lastTouchY;

            var uie = new MoveEvent(
                PointF.Create(_pointerDownX, _pointerDownY),
                PointF.Create(_lastTouchX, _lastTouchY),
                PointF.Create(x, y),
                PointF.Create(relativeDeltaX, relativeDeltaY),
                1 // Avalonia doesn't provide direct pointer count like Android
            );

            _lastTouchX = x;
            _lastTouchY = y;

            _detectedGestures.OnNext(uie);
        }

        private void OnPointerReleased(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(_owner);
            var x = (float)point.X;
            var y = (float)point.Y;

            var uie = new PointerEvent(
                EventType.PointerUp,
                PointF.Create(_pointerDownX, _pointerDownY),
                PointF.Create(_lastTouchX, _lastTouchY),
                PointF.Create(x, y),
                1
            );

            ActivePointerId = InvalidPointerId;

            _detectedGestures.OnNext(uie);
        }

        private void OnPointerCancelled(object sender, PointerCaptureLostEventArgs e)
        {
            //var point = e. Sourc;
            //var x = (float)point.X;
            //var y = (float)point.Y;

            //var uie = new PointerEvent(
            //    EventType.Cancel,
            //    PointF.Create(_pointerDownX, _pointerDownY),
            //    PointF.Create(_lastTouchX, _lastTouchY),
            //    PointF.Create(x, y),
            //    1
            //);

            ActivePointerId = InvalidPointerId;

            //_detectedGestures.OnNext(null);
        }

        public void Reset()
        {
            _lastTouchX = 0;
            _lastTouchY = 0;
            ActivePointerId = InvalidPointerId;
        }

        private class RotationGestureDetector
        {
            private float? _startAngle;
            private float? _previousAngle;

            public event EventHandler<UserInputEvent> OnRotateEvent;

            public void OnPointerEvent(PointerEventArgs[] events)
            {
                if (events.Length < 2) return;

                var point1 = events[0].GetPosition(null);
                var point2 = events[1].GetPosition(null);

                var angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
                angle = (float)(angle * (180.0 / Math.PI));

                if (!_startAngle.HasValue)
                {
                    // Rotation Start
                    _startAngle = angle;
                    OnRotateEvent?.Invoke(this, new RotateEvent(0, 0, RotateStatus.Start, 2));
                }
                else
                {
                    // Rotating
                    var delta = (_previousAngle ?? angle) - angle;
                    var absoluteDelta = (_startAngle.Value - angle) % 360;

                    OnRotateEvent?.Invoke(this, new RotateEvent(
                        delta,
                        absoluteDelta,
                        RotateStatus.Rotating,
                        2
                    ));
                }

                _previousAngle = angle;
            }
        }

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
            
            _detectedGestures?.Dispose();
        }

        public void OnNext(UserInputEvent e)
        {
        }
    }
}