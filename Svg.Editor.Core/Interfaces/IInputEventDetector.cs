using System;
using Svg.Editor.Events;

namespace Svg.Editor.Interfaces
{
    /// <summary>
    /// This interface provides an observable for detected "crude" gestures, which are called <see cref="UserInputEvent"/>.<p/>
    /// There are various sorts of these events, including:
    /// <list type="bullet">
    /// <item><see cref="PointerEvent"/></item>
    /// <item><see cref="MoveEvent"/></item>
    /// <item><see cref="RotateEvent"/></item>
    /// <item><see cref="ScaleEvent"/></item>
    /// </list>
    /// </summary>
    public interface IInputEventDetector
    {
        IObservable<UserInputEvent> UserInputEvents { get; }
    }
}