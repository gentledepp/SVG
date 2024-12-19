using Svg.Editor.Events;
using Svg.Editor.Gestures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
