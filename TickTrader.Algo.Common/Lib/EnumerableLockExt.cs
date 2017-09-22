using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class EnumerableLockExt
    {
        public static IEnumerable<T> GetSyncWrapper<T>(this IEnumerable<T> src, object lockObj)
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
