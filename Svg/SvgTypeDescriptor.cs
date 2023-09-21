using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;

namespace Svg.Shared
{
    internal class SvgTypeDescriptor : ISvgTypeDescriptor
    {
        private readonly ISvgTypeConverterRegistry _registry;
        private readonly ConcurrentDictionary<Type, IEnumerable<Attribute>> _attributeCache = new();
        private readonly ConcurrentDictionary<Type, IEnumerable<PropertyInfo>> _propertyCache = new();
        private readonly ConcurrentDictionary<Type, IEnumerable<EventInfo>> _eventCache = new();

        private static readonly IEnumerable<Attribute> NullAttributes = Enumerable.Empty<Attribute>();
        private static readonly IEnumerable<EventInfo> NullEvents = Enumerable.Empty<EventInfo>();
        private static readonly IEnumerable<PropertyInfo> NullProperties = Enumerable.Empty<PropertyInfo>();

        public SvgTypeDescriptor(ISvgTypeConverterRegistry registry)
        {
            _registry = registry;
        }

        public IEnumerable<Attribute> GetAttributes(object obj)
        {
            if (obj == null)
                return NullAttributes;

            var k = obj.GetType();
            if (!_attributeCache.TryGetValue(k, out var attributes))
            {
                attributes = k.GetTypeInfo().GetCustomAttributes<Attribute>().ToArray();
                _attributeCache.TryAdd(k, attributes);
            }

            return attributes;
        }

        public IEnumerable<EventInfo> GetEvents(object obj)
        {
            if (obj == null)
                return NullEvents;

            var k = obj.GetType();
            if (!_eventCache.TryGetValue(k, out var events))
            {
                events = k.GetTypeInfo().DeclaredEvents.ToArray();
                _eventCache.TryAdd(k, events);
            }

            return events;
        }

        public IEnumerable<PropertyInfo> GetProperties(object obj)
        {
            if (obj == null)
                return NullProperties;


            var k = obj.GetType();
            if (!_propertyCache.TryGetValue(k, out var properties))
            {
                properties = k.GetTypeInfo().DeclaredProperties.ToArray();
                _propertyCache.TryAdd(k, properties);
            }

            return properties;
        }

        public ITypeConverter GetConverter(Type type)
        {
            return _registry.Get(type);
        }
    }
}
