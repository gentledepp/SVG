using ExCSS;
using SkiaSharp;
using Svg.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Svg.SvgTextBase;
using static Svg.SvgVisualElement;

namespace Svg.Platform
{
    public class SkiaTextRenderer : IAlternativeSvgTextRenderer
    {
        public void Render(SvgTextBase txt, ISvgRenderer renderer)
        {
            if (!txt.Visible || !txt.Displayable)
                return;

            bool textIsEmpty = string.IsNullOrEmpty(txt.Text);
            bool hasNoChildren = !txt.Children.OfType<SvgTextBase>().Any();
            
            if (textIsEmpty && hasNoChildren)
                return;

            var cache = txt.GetOrCreateRenderCacheEntry<TextRenderCacheEntry>(renderer);

            if (!textIsEmpty)
            {
                RenderFill(txt, renderer, cache);
                RenderStroke(txt, renderer, cache);
            }
        }
        
        private void RenderStroke(SvgTextBase txt, ISvgRenderer renderer, TextRenderCacheEntry cacheEntry)
        {
            if (txt.Stroke != null)
            {
                if (cacheEntry.StrokeBrush is null)
                {
                    cacheEntry.StrokeBrush = CreateStrokeBrush(txt, renderer);
                    cacheEntry.FontFamily = renderer.GetFontFamily(txt);
                    if (cacheEntry.FontFamily is { } ff)
                        cacheEntry.StrokeBrush.SetFontFamily(ff);
                }
                cacheEntry.StrokePen ??= CreateStrokePen(txt, cacheEntry.StrokeBrush, renderer);
                var pen = (SkiaPen)cacheEntry.StrokePen;

                var (x, y) = GetPosition(txt, renderer);

                DrawLines(txt, renderer, x, y, pen);
            }
        }

        private void RenderFill(SvgTextBase txt, ISvgRenderer renderer, TextRenderCacheEntry cacheEntry)
        {
            if (txt.Fill != null)
            {
                if (cacheEntry.FillBrush is null)
                {
                    cacheEntry.FillBrush = CreateFillBrush(txt, renderer);
                    cacheEntry.FontFamily = renderer.GetFontFamily(txt);
                    if (cacheEntry.FontFamily is { } ff)
                        cacheEntry.FillBrush.SetFontFamily(ff);
                }
                cacheEntry.FillPen ??= CreateFillPen(txt, cacheEntry.FillBrush, renderer);
                var pen = (SkiaPen)cacheEntry.FillPen;

                var (x, y) = GetPosition(txt, renderer);


                DrawLines(txt, renderer, x, y, pen);
            }
        }

        private (float x, float y) GetPosition(SvgTextBase txt, ISvgRenderer renderer)
        {
            float x = 0f;
            float y = 0f;

            if (txt.Parent is SvgText parent && (txt is SvgTextSpan || txt is SvgTextRef))
            {
                // x and y start out at the end of the previous text
                var prevIndex = parent.Children.IndexOf(txt) - 1;
                if (prevIndex >= 0)
                {
                    var predecessor = (SvgTextBase)txt.Parent.Children[prevIndex];
                    var b = predecessor.Bounds;
                    x = b.X + b.Width;
                    y = b.Y + b.Height;
                }
                else
                {
                    if (parent.X.Count > 0)
                        x = parent.X[0].Value;
                    if (parent.Y.Count > 0)
                        y = parent.Y[0].Value;
                }
            }

            if (txt.X.Count > 0)
                x = txt.X[0].Value;

            // note: Dx could contain multiple values, which would stand for each character!
            // see: https://wiki.selfhtml.org/wiki/SVG/Attribute/dx
            if (txt.Dx.Count > 0)
                x += txt.Dx[0].Value;


            if (txt.Y.Count > 0)
                y = txt.Y[0].Value;

            // note: Dx could contain multiple values, which would stand for each character!
            // see: https://wiki.selfhtml.org/wiki/SVG/Attribute/dx
            if (txt.Dy.Count > 0)
                y += txt.Dy[0].Value;
            
            return (x, y);
        }
        
