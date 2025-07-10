using System;
using System.Globalization;
using Svg.Converters.Svg;

namespace Svg.Converters
{
    internal class SvgViewBoxConverter : BaseConverter
    {
        public override object ConvertFromString(string value, Type targetType, SvgDocument document)
        {
            string[] coords = ((string)value).Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (coords.Length != 4)
            {
                throw new SvgException("The 'viewBox' attribute must be in the format 'minX, minY, width, height'.");
            }

            return new SvgViewBox(float.Parse(coords[0], NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(coords[1], NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(coords[2], NumberStyles.Float, CultureInfo.InvariantCulture),
                float.Parse(coords[3], NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        public override string ConvertToString(object value)
        {
            var viewBox = (SvgViewBox)value;

            return string.Format("{0}, {1}, {2}, {3}",
                viewBox.MinX.ToString(CultureInfo.InvariantCulture), viewBox.MinY.ToString(CultureInfo.InvariantCulture),
                viewBox.Width.ToString(CultureInfo.InvariantCulture), viewBox.Height.ToString(CultureInfo.InvariantCulture));
        }
    }
}
