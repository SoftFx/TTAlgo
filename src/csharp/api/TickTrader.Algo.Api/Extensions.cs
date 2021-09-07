using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public static class Extensions
    {
        public static IEnumerable<T> TakeExact<T>(this IEnumerable<T> src, int count)
        {
            using (var e = src.GetEnumerator())
            {
                for (int i = 0; i < count; i++)
                {
                    if (e.MoveNext())
                        yield return e.Current;
                    else
                        throw new ArgumentOutOfRangeException("Not enough elements!");
                }
            }
        }
    }
}