        private Brush CreateStrokeBrush(SvgTextBase txt, ISvgRenderer renderer)
        {
            return txt.Stroke.GetBrush(txt, renderer, 1f);
        }
        
        private Pen CreateStrokePen(SvgTextBase txt, Brush brush, ISvgRenderer renderer)
        {
            var pen = (SkiaPen)SvgEngine.Factory.CreatePen(brush, txt.StrokeWidth.Value);
            
            pen.TextSize = txt.FontSize.Value != 0 ? txt.FontSize.Value : pen.TextSize;
            pen.TextAlign = FromAnchor(txt.TextAnchor);

            byte strokeOpacity = (byte)(pen.Paint.Color.Alpha * txt.StrokeOpacity);
            pen.Paint.Color = new SKColor(pen.Paint.Color.Red, pen.Paint.Color.Green, pen.Paint.Color.Blue,
                strokeOpacity);
            pen.Paint.IsStroke = true;

            return pen;
        }

        private Brush CreateFillBrush(SvgTextBase txt, ISvgRenderer renderer)
        {
            var brush = txt.Fill.GetBrush(txt, renderer, 1f);
            var fontFamily = renderer.GetFontFamily(txt);
            if (fontFamily is { } ff)
                brush.SetFontFamily(ff);
            
            return brush;
        }

        public Dictionary<string, FontFaceRule> CustomFonts { get; set; }

        private Pen CreateFillPen(SvgTextBase txt, Brush brush, ISvgRenderer renderer)
        {

            var pen = (SkiaPen)SvgEngine.Factory.CreatePen(brush, 0f);
            
            pen.TextSize = txt.FontSize.Value != 0 ? txt.FontSize.Value : pen.TextSize;
            pen.TextAlign = FromAnchor(txt.TextAnchor);

            byte fillOpacity = (byte)(pen.Paint.Color.Alpha * txt.FillOpacity);
            pen.Paint.Color = new SKColor(pen.Paint.Color.Red, pen.Paint.Color.Green, pen.Paint.Color.Blue,
                fillOpacity);

            pen.Paint.IsStroke = false;

            return pen;
        }

        private static void DrawLines(SvgTextBase txt, ISvgRenderer renderer, float x, float y, SkiaPen pen)
        {
            renderer.DrawText(txt.ActualText, x, y, pen);
        }

        public RectangleF GetBounds(SvgTextBase txt, ISvgRenderer renderer)
        {
            if (!txt.Visible || !txt.Displayable)
                return RectangleF.Create(0f, 0f, 0f, 0f);

            bool textIsEmpty = string.IsNullOrEmpty(txt.Text);
            bool hasChildren = txt.Children.OfType<SvgTextBase>().Any();

            if (textIsEmpty && !hasChildren)
                return RectangleF.Create(0f, 0f, 0f, 0f);

            RectangleF result;

            if (!textIsEmpty)
            {
                var cacheEntry = txt.GetOrCreateRenderCacheEntry<RenderCacheEntry>(renderer);
                cacheEntry.FillBrush ??= CreateFillBrush(txt, renderer);
                cacheEntry.FillPen ??= CreateFillPen(txt, cacheEntry.FillBrush, renderer);
                var pen = (SkiaPen)cacheEntry.FillPen;

                var (x, y) = GetPosition(txt, renderer);
                var line = txt.ActualText;
                
                var (width,height) = pen.Paint.MeasureTextWithWhiteSpace(line);
                

                SvgTextBase t = txt;
                var anchor = t.TextAnchor;
                while (t != null && anchor == SvgTextAnchor.Inherit)
                {
                    t = t.Parent as SvgTextBase;
                    if (t == null)
                        anchor = SvgTextAnchor.Start;
                    else
                        anchor = t.TextAnchor;
                }

                // textanchor affects boundingbox:
                // if "Middle" (aka Align.Center) then x is the center of the rectangle
                if (anchor == SvgTextAnchor.Middle)
                    x -= width / 2;
                // if "End" (aka Align.Right) then x is at the very right end of the text
                else if (anchor == SvgTextAnchor.End)
                    x -= width;

                // IMPORTANT: In skiasharp y of the text marks the lower left point where the text starts, so we must subtract the height
                y -= height;

                result = RectangleF.Create(x, y, width, height);
            }
            else
                result = RectangleF.Create(0f, 0f, 0f, 0f);
            
            
            if(hasChildren)
            {
                foreach (var child in txt.Children.OfType<SvgTextBase>())
                {
                    var bounds = GetBounds(child, renderer);
                    if (result == null)
                        result = bounds;
                    else
                    {
                        result = result.UnionAndCopy(bounds);
                    }
                }
            }

            return result;
        }
        
