
using System;
using Avalonia;
using Avalonia.Input;

namespace Svg.Editor.Avalon.Views.Platforms.Windows;

public class CustomGestureRecognizer
{
    public event EventHandler<GestureRecognizerEventArgs> OnManipulationStarted;
    public event EventHandler<GestureRecognizerEventArgs> OnManipulationDelta;
    public event EventHandler<GestureRecognizerEventArgs> OnManipulationCompleted;

    public void ProcessStart(Point point)
    {
        OnManipulationStarted?.Invoke(this, new GestureRecognizerEventArgs() { Point = point });
    }

    public void ProcessUp(PointerPoint point)
    {
        OnManipulationCompleted?.Invoke(this, new GestureRecognizerEventArgs()
        {
            Point = point.Position,
        });
    }
    public void ProcessMove(PointerPoint point)
    {
        OnManipulationDelta?.Invoke(this, new GestureRecognizerEventArgs() { Point = point.Position });
    }
}

public sealed class GestureRecognizerEventArgs : EventArgs
{
    public Point Point;
}