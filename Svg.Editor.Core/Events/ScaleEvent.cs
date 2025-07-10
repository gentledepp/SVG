using System;
using System.Diagnostics;

namespace Svg.Editor.Events
{
    public enum ScaleStatus
    {
        Start,
        Scaling,
        End
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public class ScaleEvent : UserInputEvent
    {
        public ScaleStatus Status { get; private set; }
        public float ScaleFactor { get; private set; }
        public float FocusX { get; private set; }
        public float FocusY { get; private set; }

        public ScaleEvent(ScaleStatus status, float scaleFactor, float focusX, float focusY)
        {
            Status = status;
            ScaleFactor = scaleFactor;
            FocusX = focusX;
            FocusY = focusY;
        }

        /// <summary>
        /// On Windows, we want to be able to change the zoom focus on every scroll wheel event as we are using the mouse
        /// On iOS and Android, we use touch and in order to not make the zoom too "jumpy", we fix the focus point on zoom
        /// </summary>
        public bool ChangeFocus { get; set; }

        public override string DebuggerDisplay => $"Scale ({Enum.GetName(typeof(ScaleStatus), Status)}) {ScaleFactor} at x:{FocusX} y:{FocusY}";

        public override string ToString()
        {
            return DebuggerDisplay;
        }
    }
}
