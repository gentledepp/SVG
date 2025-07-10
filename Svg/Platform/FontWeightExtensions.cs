using SkiaSharp;

namespace Svg.Platform;

internal static class FontWeightExtensions
{
    internal static SKFontStyleWeight ToSkFontStyleWeight(this SvgFontWeight style)
    {
        return style switch
        {
            SvgFontWeight.Normal => SKFontStyleWeight.Normal,
            SvgFontWeight.Lighter => SKFontStyleWeight.Light,
            SvgFontWeight.Bold => SKFontStyleWeight.Bold,
            SvgFontWeight.Bolder => SKFontStyleWeight.ExtraBold,
            SvgFontWeight.W600 => SKFontStyleWeight.Bold,
            SvgFontWeight.W800 => SKFontStyleWeight.ExtraBold,
            SvgFontWeight.W900 => SKFontStyleWeight.ExtraBold,
            _ => SKFontStyleWeight.Normal
        };
    }
    internal static SKFontStyleSlant ToSkFontStyleSlant(this SvgFontStyle style)
    {
        return style switch
        {
            SvgFontStyle.Normal => SKFontStyleSlant.Upright,
            SvgFontStyle.Italic => SKFontStyleSlant.Italic,
            SvgFontStyle.Oblique => SKFontStyleSlant.Oblique,
            _ => SKFontStyleSlant.Upright
        };
    }

    
    internal static SKFontStyle ToSkFontStyle(this SvgFontStyle style)
    {
        return style switch
        {
            SvgFontStyle.Normal => SKFontStyle.Normal,
            SvgFontStyle.Italic => SKFontStyle.Italic,
            _ => SKFontStyle.Normal
        };
    }
}