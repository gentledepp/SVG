using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svg.Converters.Svg;
using Svg.Interfaces;

namespace Svg
{
    public class SvgElementAttributeProvider : ISvgElementAttributeProvider
    {
        private static readonly ConcurrentDictionary<Type,IEnumerable<SvgElement.PropertyAttributeTuple>> PropertyCache = new ConcurrentDictionary<Type, IEnumerable<SvgElement.PropertyAttributeTuple>>();
        private static readonly ConcurrentDictionary<Type, IEnumerable<SvgElement.EventAttributeTuple>> EventCache = new ConcurrentDictionary<Type, IEnumerable<SvgElement.EventAttributeTuple>>();

        public IEnumerable<SvgElement.PropertyAttributeTuple> GetPropertyAttributes(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            IEnumerable<SvgElement.PropertyAttributeTuple> attrs;
            if (PropertyCache.TryGetValue(instance.GetType(), out attrs))
            {
                return attrs;
            }

            attrs  = (from PropertyDescriptor a in TypeDescriptor.GetProperties(instance?.GetType())
            select new SvgElement.PropertyAttributeTuple { Property = a, Attribute = a.Attribute }).ToArray();

            var visibilityAttribute = attrs.SingleOrDefault(a => a.Attribute.Name.ToLower() == "visibility");
            if(visibilityAttribute != null)
                visibilityAttribute.Property.Converter = new SvgBoolConverter();

            PropertyCache.AddOrUpdate(instance.GetType(), attrs, (key, oldValue) => attrs);

            return attrs;
        }

        public IEnumerable<SvgElement.EventAttributeTuple> GetEventAttributes(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            IEnumerable<SvgElement.EventAttributeTuple> attrs;
            if (EventCache.TryGetValue(instance.GetType(), out attrs))
            {
                return attrs;
            }
            //attrs = from EventInfo a in TypeDescriptor.GetEvents(instance?.GetType())
            //       select new SvgElement.EventAttributeTuple { Event = a..ComponentType.GetField(a.Name, BindingFlags.Instance | BindingFlags.NonPublic), Attribute = attribute };
            //throw new NotImplementedException("not implemented yet!");

            //EventCache.AddOrUpdate(instance.GetType(), attrs, (key, oldValue) => attrs);

            //return attrs;

            return Enumerable.Empty<SvgElement.EventAttributeTuple>();
        }
    }

    internal static class TypeDescriptor
    {
        internal static List<IPropertyDescriptor> GetProperties(Type elementType, string attributeName = null)
        {
            var result = new Dictionary<string, IPropertyDescriptor>();

            var ti = elementType.GetTypeInfo();
            var parent = ti;
            while (parent != null)
            {
                List<PropertyInfo> properties;

                if (string.IsNullOrEmpty(attributeName))
                {
                    properties = parent.DeclaredProperties.Where(
                            p =>
                                p.GetCustomAttributes()
                                    .OfType<SvgAttributeAttribute>()
                                    .Any())
                        .ToList();
                }
                else
                {
                    properties = parent.DeclaredProperties.Where(
                            p =>
                                p.GetCustomAttributes()
                                    .OfType<SvgAttributeAttribute>()
                                    .Any(a => string.IsNullOrEmpty(attributeName) || a.Name == attributeName))
                        .ToList();
                }


                foreach (var p in properties)
                {
                    var converter = SvgEngine.TypeConverterRegistry.Get(p.PropertyType);
                    var attribute = p.GetCustomAttributes().OfType<SvgAttributeAttribute>().Single();
                    var key = attribute.NamespaceAndName;
                    // make sure an overridden property is not added twice here!
                    if (!result.ContainsKey(key))
                    {
                        result.Add(key, new PropertyDescriptor(p, converter, attribute));
                    }
                }

                parent = parent.BaseType?.GetTypeInfo();
            }

            return result.Values.ToList();
        }
        
        internal static IEnumerable<EventInfo> GetEvents(Type type)
        {
            if(type == null)
                yield break;
            var ti = type.GetTypeInfo();
            var parent = ti;
            while (parent != null)
            {
                var events =
                    parent.DeclaredEvents.Where(
                            p => p.GetCustomAttributes().OfType<SvgAttributeAttribute>().Any())
                        .ToList();

                foreach (var p in events)
                {
                    yield return p;
                }

                parent = parent.BaseType?.GetTypeInfo();
            }
        }
    }
}