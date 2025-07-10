using System;
using System.Reactive.Linq;

namespace Svg.Editor.Extensions
{
    public static class ReactiveExtensions
    {
        public static IObservable<Tuple<TSource, TSource>> PairWithPrevious<TSource>(this IObservable<TSource> source)
        {
            return source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Tuple.Create(acc.Item2, current)
            );
        }
    }
}
