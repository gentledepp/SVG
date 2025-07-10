using System.Collections.Generic;
using Svg.Interfaces;

namespace Svg
{
    public class SvgSortedList<TKey, TValue> : SortedList<TKey, TValue>, ISortedList<TKey, TValue>
    {
    }
}