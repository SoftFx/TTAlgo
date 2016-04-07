using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Functions
{
    internal static class Utility
    {
        internal static void ApplyShiftedValue(DataSeries series, int shift, double curVal, Queue<double> cache, int barsCount)
        {
            if (shift > 0)
            {
                series[0] = barsCount > shift ? cache.Dequeue() : double.NaN;
                cache.Enqueue(curVal);
            }
            else if (shift <= 0 && -shift < barsCount)
            {
                series[-shift] = curVal;
            }
        }

        internal static double GetShiftedValue(int shift, double curVal, Queue<double> cache, int barsCount)
        {
            var res = double.NaN;
            if (shift > 0)
            {
                res = barsCount > shift ? cache.Dequeue() : double.NaN;
                cache.Enqueue(curVal);
            }
            else if (shift <= 0 && -shift < barsCount)
            {
                res = curVal;
            }
            return res;
        }
    }
}
