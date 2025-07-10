using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Svg.Interfaces;

namespace Svg.Platform
{
    public class SkiaColors : Colors
    {

        private readonly Dictionary<string, Color> _colorsByName;

        public SkiaColors()
        {
            _colorsByName = this.GetType()
                .GetTypeInfo()
                .DeclaredProperties.Where(p => p.PropertyType == typeof(Color))
                .ToDictionary(p => p.Name.ToLowerInvariant(), p => (Color)p.GetValue(this));
        }

        private static SkiaColor From(Color c)
        {
            return new SkiaColor(c.A, c.R, c.G, c.B);
        }

        public Color Lime { get; } = From(SystemColors.Instance.Lime);
        public Color Black { get; } = From(SystemColors.Instance.Black);
        public Color LimeGreen { get; } = From(SystemColors.Instance.LimeGreen);
        public Color BlanchedAlmond { get; } = From(SystemColors.Instance.BlanchedAlmond);
        public Color Linen { get; } = From(SystemColors.Instance.Linen);
        public Color Blue { get; } = From(SystemColors.Instance.Blue);
        public Color Magenta { get; } = From(SystemColors.Instance.Magenta);
        public Color BlueViolet { get; } = From(SystemColors.Instance.BlueViolet);
        public Color Maroon { get; } = From(SystemColors.Instance.Maroon);
        public Color Brown { get; } = From(SystemColors.Instance.Brown);
        public Color MediumAquamarine { get; } = From(SystemColors.Instance.MediumAquamarine);
        public Color BurlyWood { get; } = From(SystemColors.Instance.BurlyWood);
        public Color MediumBlue { get; } = From(SystemColors.Instance.MediumBlue);
        public Color CadetBlue { get; } = From(SystemColors.Instance.CadetBlue);
        public Color MediumOrchid { get; } = From(SystemColors.Instance.MediumOrchid);
        public Color Chartreuse { get; } = From(SystemColors.Instance.Chartreuse);
        public Color MediumPurple { get; } = From(SystemColors.Instance.MediumPurple);
        public Color Chocolate { get; } = From(SystemColors.Instance.Chocolate);
        public Color MediumSeaGreen { get; } = From(SystemColors.Instance.MediumSeaGreen);
        public Color Coral { get; } = From(SystemColors.Instance.Coral);
        public Color MediumSlateBlue { get; } = From(SystemColors.Instance.MediumSlateBlue);
        public Color CornflowerBlue { get; } = From(SystemColors.Instance.CornflowerBlue);
        public Color MediumSpringGreen { get; } = From(SystemColors.Instance.MediumSpringGreen);
        public Color Cornsilk { get; } = From(SystemColors.Instance.Cornsilk);
        public Color MediumTurquoise { get; } = From(SystemColors.Instance.MediumTurquoise);
        public Color Crimson { get; } = From(SystemColors.Instance.Crimson);
        public Color MediumVioletRed { get; } = From(SystemColors.Instance.MediumVioletRed);
        public Color Cyan { get; } = From(SystemColors.Instance.Cyan);
        public Color MidnightBlue { get; } = From(SystemColors.Instance.MidnightBlue);
        public Color DarkBlue { get; } = From(SystemColors.Instance.DarkBlue);
        public Color MintCream { get; } = From(SystemColors.Instance.MintCream);
        public Color DarkCyan { get; } = From(SystemColors.Instance.DarkCyan);
        public Color MistyRose { get; } = From(SystemColors.Instance.MistyRose);
        public Color DarkGoldenrod { get; } = From(SystemColors.Instance.DarkGoldenrod);
        public Color Moccasin { get; } = From(SystemColors.Instance.Moccasin);
        public Color DarkGray { get; } = From(SystemColors.Instance.DarkGray);
        public Color NavajoWhite { get; } = From(SystemColors.Instance.NavajoWhite);
        public Color DarkGreen { get; } = From(SystemColors.Instance.DarkGreen);
        public Color Navy { get; } = From(SystemColors.Instance.Navy);
        public Color DarkKhaki { get; } = From(SystemColors.Instance.DarkKhaki);
        public Color OldLace { get; } = From(SystemColors.Instance.OldLace);
        public Color DarkMagena { get; } = From(SystemColors.Instance.DarkMagena);
        public Color Olive { get; } = From(SystemColors.Instance.Olive);
        public Color DarkOliveGreen { get; } = From(SystemColors.Instance.DarkOliveGreen);
        public Color OliveDrab { get; } = From(SystemColors.Instance.OliveDrab);
        public Color DarkOrange { get; } = From(SystemColors.Instance.DarkOrange);
        public Color Orange { get; } = From(SystemColors.Instance.Orange);
        public Color DarkOrchid { get; } = From(SystemColors.Instance.DarkOrchid);
        public Color OrangeRed { get; } = From(SystemColors.Instance.OrangeRed);
        public Color DarkRed { get; } = From(SystemColors.Instance.DarkRed);
        public Color Orchid { get; } = From(SystemColors.Instance.Orchid);
        public Color DarkSalmon { get; } = From(SystemColors.Instance.DarkSalmon);
        public Color PaleGoldenrod { get; } = From(SystemColors.Instance.PaleGoldenrod);
        public Color DarkSeaGreen { get; } = From(SystemColors.Instance.DarkSeaGreen);
        public Color PaleGreen { get; } = From(SystemColors.Instance.PaleGreen);
        public Color DarkSlateBlue { get; } = From(SystemColors.Instance.DarkSlateBlue);
        public Color PaleTurquoise { get; } = From(SystemColors.Instance.PaleTurquoise);
        public Color DarkSlateGray { get; } = From(SystemColors.Instance.DarkSlateGray);
        public Color PaleVioletRed { get; } = From(SystemColors.Instance.PaleVioletRed);
        public Color DarkTurquoise { get; } = From(SystemColors.Instance.DarkTurquoise);
        public Color PapayaWhip { get; } = From(SystemColors.Instance.PapayaWhip);
        public Color DarkViolet { get; } = From(SystemColors.Instance.DarkViolet);
        public Color PeachPuff { get; } = From(SystemColors.Instance.PeachPuff);
        public Color DeepPink { get; } = From(SystemColors.Instance.DeepPink);
        public Color Peru { get; } = From(SystemColors.Instance.Peru);
        public Color DeepSkyBlue { get; } = From(SystemColors.Instance.DeepSkyBlue);
        public Color Pink { get; } = From(SystemColors.Instance.Pink);
        public Color DimGray { get; } = From(SystemColors.Instance.DimGray);
        public Color Plum { get; } = From(SystemColors.Instance.Plum);
        public Color DodgerBlue { get; } = From(SystemColors.Instance.DodgerBlue);
        public Color PowderBlue { get; } = From(SystemColors.Instance.PowderBlue);
        public Color Firebrick { get; } = From(SystemColors.Instance.Firebrick);
        public Color Purple { get; } = From(SystemColors.Instance.Purple);
        public Color FloralWhite { get; } = From(SystemColors.Instance.FloralWhite);
        public Color Red { get; } = From(SystemColors.Instance.Red);
        public Color ForestGreen { get; } = From(SystemColors.Instance.ForestGreen);
        public Color RosyBrown { get; } = From(SystemColors.Instance.RosyBrown);
        public Color Fuschia { get; } = From(SystemColors.Instance.Fuschia);
        public Color RoyalBlue { get; } = From(SystemColors.Instance.RoyalBlue);
        public Color Gainsboro { get; } = From(SystemColors.Instance.Gainsboro);
        public Color SaddleBrown { get; } = From(SystemColors.Instance.SaddleBrown);
        public Color GhostWhite { get; } = From(SystemColors.Instance.GhostWhite);
        public Color Salmon { get; } = From(SystemColors.Instance.Salmon);
        public Color Gold { get; } = From(SystemColors.Instance.Gold);
        public Color SandyBrown { get; } = From(SystemColors.Instance.SandyBrown);
        public Color Goldenrod { get; } = From(SystemColors.Instance.Goldenrod);
        public Color SeaGreen { get; } = From(SystemColors.Instance.SeaGreen);
        public Color Gray { get; } = From(SystemColors.Instance.Gray);
        public Color Seashell { get; } = From(SystemColors.Instance.Seashell);
        public Color Green { get; } = From(SystemColors.Instance.Green);
        public Color Sienna { get; } = From(SystemColors.Instance.Sienna);
        public Color GreenYellow { get; } = From(SystemColors.Instance.GreenYellow);
        public Color Silver { get; } = From(SystemColors.Instance.Silver);
        public Color Honeydew { get; } = From(SystemColors.Instance.Honeydew);
        public Color SkyBlue { get; } = From(SystemColors.Instance.SkyBlue);
        public Color HotPink { get; } = From(SystemColors.Instance.HotPink);
        public Color SlateBlue { get; } = From(SystemColors.Instance.SlateBlue);
        public Color IndianRed { get; } = From(SystemColors.Instance.IndianRed);
        public Color SlateGray { get; } = From(SystemColors.Instance.SlateGray);
        public Color Indigo { get; } = From(SystemColors.Instance.Indigo);
        public Color Snow { get; } = From(SystemColors.Instance.Snow);
        public Color Ivory { get; } = From(SystemColors.Instance.Ivory);
        public Color SpringGreen { get; } = From(SystemColors.Instance.SpringGreen);
        public Color Khaki { get; } = From(SystemColors.Instance.Khaki);
        public Color SteelBlue { get; } = From(SystemColors.Instance.SteelBlue);
        public Color Lavender { get; } = From(SystemColors.Instance.Lavender);
        public Color Tan { get; } = From(SystemColors.Instance.Tan);
        public Color LavenderBlush { get; } = From(SystemColors.Instance.LavenderBlush);
        public Color Teal { get; } = From(SystemColors.Instance.Teal);
        public Color LawnGreen { get; } = From(SystemColors.Instance.LawnGreen);
        public Color Thistle { get; } = From(SystemColors.Instance.Thistle);
        public Color LemonChiffon { get; } = From(SystemColors.Instance.LemonChiffon);
        public Color Tomato { get; } = From(SystemColors.Instance.Tomato);
        public Color LightBlue { get; } = From(SystemColors.Instance.LightBlue);
        public Color Turquoise { get; } = From(SystemColors.Instance.Turquoise);
        public Color LightCoral { get; } = From(SystemColors.Instance.LightCoral);
        public Color Violet { get; } = From(SystemColors.Instance.Violet);
        public Color LightCyan { get; } = From(SystemColors.Instance.LightCyan);
        public Color Wheat { get; } = From(SystemColors.Instance.Wheat);
        public Color LightGoldenrodYellow { get; } = From(SystemColors.Instance.LightGoldenrodYellow);
        public Color Transparent { get; } = From(SystemColors.Instance.Transparent); 
        public Color AliceBlue { get; } = From(SystemColors.Instance.AliceBlue);
        public Color LightSalmon { get; } = From(SystemColors.Instance.LightSalmon);
        public Color AntiqueWhite { get; } = From(SystemColors.Instance.AntiqueWhite);
        public Color LightSeaGreen { get; } = From(SystemColors.Instance.LightSeaGreen);
        public Color Aqua { get; } = From(SystemColors.Instance.Aqua);
        public Color LightSkyBlue { get; } = From(SystemColors.Instance.LightSkyBlue);
        public Color Aquamarine { get; } = From(SystemColors.Instance.Aquamarine);
        public Color LightSlateGray { get; } = From(SystemColors.Instance.LightSlateGray);
        public Color Azure { get; } = From(SystemColors.Instance.Azure);
        public Color LightSteelBlue { get; } = From(SystemColors.Instance.LightSteelBlue);
        public Color Beige { get; } = From(SystemColors.Instance.Beige);
        public Color LightYellow { get; } = From(SystemColors.Instance.LightYellow);
        public Color Bisque { get; } = From(SystemColors.Instance.Bisque);
        public Color White { get; } = From(SystemColors.Instance.White);
        public Color LightGreen { get; } = From(SystemColors.Instance.LightGreen);
        public Color WhiteSmoke { get; } = From(SystemColors.Instance.WhiteSmoke);
        public Color LightGray { get; } = From(SystemColors.Instance.LightGray);
        public Color Yellow { get; } = From(SystemColors.Instance.Yellow);
        public Color LightPink { get; } = From(SystemColors.Instance.LightPink);
        public Color YellowGreen { get; } = From(SystemColors.Instance.YellowGreen);

        public Color FromName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            return _colorsByName[name.ToLowerInvariant()];
        }
    }
}
