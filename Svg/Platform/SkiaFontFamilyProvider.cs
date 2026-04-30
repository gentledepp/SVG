using System.Collections.Generic;
using SkiaSharp;

namespace Svg.Platform
{
    //http://stackoverflow.com/questions/3532397/how-to-retrieve-a-list-of-available-installed-fonts-in-android
    public class SkiaFontFamilyProvider : FontFamilyProvider
    {
        public IEnumerable<FontFamily> Families
        {
            get
            {
                return new List<FontFamily>()
                {
                    new SkiaFontFamily(SKTypeface.FromFamilyName(string.Empty, SKFontStyle.Normal), "Default"), GenericSerif, GenericSansSerif, GenericMonospace,
                };
            }
        }

        public FontFamily GenericSerif { get { return new SkiaFontFamily(SKTypeface.FromFamilyName("Serif"), "Serif"); } }
        public FontFamily GenericSansSerif { get { return new SkiaFontFamily(SKTypeface.FromFamilyName("SansSerif"), "SansSerif"); } }
        public FontFamily GenericMonospace { get { return new SkiaFontFamily(SKTypeface.FromFamilyName("Monospace"), "Monospace"); } }
        public StringFormat GenericTypographic { get { return SvgEngine.Factory.CreateStringFormatGenericTypographic(); } }
    }
}
