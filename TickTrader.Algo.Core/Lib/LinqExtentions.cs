using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public static class LinqExtentions
    {
        public static TElement MaxBy<TElement, TProperty>(this IEnumerable<TElement> source, Func<TElement, TProperty> selector)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            bool first = true;
            TElement maxElement = default(TElement);
            TProperty maxProperty = default(TProperty);

            var comparer = Comparer<TProperty>.Default;

            foreach (var item in source)
            {
                if (first)
                {
                    maxElement = item;
                    maxProperty = selector(item);
                    first = false;
                }
                else
                {
                    var property = selector(item);
                    if (comparer.Compare(property, maxProperty) > 0)
                    {
                        maxProperty = property;
                        maxElement = item;
                    }
                }
            }

            if (first)
                throw new InvalidOperationException("Sequence is empty.");

            return maxElement;
        }

        public static IEnumerable<TOut> JoinSorted<TIn, TOut>(this IEnumerable<TIn> first, IEnumerable<TIn> second,
            Func<TIn, TIn, int> joinCompare, Func<TIn, TIn, TOut> joinFunc)
        {
            using (var firstEnumerator = first.GetEnumerator())
            using (var secondEnumerator = second.GetEnumerator())
            {
                var elementsLeftInFirst = firstEnumerator.MoveNext();
                var elementsLeftInSecond = secondEnumerator.MoveNext();

                while (elementsLeftInFirst || elementsLeftInSecond)
                {
                    if (!elementsLeftInFirst)
                    {
                        do
                        {
                            yield return joinFunc(default(TIn), secondEnumerator.Current);
                        }
                        while (secondEnumerator.MoveNext());

                        yield break;
                    }

                    if (!elementsLeftInSecond)
                    {
                        do
                        {
                            yield return joinFunc(firstEnumerator.Current, default(TIn));
                        }
                        while (firstEnumerator.MoveNext());

                        yield break;
                    }

                    var cmpResult = joinCompare(firstEnumerator.Current, secondEnumerator.Current);

                    if (cmpResult == 0)
                    {
                        yield return joinFunc(firstEnumerator.Current, secondEnumerator.Current);
                        elementsLeftInFirst = firstEnumerator.MoveNext();
                        elementsLeftInSecond = secondEnumerator.MoveNext();
                    }
                    else if(cmpResult < 0)
                    {
                        yield return joinFunc(firstEnumerator.Current, default(TIn));
                        elementsLeftInFirst = firstEnumerator.MoveNext();
                    }
                    else
                    {
                        yield return joinFunc(default(TIn), secondEnumerator.Current);
                        elementsLeftInSecond = secondEnumerator.MoveNext();
                    }
                }
            }
        }
    }
}
