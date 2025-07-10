using System.Collections.Generic;
using Svg;

namespace ExCSS.Model.TextBlocks
{
    internal class RangeBlock : Block
    {
        public RangeBlock()
        {
            GrammarSegment = GrammarSegment.Range;
        }

        internal bool IsEmpty
        {
            get { return SelectedRange == null || SelectedRange.Length == 0; }
        }

        internal string[] SelectedRange { get; private set; }

        internal RangeBlock SetRange(string start, string end)
        {
            var startValue = int.Parse(start, System.Globalization.NumberStyles.HexNumber);

            if (startValue > Specification.MaxPoint)
            {
                return this;
            }

            if (end == null)
            {
                SelectedRange = new [] { startValue.ConvertFromUtf32() };
            }
            else
            {
                var list = new List<string>();
                var endValue = int.Parse(end, System.Globalization.NumberStyles.HexNumber);

                if (endValue > Specification.MaxPoint)
                {
                    endValue = Specification.MaxPoint;
                }

                for (; startValue <= endValue; startValue++)
                {
                    list.Add(startValue.ConvertFromUtf32());
                }

                SelectedRange = list.ToArray();
            }

            return this;
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return string.Empty;
            }

            if (SelectedRange.Length == 1)
            {
                return "#" + SelectedRange[0].ConvertToUtf32(0).ToString("x");
            }

            return "#" + SelectedRange[0].ConvertToUtf32( 0).ToString("x") + "-#" +
                SelectedRange[SelectedRange.Length - 1].ConvertToUtf32(0).ToString("x");
        }
    }
}
