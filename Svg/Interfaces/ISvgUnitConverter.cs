using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface ISvgUnitConverter
    {
        object ConvertFromInvariantString(string text);
    }
}

