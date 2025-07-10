using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface ISvgElementAttributeProvider
    {
        IEnumerable<SvgElement.PropertyAttributeTuple> GetPropertyAttributes(object instance);
        IEnumerable<SvgElement.EventAttributeTuple> GetEventAttributes(object instance);
    }
}
