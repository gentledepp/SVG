using System;
using Svg.Editor.Events;
using Svg.Editor.Gestures;

namespace Svg.Editor.Interfaces
{
    public interface IGestureRecognizer
    {
        void OnNext(UserInputEvent e);

        /// <summary>
        /// Observable for recognized gestures.
        /// </summary>
        IObservable<UserGesture> RecognizedGestures { get; }
    }
}