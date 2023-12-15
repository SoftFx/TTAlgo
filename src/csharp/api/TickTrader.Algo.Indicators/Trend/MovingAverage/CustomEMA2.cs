namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    /// <summary>
    /// Regular EMA does not reset when MA window moves forward.
    /// This MA algo recalculates EMA formula from zero every time MA window moves forward
    /// </summary>
    /// This implementation is unused after design change. Will leave it for now
    internal class CustomEMA2 : IMovAvgAlgo
    {
        private readonly MovAvgCache _cache;

        private double _prev, _current;


        public MovAvgArgs Args { get; }

        public double Average { get; private set; }


        public CustomEMA2(MovAvgArgs args)
        {
            Args = args;
            _cache = new MovAvgCache(Args.Period);
        }


        public void Reset()
        {
            Average = double.NaN;
            _prev = _current = double.NaN;
        }

        public void Add(double value)
        {
            _cache.Add(value);

            var cache = _cache.Cache;
            var cacheSize = _cache.CacheSize;

            if (cacheSize == 1)
            {
                _current = value;
            }
            else
            {
                var k = Args.SmoothFactor;

                var prev = cache[0];
                for (var i = 1; i < cacheSize - 1; i++)
                    prev = k * cache[i] + (1 - k) * prev;

                _prev = prev;
                _current = k * value + (1 - k) * prev;
            }

            Average = _current;
        }

        public void UpdateLast(double value)
        {
            _cache.UpdateLast(value);

            if (_cache.CacheSize == 1)
            {
                _current = value;
            }
            else
            {
                var k = Args.SmoothFactor;
                _current = k * value + (1 - k) * _prev;
            }

            Average = _current;
        }
    }
}
