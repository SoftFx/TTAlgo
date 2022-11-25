namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class SMMA2 : IMovAvgAlgo
    {
        private readonly MovAvgCache _cache;

        private int _calcState;
        private double _prev, _prevSum, _sum, _current;


        public MovAvgArgs Args { get; }

        public double Average { get; private set; }


        public SMMA2(MovAvgArgs args)
        {
            Args = args;
            _cache = new MovAvgCache(args.Period);
        }


        public void Reset()
        {
            Average = double.NaN;
            _cache.Reset();
            _calcState = 0;
            _prev = _prevSum = _sum = _current = double.NaN;
        }

        public void Add(double value)
        {
            _cache.Add(value);

            var period = Args.Period;
            var cache = _cache.Cache;
            var cacheSize = _cache.CacheSize;

            if (cacheSize < period)
                return;

            if (_calcState == 0)
            {
                var sum = 0.0;
                for (var i = 0; i < cacheSize - 1; i++)
                    sum += cache[i];

                _prevSum = sum;
                _sum = sum + value;
                _current = _sum / period;

                _calcState = 1;
            }
            else
            {
                _prev = _current;
                _prevSum = _sum;

                _sum = _prevSum - _prev + value;
                _current = _sum / period;

                _calcState = 2;
            }

            Average = _current;
        }

        public void UpdateLast(double value)
        {
            _cache.UpdateLast(value);

            var period = Args.Period;
            if (_cache.CacheSize < period)
                return;

            if (_calcState == 1)
            {
                _sum = _prevSum + value;
                _current = _sum / period;
            }
            else if (_calcState == 2)
            {
                _sum = _prevSum - _prev + value;
                _current = _sum / period;
            }

            Average = _current;
        }
    }
}
