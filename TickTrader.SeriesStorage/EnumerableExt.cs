using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public static class EnumerableExt
    {
        public static IEnumerable<T> LockOnEach<T>(this IEnumerable<T> src, object lockObj)
        {
            IEnumerator<T> enumerator;
            T current;

            lock (lockObj)
            {
                enumerator = src.GetEnumerator();
                if (!enumerator.MoveNext())
                    yield break;
                current = enumerator.Current;
            }

            while (true)
            {
                yield return current;

                lock (lockObj)
                {
                    if (!enumerator.MoveNext())
                        break;

                    current = enumerator.Current;
                }
            }
        }
    }
}
