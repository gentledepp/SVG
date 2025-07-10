using System;
using Svg.Converters.Svg;

namespace Svg.Converters
{
    public class SvgUnitCollectionConverter : BaseConverter
    {
        private static readonly SvgUnitConverter _unitConverter = new SvgUnitConverter();

        public override object ConvertFromString(string value, Type targetType, SvgDocument document)
        {
            if (string.Compare(((string)value).Trim(), "none", StringComparison.OrdinalIgnoreCase) == 0) return null;
            string[] points = ((string)value).Trim().Split(new char[] { ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            SvgUnitCollection units = new SvgUnitCollection();

            foreach (string point in points)
            {
                SvgUnit newUnit = (SvgUnit)_unitConverter.ConvertFromString(point.Trim(), targetType, document);
                if (!newUnit.IsNone)
                    units.Add(newUnit);
            }

            return units;
        }

        public override string ConvertToString(object value)
        {
            return ((SvgUnitCollection)value).ToString();
        }
    }
}
