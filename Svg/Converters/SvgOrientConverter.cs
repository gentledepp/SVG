using System;
using Svg.Converters.Svg;

namespace Svg.Converters
{
    public class SvgOrientConverter : BaseConverter
    {
        public override object ConvertFromString(string value, Type targetType, SvgDocument document)
        {
            if (value == null)
            {
                return new SvgUnit(SvgUnitType.User, 0.0f);
            }

            if (!(value is string))
            {
                throw new ArgumentOutOfRangeException("value must be a string.");
            }

            switch (value.ToString())
            {
                case "auto":
                    return (new SvgOrient() { IsAuto = true });
                default:
                    float fTmp = float.MinValue;
                    if (!float.TryParse(value.ToString(), out fTmp))
                        throw new ArgumentOutOfRangeException("value must be a valid float.");
                    return (new SvgOrient(fTmp));
            }
        }
    }
}
