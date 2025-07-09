using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using System;
using Avalonia;
using System.Reflection;
using Avalonia.Interactivity;

namespace Svg.Editor.Avalon.Views.CustomGestureRecognizer;

public class DragGestureRecognizer : GestureRecognizer
{
    private IPointer? _firstContact;
    private IPointer? _secondContact;
    private Point _firstPoint;
    private Point _secondPoint;
    private float _initialScale;
    private float _currentScale;

    public event EventHandler<PointerEventArgs>? Drag;
    protected override void PointerPressed(PointerPressedEventArgs e)
    {
        if (Target is Visual visual && (e.Pointer.Type == PointerType.Touch || e.Pointer.Type == PointerType.Pen))
        {
            SetEventHandled(e);

            if (_firstContact == null)
            {
                _firstContact = e.Pointer;
                _firstPoint = e.GetPosition(visual);
                return;
            }
            else if (_secondContact == null && _firstContact != e.Pointer)
            {
                _secondContact = e.Pointer;
                _secondPoint = e.GetPosition(visual);
            }
            else
            {
                return;
            }

            if (_firstContact != null && _secondContact != null)
            {
                _initialScale = _currentScale;
            }
        }
    }

    protected override void PointerReleased(PointerReleasedEventArgs e)
    {
        SetEventHandled(e);
        RemoveContact(e.Pointer);
    }

    protected override void PointerMoved(PointerEventArgs e)
    {
        if (Target is Visual visual)
        {
            if (_firstContact == e.Pointer)
            {
                _firstPoint = e.GetPosition(visual);
            }
            else if (_secondContact == e.Pointer)
            {
                _secondPoint = e.GetPosition(visual);
            }
            SetEventHandled(e);
            var pointer = e.GetCurrentPoint(visual);
            if ((_firstContact != null && _secondContact != null) || pointer.Properties.IsMiddleButtonPressed)
            {
                Drag?.Invoke(this, e);
            }

        }
    }

    protected override void PointerCaptureLost(IPointer pointer)
    {
        RemoveContact(pointer);
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
    }

    private bool RemoveContact(IPointer pointer)
    {
        if (_firstContact == pointer || _secondContact == pointer)
        {
            if (_secondContact == pointer)
            {
                _secondContact = null;
            }

            if (_firstContact == pointer)
            {
                _firstContact = _secondContact;
                _secondContact = null;
            }

            Target?.RaiseEvent(new PinchEndedEventArgs());
            return true;
        }

        return false;
    }

    private void SetEventHandled(PointerEventArgs e)
    {
        if(Target is Visual visual)
        {
            var pointer = e.GetCurrentPoint(visual);
            if (pointer.Properties.IsMiddleButtonPressed)
            {
                e.Handled = true;
            }
        }
    }

    private static float GetDistance(Point a, Point b)
    {
        var length = b - a;
        return (float)new Vector(length.X, length.Y).Length;
    }
}