        private SKTextAlign FromAnchor(SvgTextAnchor textAnchor)
        {
            switch (textAnchor)
            {
                case SvgTextAnchor.Middle:
                    return SKTextAlign.Center;
                case SvgTextAnchor.End:
                    return SKTextAlign.Right;
                case SvgTextAnchor.Start:
                    return SKTextAlign.Left;
                default:
                    return SKTextAlign.Left;
            }
        }
    }

    public static class SKPaintExtensions
    {
        // Fallback typefaces for characters missing from a primary font, keyed by codepoint
        private static readonly ConcurrentDictionary<int, SKTypeface> _fallbackCache = new();

        internal readonly struct TextRun
        {
            public readonly string Text;
            public readonly SKFont Font;
            public readonly float Width;

            public TextRun(string text, SKFont font, float width)
            {
                Text = text;
                Font = font;
                Width = width;
            }
        }

        private static SKTypeface ResolveFallback(char c)
        {
            return _fallbackCache.GetOrAdd(c,
                cp => SKFontManager.Default.MatchCharacter(cp) ?? SKTypeface.Default);
        }

        /// <summary>
        /// Splits <paramref name="text"/> into runs, using a fallback typeface
        /// for any characters the paint's typeface has no glyph for.
        /// The returned <see cref="TextRun.Font"/> instances are owned by the caller and must be disposed.
        /// </summary>
        internal static List<TextRun> BuildTextRuns(SKPaint paint, string text, out float totalWidth)
        {
            var runs = new List<TextRun>();
            totalWidth = 0f;
            if (string.IsNullOrEmpty(text))
                return runs;

            var primary = paint.Typeface ?? SKTypeface.Default;
            var size = paint.TextSize;

            var i = 0;
            while (i < text.Length)
            {
                var primaryHasGlyph = primary.GetGlyph(text[i]) != 0;
                var start = i;
                while (i < text.Length && (primary.GetGlyph(text[i]) != 0) == primaryHasGlyph)
                    i++;

                var runText = text.Substring(start, i - start);
                var typeface = primaryHasGlyph ? primary : ResolveFallback(text[start]);
                var font = new SKFont(typeface, size);
                // SKFont.MeasureText returns the advance width (includes whitespace), which is the
                // exact amount SKCanvas.DrawText advances the pen for this run.
                var width = font.MeasureText(runText, paint);

                runs.Add(new TextRun(runText, font, width));
                totalWidth += width;
            }

            return runs;
        }

        public static (float width, float height) MeasureTextWithWhiteSpace(this SKPaint paint, string text)
        {
            var runs = BuildTextRuns(paint, text, out var width);

            // Height is the ink extent measured with the primary font. Missing (blank) glyphs
            // contribute no ink, so measuring the whole string with the primary font is enough.
            float height;
            using (var primaryFont = new SKFont(paint.Typeface ?? SKTypeface.Default, paint.TextSize))
            {
                primaryFont.MeasureText(text, out var rect, paint);
                height = rect.Bottom - rect.Top;
            }

            foreach (var run in runs)
                run.Font.Dispose();

            return (width, height);
        }
    }
}