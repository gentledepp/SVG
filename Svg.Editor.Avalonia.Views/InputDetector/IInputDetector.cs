using Svg.Editor.Events;
using Svg.Editor.Gestures;
using System;

namespace Svg.Editor.Avalon.Views.InputDetector
{
    public interface IInputDetector : IDisposable
    {
        void OnNext(UserInputEvent e);

        /// <summary>
        /// Observable for recognized gestures.
        /// </summary>
        IObservable<UserGesture> RecognizedGestures { get; }

        public IObservable<UserInputEvent> UserInputEvents { get; }
    }
}