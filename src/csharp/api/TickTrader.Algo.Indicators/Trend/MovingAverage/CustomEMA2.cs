namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class CustomEMA2 : IMovAvgAlgo
    {
        private readonly MovAvgArgs _args;
        private readonly MovAvgCache _cache;

        private double _prev, _current;


        public CustomEMA2(MovAvgArgs args)
        {
            _args = args;
            _cache = new MovAvgCache(_args.Period);
        }

        public void OnInit()
        {
            _prev = _current = double.NaN;
        }

        public void OnReset()
        {
            _prev = _current = double.NaN;
        }

        public void OnAdded(double value)
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
                var k = _args.SmoothFactor;

                var prev = cache[0];
                for (var i = 1; i < cacheSize - 1; i++)
                    prev = k * cache[i] + (1 - k) * prev;

                _prev = prev;
                _current = k * value + (1 - k) * prev;
            }
        }

        public void OnLastUpdated(double value)
        {
            _cache.UpdateLast(value);

            if (_cache.CacheSize == 1)
            {
                _current = value;
            }
            else
            {
                var k = _args.SmoothFactor;
                _current = k * value + (1 - k) * _prev;
            }
        }

        public double Calculate()
        {
            return _current;
        }
    }
}
