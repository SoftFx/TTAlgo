using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Utility
{
    public static class PeriodHelper
    {
        public static double FindMin(DataSeries series, int period, bool fullPeriod = true)
        {
            var res = double.PositiveInfinity;
            var n = fullPeriod ? series.Count >= period ? period : 0 : Math.Min(series.Count, period);
            for (var i = 0; i < n; i++)
            {
                res = Math.Min(res, series[i]);
            }
            return double.IsPositiveInfinity(res) ? double.NaN : res;
        }

        public static double FindMax(DataSeries series, int period, bool fullPeriod = true)
        {
            var res = double.NegativeInfinity;
            var n = fullPeriod ? series.Count >= period ? period : 0 : Math.Min(series.Count, period);
            for (var i = 0; i < n; i++)
            {
                res = Math.Max(res, series[i]);
            }
            return double.IsNegativeInfinity(res) ? double.NaN : res;
        }
    }
}
