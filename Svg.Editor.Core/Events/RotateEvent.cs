using System;
using System.Diagnostics;

namespace Svg.Editor.Events
{
    public enum RotateStatus
    {
        Start,
        Rotating,
        End
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public class RotateEvent : UserInputEvent
    {

        public RotateEvent(float relativeRotationDegrees, float absoluteRotationDegrees, RotateStatus status, int pointerCount)
        {
            RelativeRotationDegrees = relativeRotationDegrees;
            AbsoluteRotationDegrees = absoluteRotationDegrees;
            Status = status;
            PointerCount = pointerCount;
        }

        public float RelativeRotationDegrees { get; private set; }
        public float AbsoluteRotationDegrees { get; private set; }
        public RotateStatus Status { get; private set; }
        public int PointerCount { get; }

        public override string DebuggerDisplay => $"Rotate '{Enum.GetName(typeof(RotateStatus), Status)}' relative delta {RelativeRotationDegrees}, absolute delta {AbsoluteRotationDegrees}";

        public override string ToString()
        {
            return DebuggerDisplay;
        }
    }
}
