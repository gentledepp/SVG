using Svg.Interfaces;

namespace Svg.Editor.Gestures
{
    public class LongPressGesture : UserGesture
    {
        public override GestureType Type => GestureType.LongPress;

        public PointF Position { get; }

        public LongPressGesture(PointF pointer1Position)
        {
            Position = pointer1Position;
        }
    }
}