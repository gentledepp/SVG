using System;
using System.Collections.Generic;
using System.Reflection;

namespace Svg
{
    /// <summary>
    /// A collection of Scalable Vector Attributes that can be inherited from the owner elements ancestors.
    /// </summary>
    public sealed class SvgAttributeCollection : Dictionary<string, object>
    {
        private SvgElement _owner;

        /// <summary>
        /// Initialises a new instance of a <see cref="SvgAttributeCollection"/> with the given <see cref="SvgElement"/> as the owner.
        /// </summary>
        /// <param name="owner">The <see cref="SvgElement"/> owner of the collection.</param>
        public SvgAttributeCollection(SvgElement owner)
        {
            this._owner = owner;
        }

        /// <summary>
        /// Gets the attribute with the specified name.
        /// </summary>
        /// <typeparam name="TAttributeType">The type of the attribute value.</typeparam>
        /// <param name="attributeName">A <see cref="string"/> containing the name of the attribute.</param>
        /// <returns>The attribute value if available; otherwise the default value of <typeparamref name="TAttributeType"/>.</returns>
        public Attribute<TAttributeType> GetAttribute<TAttributeType>(string attributeName)
        {
            return this.GetAttribute<TAttributeType>(attributeName, default(TAttributeType));
        }

        /// <summary>
        /// Gets the attribute with the specified name.
        /// </summary>
        /// <typeparam name="TAttributeType">The type of the attribute value.</typeparam>
        /// <param name="attributeName">A <see cref="string"/> containing the name of the attribute.</param>
        /// <param name="defaultValue">The value to return if a value hasn't already been specified.</param>
        /// <returns>The attribute value if available; otherwise the default value of <typeparamref name="T"/>.</returns>
        public Attribute<TAttributeType> GetAttribute<TAttributeType>(string attributeName, TAttributeType defaultValue)
            => new Attribute<TAttributeType>(attributeName, defaultValue, this);

        private TAttributeType GetAttributeValue<TAttributeType>(string attributeName, TAttributeType defaultValue)
        {
            if (this.TryGetValue(attributeName, out var value) && value != null)
            {
                return (TAttributeType)value;
            }

            return defaultValue;
        }

        public InheritedAttribute<TAttributeType> GetInheritedAttribute<TAttributeType>(string attributeName) =>
            new(attributeName, this);

        /// <summary>
        /// Gets the attribute with the specified name and inherits from ancestors if there is no attribute set.
        /// </summary>
        /// <typeparam name="TAttributeType">The type of the attribute value.</typeparam>
        /// <param name="attributeName">A <see cref="string"/> containing the name of the attribute.</param>
        /// <returns>The attribute value if available; otherwise the ancestors value for the same attribute; otherwise the default value of <typeparamref name="TAttributeType"/>.</returns>
        private TAttributeType GetInheritedAttributeValue<TAttributeType>(string attributeName)
        {
            if (base.TryGetValue(attributeName, out var result) && !IsInheritValue(result))
            {
                var deferred = result as SvgDeferredPaintServer;
                deferred?.EnsureServer(_owner);
                return (TAttributeType)result;
            }

            if (_owner.Parent != null)
            {
                return _owner.Parent.Attributes.GetInheritedAttributeValue<TAttributeType>(attributeName);
            }

            return default(TAttributeType);
        }

        private bool IsInheritValue(object value)
        {
            return value == null ||
                   value is SvgFontWeight && (SvgFontWeight)value == SvgFontWeight.Inherit ||
                   value is SvgTextAnchor && (SvgTextAnchor)value == SvgTextAnchor.Inherit ||
                   value is SvgFontVariant && (SvgFontVariant)value == SvgFontVariant.Inherit || 
                   value is SvgTextDecoration && (SvgTextDecoration)value == SvgTextDecoration.Inherit ||
                   //value is XmlSpaceHandling && (XmlSpaceHandling)value == XmlSpaceHandling.inherit ||
                   value is SvgOverflow && (SvgOverflow)value == SvgOverflow.Inherit ||
                   value == SvgColourServer.Inherit ||
                   value == SvgColourServer.NotSet ||
                   value == SvgUnitCollection.Inherit ||
                   value is string && (string)value == "inherit";
        }

        /// <summary>
        /// Gets the attribute with the specified name.
        /// </summary>
        /// <param name="attributeName">A <see cref="string"/> containing the attribute name.</param>
        /// <returns>The attribute value associated with the specified name; If there is no attribute the parent's value will be inherited.</returns>
        public new object this[string attributeName]
        {
            get { return this.GetInheritedAttributeValue<object>(attributeName); }
            set
            {
                if (base.TryGetValue(attributeName, out var oldVal))
                {
                    if (TryUnboxedCheck(oldVal, value))
                    {
                        base[attributeName] = value;
                        OnAttributeChanged(attributeName, value, oldVal);
                    }
                }
                else
                {
                    base[attributeName] = value;
                    OnAttributeChanged(attributeName, value, null);
                }
            }
        }

