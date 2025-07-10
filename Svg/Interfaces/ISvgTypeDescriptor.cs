using System;
using System.Collections.Generic;
using System.Reflection;

namespace Svg.Interfaces
{
    public interface ISvgTypeDescriptor
    {
        IEnumerable<Attribute> GetAttributes(object obj);
        IEnumerable<EventInfo> GetEvents(object obj);
        IEnumerable<PropertyInfo> GetProperties(object obj);
        ITypeConverter GetConverter(Type type);
    }
}
