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

        public static TElement MinBy<TElement, TProperty>(this IEnumerable<TElement> source, Func<TElement, TProperty> selector)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            bool first = true;
            TElement minElement = default(TElement);
            TProperty minPropertyVal = default(TProperty);

            var comparer = Comparer<TProperty>.Default;

            foreach (var item in source)
            {
                if (first)
                {
                    minElement = item;
                    minPropertyVal = selector(item);
                    first = false;
                }
                else
                {
                    var property = selector(item);
                    if (comparer.Compare(property, minPropertyVal) < 0)
                    {
                        minPropertyVal = property;
                        minElement = item;
                    }
                }
            }

            if (first)
                throw new InvalidOperationException("Sequence is empty.");

            return minElement;
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

        public static int BinarySearch<T, TVal>(this IReadOnlyList<T> list, Func<T, TVal> getter, TVal value,
            BinarySearchTypes type = BinarySearchTypes.Exact, IComparer<TVal> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (list.Count == 0)
                return -1;

            comparer = comparer ?? Comparer<TVal>.Default;

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer.Compare(value, getter(list[middle]));
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            if (type == BinarySearchTypes.Exact)
                return -1;
            else if (type == BinarySearchTypes.NearestLower)
            {
                return lower > 0 ? lower - 1 : 0;
            }
            else // if (type == BinarySearchTypes.NearestHigher)
            {
                if (lower == 0)
                    return lower;
                else if (lower == list.Count)
                    return lower - 1;
                return lower;
            }
        }

        public static int BinarySearch<T>(this IReadOnlyList<T> list, T value,
            BinarySearchTypes type = BinarySearchTypes.Exact, IComparer<T> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (list.Count == 0)
                return -1;

            comparer = comparer ?? Comparer<T>.Default;

            int lower = 0;
            int upper = list.Count - 1;

            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer.Compare(value, list[middle]);
                if (comparisonResult == 0)
                    return middle;
                else if (comparisonResult < 0)
                    upper = middle - 1;
                else
                    lower = middle + 1;
            }

            if (type == BinarySearchTypes.Exact)
                return -1;
            else if (type == BinarySearchTypes.NearestLower)
            {
                return lower > 0 ? lower - 1 : 0;
            }
            else // if (type == BinarySearchTypes.NearestHigher)
                return lower;
        }
    }

    public enum BinarySearchTypes { NearestLower, Exact, NearestHigher }
}
