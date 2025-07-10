using Svg.Interfaces;

namespace Svg
{
    public interface LinearGradientBrush : Brush
    {
        ColorBlend InterpolationColors { get; set; }
        WrapMode WrapMode { get; set; }
        PointF Start { get; set; }
        PointF End { get; set; }
    }
}