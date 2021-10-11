using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Api
{
    public static class Extensions
    {
        public static bool IsBuy(this OrderSide side) => side == OrderSide.Buy;

        public static bool IsSell(this OrderSide side) => side == OrderSide.Sell;


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
