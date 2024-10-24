using Avalonia;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using System;

namespace Svg.Editor.Avalon.Views.CustomGestureRecognizer
{
    /// <summary>
    /// When touching screen with three fingers
    /// </summary>
    public class RotatetGestureRecognizer : GestureRecognizer
    {
        private IPointer _firstContact;
        private Point? _firstNow;
        private Point _firstPoint;
        private IPointer _secondContact;
        private Point? _secondNow;
        private Point _secondPoint;
        private IPointer _thirdContact;
        private Point _thirdPoint;
        private float? _startAngle;
        private float _angle = 25; // first rotation
        private float? _previousAngle;
        private float _firstDistance;
        public event EventHandler<PointerEventArgs>? RotateStart;
        public event EventHandler<RotateEventArgs>? Rotate;
        public event EventHandler<RotateEventArgs>? RotateEnd;

        protected override void PointerPressed(PointerPressedEventArgs e)
        {
            if (Target is Visual visual && (e.Pointer.Type == PointerType.Touch || e.Pointer.Type == PointerType.Pen))
            {
                if (_firstContact == null)
                {
                    _firstContact = e.Pointer;
                    _firstPoint = e.GetPosition(visual);
                }
                else if (_secondContact == null && _firstContact != e.Pointer)
                {
                    _secondContact = e.Pointer;
                    _secondPoint = e.GetPosition(visual);
                }
                else if (_thirdContact == null && _firstContact != e.Pointer && _secondContact != e.Pointer)
                {
                    _thirdContact = e.Pointer;
                    _thirdPoint = e.GetPosition(visual);
                    e.PreventGestureRecognition();
                }

                SetEventHandled(e);
            }
        }

        protected override void PointerMoved(PointerEventArgs e)
        {
            if (Target is Visual visual)
            {
                if (_firstContact == null)
                {
                    _firstContact = e.Pointer;
                }
                else if (_secondContact == null && _firstContact != e.Pointer)
                {
                    _secondContact = e.Pointer;
                }
                else if (_thirdContact == null && _firstContact != e.Pointer && _secondContact != e.Pointer)
                {
                    _thirdContact = e.Pointer;
                }

                SetEventHandled(e);

                if (_firstContact != null && _secondContact != null && _thirdContact != null)
                {
                    e.Handled = true;
                    if (_firstNow == null)
                    {
                        _firstNow = e.GetPosition(visual) * 0.9;
                    }
                    else if (_secondNow == null)
                    {
                        _secondNow = e.GetPosition(visual) * 0.9;
                    }

                    if (_secondNow != null && _firstNow != null)
                    {
                        var distance = GetDistance(_firstPoint, _firstNow.Value);
                        if (_startAngle == null)
                        {
                            _startAngle = _angle;
                            RotateStart?.Invoke(this, e);
                            _firstDistance = distance;
                        }
                        else
                        {
                            var change = _firstDistance / distance ;
                            SetAngle(change * _startAngle.Value);
                            if (_previousAngle != null)
                            {
                                var delta = (_previousAngle.Value - _angle) % 360;
                                var absoluteDelta = (_startAngle.Value - _angle) % 360;
                                Rotate?.Invoke(this, new RotateEventArgs(delta, absoluteDelta));
                            }
                        }

                        _previousAngle = _angle;
                        _firstNow = null;
                        _secondNow = null;
                        e.PreventGestureRecognition();
                    }
                }
            }
        }

        private void SetAngle(float angle)
        {
            if (angle >= 25)
            {
                _angle = 90;
            }
            else if(angle >= 115)
            {
                _angle = 180;
            }
            else if(angle >= 190)
            {
                _angle = 250;
            }
            else
            {
                _angle = 0;
            }
        }

        protected override void PointerReleased(PointerReleasedEventArgs e)
        {
            SetEventHandled(e);
            if (RemoveContact(e.Pointer))
            {
                if (_startAngle.HasValue && _previousAngle.HasValue)
                {
                    e.Handled = true;
                    var delta = (_previousAngle.Value - _angle) % 360;
                    var absoluteDelta = (_startAngle.Value - _angle) % 360;
                    RotateEnd?.Invoke(this, new RotateEventArgs(delta, absoluteDelta));
                    e.PreventGestureRecognition();
                }

                _startAngle = null;
                _previousAngle = null;
                _angle = 25;
            }
        }

        protected override void PointerCaptureLost(IPointer pointer)
        {
            RemoveContact(pointer);
        }

        private bool RemoveContact(IPointer pointer)
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

                return true;
            }

            return false;
        }
        private static float GetDistance(Point a, Point b)
        {
            var length = b - a;
            return (float)new Vector(length.X, length.Y).Length;
        }

        private void SetEventHandled(PointerEventArgs e)
        {
            if(_firstContact != null && _secondContact != null && _thirdContact != null)
                e.Handled = true;
        }
    }

    public class RotateEventArgs : EventArgs
    {
        public RotateEventArgs(float delta, float absoluteDelta)
        {
            Delta = delta;
            AbsoluteDelta = absoluteDelta;
        }

        public float Delta { get; private set; }
        public float AbsoluteDelta { get; private set; }
    }
}