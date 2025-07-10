using System;
using System.Globalization;
using Svg.Converters.Svg;
using Svg.Interfaces;

namespace Svg.Converters
{
    internal class SvgColourConverter : BaseConverter
    {
        public override object ConvertFromString(string value, Type targetType, SvgDocument document)
        {
            var oldCulture = CultureInfo.DefaultThreadCurrentCulture;
            try
            {
                CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

                var colour = value.Trim();

                if (colour.StartsWith("rgb"))
                {
                    try
                    {
                        int start = colour.IndexOf("(") + 1;

                        //get the values from the RGB string
                        string[] values = colour.Substring(start, colour.IndexOf(")") - start).Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        //determine the alpha value if this is an RGBA (it will be the 4th value if there is one)
                        int alphaValue = 255;
                        if (values.Length > 3)
                        {
                            //the alpha portion of the rgba is not an int 0-255 it is a decimal between 0 and 1
                            //so we have to determine the corosponding byte value
                            var alphastring = values[3];
                            if (alphastring.StartsWith("."))
                            {
                                alphastring = "0" + alphastring;
                            }

                            var alphaDecimal = decimal.Parse(alphastring);

                            if (alphaDecimal <= 1)
                            {
                                alphaValue = (int)(alphaDecimal * 255);
                            }
                            else
                            {
                                alphaValue = (int)alphaDecimal;
                            }
                        }

                        Color colorpart;
                        if (values[0].Trim().EndsWith("%"))
                        {
                            colorpart = SvgEngine.Factory.CreateColorFromArgb(alphaValue, (int)(255 * float.Parse(values[0].Trim().TrimEnd('%')) / 100f),
                                                                                  (int)(255 * float.Parse(values[1].Trim().TrimEnd('%')) / 100f),
                                                                                  (int)(255 * float.Parse(values[2].Trim().TrimEnd('%')) / 100f));
                        }
                        else
                        {
                            colorpart = SvgEngine.Factory.CreateColorFromArgb(alphaValue, int.Parse(values[0]), int.Parse(values[1]), int.Parse(values[2]));
                        }

                        return colorpart;
                    }
                    catch
                    {
                        throw new SvgException("Colour is in an invalid format: '" + colour + "'");
                    }
                }
                else if (colour.StartsWith("#"))
                {
                    //colour = string.Format("#{0}{0}{1}{1}{2}{2}", colour[1], colour[2], colour[3]);
                    return SvgEngine.Factory.CreateColorFromHexString(colour);
                }

                if (!colour.StartsWith("#"))
                {
                    // skip animation values
                    if (string.Equals(colour, "freeze", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(colour, "remove", StringComparison.OrdinalIgnoreCase))
                    {
                        return SvgEngine.Factory.Colors.Black;
                    }

                    return SvgEngine.Factory.Colors.FromName(colour.ToLowerInvariant());
                }
            }
            finally
            {
                CultureInfo.DefaultThreadCurrentCulture = oldCulture;
            }

            return SvgEngine.Factory.Colors.Black;
        }
    }
}
