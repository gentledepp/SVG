using System;
using System.Globalization;
using Svg.DataTypes;


namespace Svg.Converters
{
    namespace Svg
    {
        //just overrrides canconvert and returns true
        public class BaseConverter : ITypeConverter
        {
            public virtual object ConvertFromString(string value, Type targetType, SvgDocument document)
            {
                if (targetType == typeof(string))
                    return value;

                if (targetType == typeof(bool))
                {
                    if(value is { } v && bool.TryParse(v, out var bo))
                        return bo;
                    return false;
                }
                if (targetType == typeof(short))
                {
                    if (value is { } v && short.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bo))
                        return bo;
                    return default(short);
                }
                if (targetType == typeof(int))
                {
                    if (value is { } v && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bo))
                        return bo;
                    return default(int);
                }
                if (targetType == typeof(long))
                {
                    if (value is { } v && long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var bo))
                        return bo;
                    return default(long);
                }
                if (targetType == typeof(double))
                {
                    if (value is { } v && double.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var bo))
                        return bo;
                    return default(double);
                }
                if (targetType == typeof(float))
                {
                    if (value is { } v && float.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var bo))
                        return bo;
                    return default(float);
                }
                if (targetType == typeof(Uri))
                    return new Uri(value, UriKind.RelativeOrAbsolute);
                if (targetType == typeof(Guid))
                    return new Guid(value);

                throw new NotSupportedException($"No ITypeConverter registered for targetType {targetType} - cannot parse value '{value}'");
            }

            public virtual string ConvertToString(object value)
            {
                return value?.ToString();
            }
        }

        public class XmlSpaceHandlingConverter : BaseConverter
        {
            public override object ConvertFromString(string value, Type targetType, SvgDocument document)
            {
                if (string.IsNullOrEmpty(value))
                    return XmlSpaceHandling.@default;
                if (string.Equals("default", value, StringComparison.CurrentCultureIgnoreCase))
                    return XmlSpaceHandling.@default;
                if (string.Equals("preserve", value, StringComparison.CurrentCultureIgnoreCase))
                    return XmlSpaceHandling.preserve;
                
                throw new NotSupportedException($"svg attribute had wrong value: '{value}'");
            }

            public override string ConvertToString(object value)
            {
                var e = value == null ? XmlSpaceHandling.@default : (XmlSpaceHandling)value;

                if (e == XmlSpaceHandling.@default)
                    return "default";
                if (e == XmlSpaceHandling.preserve)
                    return "preserve";
                throw new NotSupportedException($"svg attribute had wrong value: '{value}'");
            }
        }

        public sealed class SvgBoolConverter : BaseConverter
        {
            public override object ConvertFromString(string value, Type targetType, SvgDocument document)
            {
                if (value == null)
                {
                    return true;
                }

                if (!(value is string))
                {
                    throw new ArgumentOutOfRangeException("value must be a string.");
                }

                // Note: currently only used by SvgVisualElement.Visible but if
                // conversion is used elsewhere these checks below will need to change
                string visibility = (string)value;
                if ((visibility == "hidden") || (visibility == "collapse"))
                    return false;
                else
                    return true;
            }

            public override string ConvertToString(object value)
            {
                return ((bool)value) ? "visible" : "hidden";
            }
        }

        //converts enums to lower case strings
        public class EnumBaseConverter<T> : BaseConverter
            where T : struct
        {
            /// <summary>If specified, upon conversion, the default value will result in 'null'.</summary>
            public T? DefaultValue { get; protected set; }

            /// <summary>Creates a new instance.</summary>
            public EnumBaseConverter() { }

            /// <summary>Creates a new instance.</summary>
            /// <param name="defaultValue">Specified the default value of the enum.</param>
            public EnumBaseConverter(T defaultValue)
            {
                this.DefaultValue = defaultValue;
            }

            /// <summary>Attempts to convert the provided value to <typeparamref name="T"/>.</summary>
            public override object ConvertFromString(string value, Type targetType, SvgDocument document)
            {
                if (value == null)
                {
                    if (this.DefaultValue.HasValue)
                        return this.DefaultValue.Value;

                    return Activator.CreateInstance(typeof(T));
                }
                
                return (T)Enum.Parse(typeof(T), (string)value, true);
            }

            /// <summary>Attempts to convert the value to the destination type.</summary>
            public override string ConvertToString(object value)
            {
                //If the value id the default value, no need to write the attribute.
                if (this.DefaultValue.HasValue && Enum.Equals(value, this.DefaultValue.Value))
                    return null;
                else
                {
                    //SVG attributes should be camelCase.
                    string stringValue = ((T)value).ToString();

                    stringValue = string.Format("{0}{1}", stringValue[0].ToString().ToLower(), stringValue.Substring(1));

                    return stringValue;
                }
            }
        }

        public sealed class SvgFillRuleConverter : EnumBaseConverter<SvgFillRule>
        {
            public SvgFillRuleConverter() : base(SvgFillRule.NonZero) { }
        }

        public sealed class SvgColourInterpolationConverter : EnumBaseConverter<SvgColourInterpolation>
        {
            public SvgColourInterpolationConverter() : base(SvgColourInterpolation.SRGB) { }
        }

        public sealed class SvgClipRuleConverter : EnumBaseConverter<SvgClipRule>
        {
            public SvgClipRuleConverter() : base(SvgClipRule.NonZero) { }
        }

        public sealed class SvgTextAnchorConverter : EnumBaseConverter<SvgTextAnchor>
        {
            public SvgTextAnchorConverter() : base(SvgTextAnchor.Start) { }
        }

        public sealed class SvgStrokeLineCapConverter : EnumBaseConverter<SvgStrokeLineCap>
        {
            public SvgStrokeLineCapConverter() : base(SvgStrokeLineCap.Butt) { }
        }

        public sealed class SvgStrokeLineJoinConverter : EnumBaseConverter<SvgStrokeLineJoin>
        {
            public SvgStrokeLineJoinConverter() : base(SvgStrokeLineJoin.Miter) { }
        }

        public sealed class SvgMarkerUnitsConverter : EnumBaseConverter<SvgMarkerUnits>
        {
            public SvgMarkerUnitsConverter() : base(SvgMarkerUnits.StrokeWidth) { }
        }

        public sealed class SvgFontStyleConverter : EnumBaseConverter<SvgFontStyle>
        {
            public SvgFontStyleConverter() : base(SvgFontStyle.All) { }
        }

        public sealed class SvgOverflowConverter : EnumBaseConverter<SvgOverflow>
        {
            public SvgOverflowConverter() : base(SvgOverflow.Auto) { }
        }

        public sealed class SvgTextLengthAdjustConverter : EnumBaseConverter<SvgTextLengthAdjust>
        {
            public SvgTextLengthAdjustConverter() : base(SvgTextLengthAdjust.Spacing) { }
        }

        public sealed class SvgTextPathMethodConverter : EnumBaseConverter<SvgTextPathMethod>
        {
            public SvgTextPathMethodConverter() : base(SvgTextPathMethod.Align) { }
        }

        public sealed class SvgTextPathSpacingConverter : EnumBaseConverter<SvgTextPathSpacing>
        {
            public SvgTextPathSpacingConverter() : base(SvgTextPathSpacing.Exact) { }
        }

        public sealed class SvgFontVariantConverter : EnumBaseConverter<SvgFontVariant>
        {
            public SvgFontVariantConverter() : base(SvgFontVariant.Normal) { }

            public override object ConvertFromString(string value, Type targetType, SvgDocument document)
            {
                if (value == "none")
                    return SvgFontVariant.Normal;

                if (value == "small-caps")
                    return SvgFontVariant.Smallcaps;

                return base.ConvertFromString(value, targetType, document);
            }

            public override string ConvertToString(object value)
            {
                if (value is SvgFontVariant && ((SvgFontVariant)value == SvgFontVariant.Smallcaps))
                {
                    return "small-caps";
                }

                return base.ConvertToString(value);
            }
        }

        public sealed class SvgCoordinateUnitsConverter : EnumBaseConverter<SvgCoordinateUnits>
        {
            //TODO Inherit is not actually valid. See TODO on SvgCoordinateUnits enum.
            public SvgCoordinateUnitsConverter() : base(SvgCoordinateUnits.Inherit) { }
        }

        public sealed class SvgGradientSpreadMethodConverter : EnumBaseConverter<SvgGradientSpreadMethod>
        {
            public SvgGradientSpreadMethodConverter() : base(SvgGradientSpreadMethod.Pad) { }
        }
        public sealed class SvgVisibleConverter : EnumBaseConverter<SvgVisible>
        {
            public SvgVisibleConverter() : base(SvgVisible.Hidden) { }
        }

        public sealed class SvgTextDecorationConverter : EnumBaseConverter<SvgTextDecoration>
        {
            public SvgTextDecorationConverter() : base(SvgTextDecoration.None) { }

            public override object ConvertFromString(string value, Type targetType, SvgDocument document)
            {
                if (value == "line-through")
                    return SvgTextDecoration.LineThrough;

                return base.ConvertFromString(value, targetType, document);
            }

            public override string ConvertToString(object value)
            {
                if (value is SvgTextDecoration && (SvgTextDecoration)value == SvgTextDecoration.LineThrough)
                {
                    return "line-through";
                }

                return base.ConvertToString(value);
            }
        }

        public sealed class SvgFontWeightConverter : EnumBaseConverter<SvgFontWeight>
        {
            //TODO Defaulting to Normal although it should be All if this is used on a font face.
            public SvgFontWeightConverter() : base(SvgFontWeight.Normal) { }

            public override object ConvertFromString(string value, Type targetType, SvgDocument document)
            {
                if (value is string)
                {
                    switch ((string)value)
                    {
                        case "100": return SvgFontWeight.W100;
                        case "200": return SvgFontWeight.W200;
                        case "300": return SvgFontWeight.W300;
                        case "400": return SvgFontWeight.W400;
                        case "500": return SvgFontWeight.W500;
                        case "600": return SvgFontWeight.W600;
                        case "700": return SvgFontWeight.W700;
                        case "800": return SvgFontWeight.W800;
                        case "900": return SvgFontWeight.W900;
                    }
                }
                return base.ConvertFromString(value, targetType, document);
            }
            public override string ConvertToString(object value)
            {
                if (value is SvgFontWeight)
                {
                    switch ((SvgFontWeight)value)
                    {
                        case SvgFontWeight.W100: return "100";
                        case SvgFontWeight.W200: return "200";
                        case SvgFontWeight.W300: return "300";
                        case SvgFontWeight.W400: return "400";
                        case SvgFontWeight.W500: return "500";
                        case SvgFontWeight.W600: return "600";
                        case SvgFontWeight.W700: return "700";
                        case SvgFontWeight.W800: return "800";
                        case SvgFontWeight.W900: return "900";
                    }
                }
                return base.ConvertToString(value);
            }
        }

        public static class Enums
        {
            public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct
            {
                var retValue = value == null ?
                            false :
                            Enum.IsDefined(typeof(TEnum), value);
                result = retValue ?
                            (TEnum)Enum.Parse(typeof(TEnum), value) :
                            default(TEnum);
                return retValue;
            }
        }
    }

}
