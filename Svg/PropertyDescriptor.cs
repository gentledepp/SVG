using System;
using System.Reflection;
using Svg.Interfaces;

namespace Svg
{
    public class PropertyDescriptor : IPropertyDescriptor
    {
        private readonly PropertyInfo _propertyInfo;

        public PropertyDescriptor(PropertyInfo propertyInfo, ITypeConverter parser, SvgAttributeAttribute attribute)
        {
            if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            if (attribute == null) throw new ArgumentNullException(nameof(attribute));

            _propertyInfo = propertyInfo;
            Converter = parser;
            Attribute = attribute;
        }
        
        public ITypeConverter Converter { get; set; }
        public SvgAttributeAttribute Attribute { get; set; }

        public object GetValue(object instance)
        {
            return _propertyInfo.GetValue(instance);
        }

        public void SetValue(object instance, object value)
        {
            _propertyInfo.SetValue(instance, value);
        }

        public Type PropertyType => _propertyInfo.PropertyType;
    }

    public interface ITypeConverter
    {
        object ConvertFromString(string value, Type targetType, SvgDocument document);
        string ConvertToString(object value);
    }
}
