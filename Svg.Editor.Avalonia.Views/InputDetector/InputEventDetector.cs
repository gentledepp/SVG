using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Input;
using Svg.Editor.Events;
using Svg.Interfaces;
using Svg.Editor.Gestures;
using Svg.Editor.Avalon.Views.CustomGestureRecognizer;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Avalon.Views.InputDetector
{
    public class InputEventDetector : IInputDetector, IGestureRecognizer, IDisposable
    {
        private const float MaxMouseWheelStep = 12;
        public const int InvalidPointerId = -1;
        private float _lastTouchX;
        private float _lastTouchY;
        private float _pointerDownX;
        private float _pointerDownY;
        private object _previousScale;
        private IPointer _firstContact;
        private IPointer _secondContact;
        private IPointer _thirdContact;
        private readonly Subject<UserInputEvent> _detectedGestures = new Subject<UserInputEvent>();
        private readonly Subject<UserGesture> _gesturesSubject = new Subject<UserGesture>();
        private readonly SKCanvasView _owner;
        private readonly ZoomGestureRecognizer _pinchGesture;
        private readonly RotatetGestureRecognizer _rotateGesture;
        public IObservable<UserInputEvent> UserInputEvents => _detectedGestures.AsObservable();
        public IObservable<UserGesture> RecognizedGestures => _gesturesSubject.AsObservable();

        public InputEventDetector(SKCanvasView owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _pinchGesture = new ZoomGestureRecognizer();
            _rotateGesture = new RotatetGestureRecognizer();
            _owner.GestureRecognizers.Add(_rotateGesture);
            _owner.GestureRecognizers.Add(_pinchGesture);

            // Add pointer event handlers
            _owner.PointerPressed += OnPointerPressed;
            _owner.PointerReleased += OnPointerReleased;
            _owner.PointerMoved += OnPointerMoved;
            _owner.PointerCaptureLost += OnPointerCancelled;
            
            _pinchGesture.ZoomStart += OnZoomStart;
            _pinchGesture.Zoom += OnZoom;
            _pinchGesture.ZoomEnd += OnZoomEnd;
            
            _rotateGesture.Rotate += OnRotate;
            _rotateGesture.RotateEnd += OnRotateEnd;
            _rotateGesture.RotateStart += OnRotateStart;
            
            // UWP Gestures
            _owner.Tapped += ElementOnTapped;
            _owner.DoubleTapped += ElementOnDoubleTapped;
            _owner.PointerWheelChanged += ElementOnPointerWheelChanged;

        }

        private void ElementOnPointerWheelChanged(object sender, PointerWheelEventArgs args)
        {
            var pointerPoint = args.GetCurrentPoint(_owner);
            var wheelDelta = (float)args.Delta.Y;
            _detectedGestures.OnNext(new ScaleEvent(ScaleStatus.Scaling, 1 + wheelDelta / MaxMouseWheelStep,
                (float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) { ChangeFocus = true });
        }

        private void ElementOnDoubleTapped(object sender, TappedEventArgs args)
        {
            var position = args.GetPosition(_owner);
            _gesturesSubject.OnNext(new DoubleTapGesture(PointF.Create((float)position.X, (float)position.Y)));
        }

        private void ElementOnTapped(object sender, TappedEventArgs args)
        {
            var position = args.GetPosition(_owner);
            _gesturesSubject.OnNext(new TapGesture(PointF.Create((float)position.X, (float)position.Y)));
        }

        private void OnRotateEnd(object? sender, RotateEventArgs e)
        {
            var s = new RotateEvent(e.Delta, e.AbsoluteDelta, RotateStatus.End, 3);
            _detectedGestures.OnNext(s);
        }

        private void OnRotate(object? sender, RotateEventArgs e)
        {
            var s = new RotateEvent(e.Delta, e.AbsoluteDelta, RotateStatus.Rotating, 3);
            _detectedGestures.OnNext(s);
        }

        private void OnRotateStart(object? sender, PointerEventArgs e)
        {
            var s = new RotateEvent(0, 0, RotateStatus.Start, 3);
            _detectedGestures.OnNext(s);
        }

        private void OnZoom(object? sender, PinchEventArgs e)
        {
            if (_thirdContact != null) return;
            var s = new ScaleEvent(ScaleStatus.Scaling, (float)e.Scale, (float)e.ScaleOrigin.X, (float)e.ScaleOrigin.Y);
            _detectedGestures.OnNext(s);
            _previousScale = (float)e.Scale;
        }

        private void OnZoomEnd(object? sender, PointerReleasedEventArgs e)
        {
            if (_previousScale != null)
            {
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
            var uie = new PointerEvent(EventType.PointerDown, PointF.Create(_pointerDownX, _pointerDownY),
                PointF.Create(_lastTouchX, _lastTouchY), PointF.Create(x, y),
                1 // Avalonia doesn't provide direct pointer count like Android
            );
            _lastTouchX = x;
            _lastTouchY = y;
            _pointerDownX = x;
            _pointerDownY = y;
            RegisterContact(e.Pointer);
            _detectedGestures.OnNext(uie);
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(_owner);
            var x = (float)point.X;
            var y = (float)point.Y;
            var relativeDeltaX = x - _lastTouchX;
            var relativeDeltaY = y - _lastTouchY;
            if (_thirdContact != null) return;
            var uie = new MoveEvent(PointF.Create(_pointerDownX, _pointerDownY),
                PointF.Create(_lastTouchX, _lastTouchY), PointF.Create(x, y),
                PointF.Create(relativeDeltaX, relativeDeltaY),
                1 // Avalonia doesn't provide direct pointer count like Android
            );
            var pointer = e.GetCurrentPoint(_owner);
            if ((_firstContact != null && _secondContact != null) || pointer.Properties.IsMiddleButtonPressed)
            {
                uie = new MoveEvent(PointF.Create(_pointerDownX, _pointerDownY),
                    PointF.Create(_lastTouchX, _lastTouchY), PointF.Create(x, y),
                    PointF.Create(relativeDeltaX, relativeDeltaY),
                    2 // Avalonia doesn't provide direct pointer count like Android
                );
            }

            _lastTouchX = x;
            _lastTouchY = y;
            _detectedGestures.OnNext(uie);
        }

        private void OnPointerReleased(object sender, PointerEventArgs e)
        {
            var point = e.GetPosition(_owner);
            var x = (float)point.X;
            var y = (float)point.Y;
            var uie = new PointerEvent(EventType.PointerUp, PointF.Create(_pointerDownX, _pointerDownY),
                PointF.Create(_lastTouchX, _lastTouchY), PointF.Create(x, y), 1);
            RemoveContact(e.Pointer);
            _detectedGestures.OnNext(uie);
        }

        private void OnPointerCancelled(object sender, PointerCaptureLostEventArgs e)
        {
            var uie = new PointerEvent(EventType.Cancel, PointF.Create(_pointerDownX, _pointerDownY),
                PointF.Create(_lastTouchX, _lastTouchY), PointF.Create(_lastTouchX, _lastTouchY), 1);
            _detectedGestures.OnNext(uie);
        }

        public void Reset()
        {
            _lastTouchX = 0;
            _lastTouchY = 0;
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
                    OnRotateEvent?.Invoke(this, new RotateEvent(delta, absoluteDelta, RotateStatus.Rotating, 2));
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
            _owner.Tapped -= ElementOnTapped;
            _owner.DoubleTapped -= ElementOnDoubleTapped;
            _owner.PointerWheelChanged -= ElementOnPointerWheelChanged;
            _pinchGesture.ZoomStart -= OnZoomStart;
            _pinchGesture.Zoom -= OnZoom;
            _pinchGesture.ZoomEnd -= OnZoomEnd;
            _rotateGesture.Rotate -= OnRotate;
            _rotateGesture.RotateEnd -= OnRotateEnd;
            _rotateGesture.RotateStart -= OnRotateStart;
            _detectedGestures?.Dispose();
        }

        private void RegisterContact(IPointer pointer)
        {
            if (_firstContact == null)
            {
                _firstContact = pointer;
            }
            else if (_secondContact == null && _firstContact != pointer)
            {
                _secondContact = pointer;
            }
            else if (_thirdContact == null && _secondContact != null && _firstContact != pointer)
            {
                _thirdContact = pointer;
            }
        }

        private void RemoveContact(IPointer pointer)
        {
            if (_firstContact == pointer || _secondContact == pointer || _thirdContact == pointer)
            {
                if (_thirdContact == pointer)
                {
                    _thirdContact = null;
                }

                if (_secondContact == pointer)
                {
                    _secondContact = _thirdContact;
                    _thirdContact = null;
                }

                if (_firstContact == pointer)
                {
                    _firstContact = _secondContact;
                    _secondContact = null;
                }
            }
        }

        public void OnNext(UserInputEvent e)
        {
        }
    }
}