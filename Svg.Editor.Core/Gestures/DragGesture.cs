using Svg.Interfaces;

namespace Svg.Editor.Gestures
{
    public enum DragState { Drag, Enter, Exit }

    public class DragGesture : UserGesture
    {
        /// <summary>
        /// Creates a new instance of a drag gesture with state = Exit.
        /// </summary>
        public static DragGesture Exit => new DragGesture(PointF.Empty, PointF.Empty, SizeF.Empty, 0) { State = DragState.Exit };

        /// <summary>
        /// Creates a new instance of a drag gesture with state = Enter.
        /// </summary>
        /// <param name="start">The position where the gesture started from.</param>
        /// <returns></returns>
        public static DragGesture Enter(PointF start)
        {
            return new DragGesture(start, start, SizeF.Empty, 0) { State = DragState.Enter };
        }

        public PointF Position { get; }
        public PointF Start { get; }
        public SizeF Delta { get; }
        public double Dist { get; }

        public override GestureType Type => GestureType.Drag;
        public DragState State { get; private set; }

        public DragGesture(PointF position, PointF start, SizeF delta, double dist)
        {
            Position = position;
            Start = start;
            Delta = delta;
            Dist = dist;
        }
    }
}