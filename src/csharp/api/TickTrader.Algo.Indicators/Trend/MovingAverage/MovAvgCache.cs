using System;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class MovAvgCache
    {
        public int Period { get; }

        public double[] Cache { get; }

        public int CacheSize { get; private set; }


        public MovAvgCache(int period)
        {
            Period = period;

            Cache = new double[period];
        }


        public void Reset()
        {
            CacheSize = 0;
        }

        public void Add(double value)
        {
            var cache = Cache;

            if (CacheSize < Period)
            {
                CacheSize++;
                cache[CacheSize - 1] = value;
            }
            else
            {
                for (var i = 0; i < CacheSize - 1; i++)
                    cache[i] = cache[i + 1];

                cache[CacheSize - 1] = value;
            }
        }

        public void UpdateLast(double value)
        {
            if (CacheSize == 0)
                throw new Exception("Last element doesn't exists.");

            Cache[CacheSize - 1] = value;
        }
    }
}