        private bool TryUnboxedCheck(object a, object b)
        {
            if (IsValueType(a))
            {
                if (a is SvgUnit)
                    return UnboxAndCheck<SvgUnit>(a, b);
                else if (a is bool)
                    return UnboxAndCheck<bool>(a, b);
                else if (a is int)
                    return UnboxAndCheck<int>(a, b);
                else if (a is float)
                    return UnboxAndCheck<float>(a, b);
                else if (a is SvgViewBox)
                    return UnboxAndCheck<SvgViewBox>(a, b);
                else
                    return true;
            }
            else
            {
                return a != b;
            }
        }

        private bool UnboxAndCheck<T>(object a, object b)
        {
            return !((T)a).Equals((T)b);
        }

        private bool IsValueType(object obj)
        {
            return obj != null && obj.GetType().GetTypeInfo().IsValueType;
        }

        /// <summary>
        /// Fired when an Atrribute has changed
        /// </summary>
        public event EventHandler<AttributeEventArgs> AttributeChanged;

        private void OnAttributeChanged(string attribute, object value, object oldValue)
        {
            var handler = AttributeChanged;
            if (handler != null)
            {
                handler(this._owner, new AttributeEventArgs(attribute, value, oldValue));
            }
        }

        /// <summary>
        /// To avoid unnecessary recursive dictionary.get[string key] calls when rendering, this class allows to lazily load and cache an inheritable attribute and resets it when appropriate
        /// </summary>
        /// <typeparam name="TAttributeType"></typeparam>
        public sealed class InheritedAttribute<TAttributeType>
        {
            private readonly SvgAttributeCollection _owner;
            private readonly string _attributeName;
            private TAttributeType _value;
            private bool _initialized;

            public InheritedAttribute(string attributeName, SvgAttributeCollection owner)
            {
                _owner = owner;
                _attributeName = attributeName;
                _owner._owner.ParentAttributeChanged += (_, __) => Reset();
                _owner.AttributeChanged += (_, args) =>
                {
                    if (args.Attribute == attributeName)
                        Reset();
                };

            }

            public TAttributeType GetValue()
            {
                if (_initialized)
                    return _value;

                _value = _owner.GetInheritedAttributeValue<TAttributeType>(_attributeName);
                _initialized = true;
                return _value;
            }

            public void Reset()
            {
                _value = default(TAttributeType);
                _initialized = false;
            }
        }

        /// <summary>
        /// To avoid unnecessary dictionary.get[string key] calls when rendering, this class allows to lazily load and cache a non-inheritable attribute and resets it when appropriate
        /// </summary>
        public sealed class Attribute<TAttributeType>
        {
            private readonly SvgAttributeCollection _owner;
            private readonly string _attributeName;
            private readonly TAttributeType _defaultValue;
            private TAttributeType _value;
            private bool _initialized;

            public Attribute(string attributeName, TAttributeType defaultValue, SvgAttributeCollection owner)
            {
                _owner = owner;
                _attributeName = attributeName;
                _defaultValue = defaultValue;
                _owner.AttributeChanged += (_, args) =>
                {
                    if (args.Attribute == attributeName)
                        Reset();
                };

            }

            public TAttributeType GetValue()
            {
                if (_initialized)
                    return _value;

                _value = _owner.GetAttributeValue<TAttributeType>(_attributeName, _defaultValue);
                _initialized = true;
                return _value;
            }

            public void Reset()
            {
                _value = default(TAttributeType);
                _initialized = false;
            }
        }
    }


    /// <summary>
    /// A collection of Custom Attributes
    /// </summary>
    public sealed class SvgCustomAttributeCollection : Dictionary<string, string>
    {
        private SvgElement _owner;

        /// <summary>
        /// Initialises a new instance of a <see cref="SvgAttributeCollection"/> with the given <see cref="SvgElement"/> as the owner.
        /// </summary>
        /// <param name="owner">The <see cref="SvgElement"/> owner of the collection.</param>
        public SvgCustomAttributeCollection(SvgElement owner)
        {
            this._owner = owner;
        }

        /// <summary>
        /// Gets the attribute with the specified name.
        /// </summary>
        /// <param name="attributeName">A <see cref="string"/> containing the attribute name.</param>
        /// <returns>The attribute value associated with the specified name; If there is no attribute the parent's value will be inherited.</returns>
        public new string this[string attributeName]
        {
            get { return base[attributeName]; }
            set
            {
                if (base.TryGetValue(attributeName, out var oldVal))
                {
                    base[attributeName] = value;
                    if (oldVal != value) OnAttributeChanged(attributeName, value, oldVal);
                }
                else
                {
                    base[attributeName] = value;
                    OnAttributeChanged(attributeName, value, null);
                }
            }
        }

        /// <summary>
        /// Fired when an Atrribute has changed
        /// </summary>
        public event EventHandler<AttributeEventArgs> AttributeChanged;

        private void OnAttributeChanged(string attribute, object value, object oldValue)
        {
            var handler = AttributeChanged;
            if (handler != null)
            {
                handler(this._owner, new AttributeEventArgs(attribute, value, oldValue));
            }
        }
    }
}