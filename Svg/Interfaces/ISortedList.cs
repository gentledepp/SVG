using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Svg.Interfaces
{
    public interface ISortedList<TKey, TValue> : IDictionary<TKey, TValue>
    {
        new IList<TValue> Values { get; }
        new IList<TKey> Keys { get; }
    }
}
