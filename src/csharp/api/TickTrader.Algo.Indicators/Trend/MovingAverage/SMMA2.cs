namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class SMMA2 : IMovAvgAlgo
    {
        private readonly MovAvgArgs _args;
        private readonly MovAvgCache _cache;

        private int _calcState;
        private double _prev, _prevSum, _sum, _current;


        public SMMA2(MovAvgArgs args)
        {
            _args = args;
            _cache = new MovAvgCache(args.Period);
        }


        public void OnInit()
        {
            _cache.Reset();
            _calcState = 0;
            _prev = _prevSum = _sum = _current = double.NaN;
        }

        public void OnReset()
        {
            _cache.Reset();
            _calcState = 0;
            _prev = _prevSum = _sum = _current = double.NaN;
        }

        public void OnAdded(double value)
        {
            _cache.Add(value);

            var period = _args.Period;
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
        }

        public void OnLastUpdated(double value)
        {
            _cache.UpdateLast(value);

            var period = _args.Period;
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

        }

        public double Calculate()
        {
            return _current;
        }
    }
}
