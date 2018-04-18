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
    }
}
