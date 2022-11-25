namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class SMA2 : IMovAvgAlgo
    {
        private readonly MovAvgArgs _args;
        private readonly MovAvgCache _cache;

        private double _partialSum, _sum;


        public SMA2(MovAvgArgs args)
        {
            _args = args;
            _cache = new MovAvgCache(args.Period);
        }


        public void OnInit()
        {
            _cache.Reset();
            _partialSum = _sum = 0;
        }

        public void OnReset()
        {
            _cache.Reset();
            _partialSum = _sum = 0;
        }

        public void OnAdded(double value)
        {
            _cache.Add(value);

            var cache = _cache.Cache;
            var cacheSize = _cache.CacheSize;

            var sum = 0.0;
            for (var i = 0; i < cacheSize - 1; i++)
                sum += cache[i];

            _partialSum = sum;
            _sum = sum + value;
        }

        public void OnLastUpdated(double value)
        {
            _cache.UpdateLast(value);

            _sum = _partialSum + value;
        }

        public double Calculate()
        {
            var period = _args.Period;
            return _cache.CacheSize < period ? double.NaN : _sum / period;
        }
    }
}
