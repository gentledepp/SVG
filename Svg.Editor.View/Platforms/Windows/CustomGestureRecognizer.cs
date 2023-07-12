using Microsoft.UI.Input;
using System.Runtime.CompilerServices;

namespace Svg.Editor.View.Platforms.Windows;

public class CustomGestureRecognizer
{
    public event EventHandler<GestureRecognizerEventArgs> OnManipulationStarted;
    public event EventHandler<GestureRecognizerEventArgs> OnManipulationDelta;
    public event EventHandler<GestureRecognizerEventArgs> OnManipulationCompleted;

    public void ProcessStart(global::Windows.Foundation.Point point)
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
    public global::Windows.Foundation.Point Point;
}