using System;
using Svg.Converters.Svg;

namespace Svg.Converters
{
    public class SvgPointCollectionConverter : BaseConverter
    {
        public override object ConvertFromString(string value, Type targetType, SvgDocument document)
        {
            var strValue = ((string)value).Trim();
            if (string.Compare(strValue, "none", StringComparison.OrdinalIgnoreCase) == 0) return null;

            var parser = new CoordinateParser(strValue);
            var pointValue = 0.0f;
            var result = new SvgPointCollection();
            while (parser.TryGetFloat(out pointValue))
            {
                result.Add(new SvgUnit(SvgUnitType.User, pointValue));
            }

            return result;
        }
    }
}
