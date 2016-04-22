using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Utility
{
    public static class PeriodHelper
    {
        public static double FindMin(DataSeries series, int period)
        {
            var res = double.PositiveInfinity;
            if (series.Count >= period)
            {
                for (var i = 0; i < period; i++)
                {
                    res = Math.Min(res, series[i]);
                }
            }
            return double.IsPositiveInfinity(res) ? double.NaN : res;
        }

        public static double FindMax(DataSeries series, int period)
        {
            var res = double.NegativeInfinity;
            if (series.Count >= period)
            {
                for (var i = 0; i < period; i++)
                {
                    res = Math.Max(res, series[i]);
                }
            }
            return double.IsNegativeInfinity(res) ? double.NaN : res;
        }
    }
}
