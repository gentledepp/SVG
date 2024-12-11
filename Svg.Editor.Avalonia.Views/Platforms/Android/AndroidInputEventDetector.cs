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

namespace Svg.Editor.Droid.Services
{
    public class AndroidInputEventDetector : IInputEventDetector, IDisposable
    {

        public const int InvalidPointerId = -1;
        public int ActivePointerId = InvalidPointerId;

        private float _lastTouchX;
        private float _lastTouchY;

        private float _pointerDownX;
        private float _pointerDownY;

        private readonly Subject<UserInputEvent> _detectedGestures = new Subject<UserInputEvent>();
        private readonly Control _owner;

        public IObservable<UserInputEvent> UserInputEvents => _detectedGestures.AsObservable();

        public AndroidInputEventDetector(Control owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            // Add pointer event handlers
            _owner.PointerPressed += OnPointerPressed;
            _owner.PointerReleased += OnPointerReleased;
            _owner.PointerMoved += OnPointerMoved;
            _owner.PointerCaptureLost += OnPointerCancelled;
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
            //var point = e.;
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

            _detectedGestures.OnNext(null);
        }

        public void Reset()
        {
            _lastTouchX = 0;
            _lastTouchY = 0;
            ActivePointerId = InvalidPointerId;
        }

        // Custom Gesture Recognition (Simplified)
        private class ScaleGestureDetector
        {
            private float _startDistance;
            private PointF _focusPoint;

            public event EventHandler<UserInputEvent> OnScaleEvent;

            public void OnPointerEvent(PointerEventArgs[] events)
            {
                if (events.Length < 2) return;

                var point1 = events[0].GetPosition(null);
                var point2 = events[1].GetPosition(null);

                var currentDistance = Distance(point1, point2);
                var focusX = (float)((point1.X + point2.X) / 2);
                var focusY = (float)((point1.Y + point2.Y) / 2);

                if (_startDistance == 0)
                {
                    // Scale Begin
                    _startDistance = currentDistance;
                    _focusPoint = PointF.Create(focusX, focusY);
                    OnScaleEvent?.Invoke(this, new ScaleEvent(ScaleStatus.Start, 1, focusX, focusY));
                }
                else
                {
                    // Scaling
                    var scaleFactor = currentDistance / _startDistance;
                    OnScaleEvent?.Invoke(this, new ScaleEvent(ScaleStatus.Scaling, scaleFactor, focusX, focusY));
                }
            }

            private float Distance(Point p1, Point p2)
            {
                return (float)Math.Sqrt(
                    Math.Pow(p1.X - p2.X, 2) +
                    Math.Pow(p1.Y - p2.Y, 2)
                );
            }
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

            _detectedGestures?.Dispose();
        }
    }
}