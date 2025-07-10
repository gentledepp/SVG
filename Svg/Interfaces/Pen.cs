
using System;

namespace Svg
{
    public abstract class Pen : IDisposable
    {
        public abstract float[] DashPattern { get; set; }
        public abstract float DashOffset { get; set; }
        public abstract LineJoin LineJoin { get; set; }
        public abstract float MiterLimit { get; set; }
        public abstract LineCap StartCap { get; set; }
        public abstract LineCap EndCap { get; set; }
        public abstract float StrokeWidth { get; set; }
        public abstract void Dispose();
    }
}