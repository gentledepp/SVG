using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using Svg.Interfaces;

namespace Svg
{
    public static class Extensions
    {
        public static IEnumerable<SvgElement> Descendants<T>(this IEnumerable<T> source) where T : SvgElement
        {
            if (source == null) throw new ArgumentNullException("source");
            return GetDescendants<T>(source, false);
        }
        private static IEnumerable<SvgElement> GetAncestors<T>(IEnumerable<T> source, bool self) where T : SvgElement
        {
            foreach (var start in source)
            {
                if (start != null)
                {
                    for (var elem = (self ? start : start.Parent) as SvgElement; elem != null; elem = (elem.Parent as SvgElement))
                    {
                        yield return elem;
                    }
                }
            }
            yield break;
        }
        private static IEnumerable<SvgElement> GetDescendants<T>(IEnumerable<T> source, bool self) where T : SvgElement
        {
            var positons = new Stack<int>();
            int currPos;
            SvgElement currParent;
            foreach (var start in source)
            {
                if (start != null)
                {
                    if (self) yield return start;

                    positons.Push(0);
                    currParent = start;

                    while (positons.Count > 0)
                    {
                        currPos = positons.Pop();
                        if (currPos < currParent.Children.Count)
                        {
                            yield return currParent.Children[currPos];
                            currParent = currParent.Children[currPos];
                            positons.Push(currPos + 1);
                            positons.Push(0);
                        }
                        else
                        {
                            currParent = currParent.Parent;
                        }
                    }
                }
            }
            yield break;
        }
        
        private static ICharConverter _converter = null;

        public static string ConvertFromUtf32(this int value)
        {
            // cache converter for better performance
            if (_converter == null)
            {
                var oldConv = _converter;
                var converter = SvgEngine.Resolve<ICharConverter>();
                Interlocked.CompareExchange(ref _converter, converter, oldConv);
            }

            return _converter.ConvertFromUtf32(value);
        }

        public static int ConvertToUtf32(this string value, int i)
        {
            // cache converter for better performance
            if (_converter == null)
            {
                var oldConv = _converter;
                var converter = SvgEngine.Resolve<ICharConverter>();
                Interlocked.CompareExchange(ref _converter, converter, oldConv);
            }

            return _converter.ConvertToUtf32(value, i);
        }
    }
}