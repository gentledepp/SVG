using System;
using System.Collections.Generic;
using System.Text;
using Svg.Interfaces;

namespace Svg.Shared.Interfaces
{
    public interface RadialGradientBrush : Brush
    {
        ColorBlend InterpolationColors { get; set; }
        PointF Center { get; set; }
        float Radius { get; set; }
        WrapMode WrapMode { get; set; }
    }
}
