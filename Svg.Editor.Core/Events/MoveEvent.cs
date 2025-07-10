using System.Diagnostics;
using Svg.Interfaces;

namespace Svg.Editor.Events
{
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class MoveEvent : PointerEvent
    {
        private PointF _absoluteDelta;

        public PointF RelativeDelta { get; private set; }

        public PointF AbsoluteDelta
        {
            get
            {
                if (_absoluteDelta == null)
                {
                    _absoluteDelta = PointF.Create(Pointer1Position.X - Pointer1Down.X, Pointer1Position.Y - Pointer1Down.Y);
                }
                return _absoluteDelta;
            }
        }

        public MoveEvent(PointF pointer1Down, PointF lastPointer1Position, PointF pointer1Position, PointF relativeDelta, int pointerCount)
            : base(EventType.Move, pointer1Down, lastPointer1Position, pointer1Position, pointerCount)
        {
            RelativeDelta = relativeDelta;
        }

        public override string DebuggerDisplay => $"Move from x:{LastPointer1DownPosition?.X} y:{LastPointer1DownPosition?.Y} to x:{Pointer1Position?.X} y:{Pointer1Position?.Y} (pointer down x:{Pointer1Down.X} y:{Pointer1Down.Y}";
    }
}
