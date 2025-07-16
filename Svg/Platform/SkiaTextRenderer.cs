using ExCSS;
using SkiaSharp;
using Svg.Interfaces;
using System;
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

                if (pen == null)
                    return;

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

                if (pen == null)
                    return;

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
                    if (parent.X?.Count > 0)
                        x = parent.X[0].Value;
                    if (parent.Y?.Count > 0)
                        y = parent.Y[0].Value;
                }
            }

            if (txt.X?.Count > 0)
                x = txt.X[0].Value;

            // note: Dx could contain multiple values, which would stand for each character!
            // see: https://wiki.selfhtml.org/wiki/SVG/Attribute/dx
            if (txt.Dx?.Count > 0)
                x += txt.Dx[0].Value;


            if (txt.Y?.Count > 0)
                y = txt.Y[0].Value;

            // note: Dy could contain multiple values, which would stand for each character!
            // see: https://wiki.selfhtml.org/wiki/SVG/Attribute/dy
            if (txt.Dy?.Count > 0)
                y += txt.Dy[0].Value;
            
            return (x, y);
        }
        
        private Brush CreateStrokeBrush(SvgTextBase txt, ISvgRenderer renderer)
        {
            return txt.Stroke.GetBrush(txt, renderer, 1f);
        }
        
        private Pen CreateStrokePen(SvgTextBase txt, Brush brush, ISvgRenderer renderer)
        {
            if (brush == null)
                return null;

            var pen = (SkiaPen)SvgEngine.Factory.CreatePen(brush, txt.StrokeWidth.Value);
            if (pen == null)
                return null;
            
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
            if (brush != null)
            {
                var fontFamily = renderer.GetFontFamily(txt);
                if (fontFamily is { } ff)
                    brush.SetFontFamily(ff);
            }
            
            return brush;
        }

        public Dictionary<string, IFontFaceRule> CustomFonts { get; set; }

        private Pen CreateFillPen(SvgTextBase txt, Brush brush, ISvgRenderer renderer)
        {
            if (brush == null)
                return null;

            var pen = (SkiaPen)SvgEngine.Factory.CreatePen(brush, 0f);
            if (pen == null)
                return null;
            
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

                if (pen == null)
                    return RectangleF.Create(0f, 0f, 0f, 0f);

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
        /// <summary>
        /// Skia ignores spaces when measuring text
        /// So we need to solve it manually.
        /// See here: https://github.com/mono/SkiaSharp/issues/605
        /// </summary>
        /// <param name="paint"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static (float width, float height) MeasureTextWithWhiteSpace(this SKPaint paint, string text)
        {
            SKRect rect = new SKRect();
            
            var wrapper = ".";
            var wrapperWidth = paint.MeasureText(wrapper);
            var textWidth = paint.MeasureText(wrapper + text + wrapper, ref rect);
            textWidth = textWidth - (wrapperWidth + wrapperWidth);

            var width = textWidth;
            var height = rect.Bottom - rect.Top;
            
            return (width, height);
        }
    }
}