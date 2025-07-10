using System;
using System.Collections.Generic;
using Svg.Converters;
using Svg.Converters.Svg;
using Svg.DataTypes;
using Svg.FilterEffects;
using Svg.Pathing;
using Svg.Transforms;

namespace Svg
{
    internal class SvgTypeConverterRegistry : ISvgTypeConverterRegistry
    {
        private static readonly Dictionary<Type, ITypeConverter> _converters = new Dictionary<Type, ITypeConverter>
        {
            {typeof(SvgClipRule), new SvgClipRuleConverter()},
            {typeof(SvgAspectRatio), new SvgPreserveAspectRatioConverter()},
            {typeof(XmlSpaceHandling), new XmlSpaceHandlingConverter()},
            {typeof(SvgPreserveAspectRatio), new SvgPreserveAspectRatioConverter()},
            {typeof(SvgColourInterpolation), new SvgColourInterpolationConverter()},
            {typeof(SvgCoordinateUnits), new SvgCoordinateUnitsConverter()},
            {typeof(SvgFontStyle), new SvgFontStyleConverter()},
            {typeof(SvgFontVariant), new SvgFontVariantConverter()},
            {typeof(SvgFontWeight), new SvgFontWeightConverter()},
            {typeof(SvgMarkerUnits), new SvgMarkerUnitsConverter()},
            {typeof(SvgOrient), new SvgOrientConverter()},
            {typeof(SvgOverflow), new SvgOverflowConverter()},
            {typeof(SvgPointCollection), new SvgPointCollectionConverter()},
            {typeof(SvgTextDecoration), new SvgTextDecorationConverter()},
            {typeof(SvgTextLengthAdjust), new SvgTextLengthAdjustConverter()},
            {typeof(SvgTextPathMethod), new SvgTextPathMethodConverter()},
            {typeof(SvgTextPathSpacing), new SvgTextPathSpacingConverter()},
            {typeof(SvgUnit), new SvgUnitConverter()},
            {typeof(SvgUnitCollection), new SvgUnitCollectionConverter()},
            {typeof(SvgViewBox), new SvgViewBoxConverter()},
            {typeof(SvgColourMatrixType), new EnumBaseConverter<SvgColourMatrixType>()},
            {typeof(SvgFillRule), new SvgFillRuleConverter()},
            {typeof(SvgGradientSpreadMethod), new SvgGradientSpreadMethodConverter()},
            {typeof(SvgPaintServer), new SvgPaintServerFactory()},
            {typeof(SvgStrokeLineCap), new SvgStrokeLineCapConverter()},
            {typeof(SvgStrokeLineJoin), new SvgStrokeLineJoinConverter()},
            {typeof(SvgPathSegmentList), new SvgPathBuilder()},
            {typeof(SvgTextAnchor), new SvgTextAnchorConverter()},
            {typeof(SvgTransformCollection), new SvgTransformConverter()},
            {typeof(SvgVisible), new SvgVisibleConverter()},
            {typeof(string), new BaseConverter()},
            {typeof(bool), new BaseConverter()},
            {typeof(short), new BaseConverter()},
            {typeof(int), new BaseConverter()},
            {typeof(long), new BaseConverter()},
            {typeof(double), new BaseConverter()},
            {typeof(float), new BaseConverter()},
            {typeof(Uri), new BaseConverter()},
            {typeof(Guid), new BaseConverter()},
        };

        public void Register<TType>(ITypeConverter parser)
        {
            var t = typeof(TType);
            Register(t, parser);
        }

        public void Register(Type type, ITypeConverter parser)
        {
            if (!_converters.ContainsKey(type))
            {
                _converters.Add(type, parser);
            }
        }

        public ITypeConverter Get(Type key)
        {
            return _converters[key];
        }
    }

    public interface ISvgTypeConverterRegistry
    {
        void Register<TType>(ITypeConverter parser);
        void Register(Type type, ITypeConverter parser);
        ITypeConverter Get(Type key);
    }
}
