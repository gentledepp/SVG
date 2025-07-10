using System;
using System.Diagnostics;
using Svg.Interfaces;

namespace Svg.Editor.Events
{
    public enum EventType
    {
        PointerDown,
        Move,
        PointerUp,
        Cancel,
    }

    [DebuggerDisplay("{DebuggerDisplay}")]
    public class PointerEvent : UserInputEvent
    {
        public EventType EventType { get; private set; }
        public PointF Pointer1Down { get; private set; }

        public PointerEvent(EventType eventType, PointF pointer1Down, PointF lastPointer1Position, PointF pointer1Position, int pointerCount)
        {
            EventType = eventType;
            Pointer1Down = pointer1Down;
            LastPointer1DownPosition = lastPointer1Position;
            Pointer1Position = pointer1Position;
            PointerCount = pointerCount;
        }

        public int PointerCount { get; }

        public PointF LastPointer1DownPosition { get; private set; }

        public PointF Pointer1Position { get; private set; }


        public override string DebuggerDisplay => $"Pointer ({Enum.GetName(typeof(EventType), EventType)}) from x:{LastPointer1DownPosition?.X} y:{LastPointer1DownPosition?.Y} to x:{Pointer1Position?.X} y:{Pointer1Position?.Y}";

        public override string ToString()
        {
            return DebuggerDisplay;
        }
    }
}
