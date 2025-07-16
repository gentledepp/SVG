using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzler;
using ExCSS;

namespace Svg.Css
{
    internal static class CssQuery
    {
        public static IEnumerable<SvgElement> QuerySelectorAll(this SvgElement elem, string selector)
        {
            var generator = new SelectorGenerator<SvgElement>(new SvgElementOps());
            Fizzler.Parser.Parse(selector, generator);
            return generator.Selector(Enumerable.Repeat(elem, 1));
        }

        public static int GetSpecificity(this ISelector selector)
        {
            if (selector == null) return 0;
            
            // Simplified specificity calculation based on selector text
            // This is a workaround since ExCSS-Core doesn't expose the same selector structure
            var selectorText = selector.ToString();
            if (string.IsNullOrEmpty(selectorText)) return 0;
            
            selectorText = selectorText.ToLowerInvariant();
            var specificity = 0;
            
            // Count ID selectors (#id)
            specificity += (selectorText.Split('#').Length - 1) * (1 << 12);
            
            // Count class selectors (.class), attribute selectors ([attr]), and pseudo-classes (:hover)
            var classCount = (selectorText.Split('.').Length - 1) + 
                            (selectorText.Split('[').Length - 1) + 
                            CountPseudoClasses(selectorText);
            specificity += classCount * (1 << 8);
            
            // Count element selectors (rough approximation)
            var elementCount = CountElements(selectorText);
            specificity += elementCount * (1 << 4);
            
            return specificity;
        }
        
        private static int CountPseudoClasses(string selectorText)
        {
            var pseudoClasses = new[] { ":hover", ":active", ":focus", ":visited", ":link", ":target", ":enabled", ":disabled", ":checked" };
            return pseudoClasses.Sum(pseudo => (selectorText.Split(new[] { pseudo }, StringSplitOptions.None).Length - 1));
        }
        
        private static int CountElements(string selectorText)
        {
            // Simple approximation - count words that are not pseudo-classes, classes, or IDs
            var words = selectorText.Split(new[] { ' ', '>', '+', '~', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Count(word => !word.StartsWith(".") && !word.StartsWith("#") && !word.StartsWith(":") && !word.StartsWith("["));
        }
    }
}
