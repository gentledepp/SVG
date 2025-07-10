using System;

namespace Svg.Interfaces
{
    public interface IPropertyDescriptor
    {
        ITypeConverter Converter { get; set; }
        object GetValue(object instance);
        void SetValue(object instance, object value);
        Type PropertyType { get; }
    }
}
