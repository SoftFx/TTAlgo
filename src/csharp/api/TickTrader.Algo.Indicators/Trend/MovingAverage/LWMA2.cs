namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class LWMA2 : IMovAvgAlgo
    {
        private readonly MovAvgCache _cache;

        private int _indexSum;
        private double _partialSum, _sum;


        public MovAvgArgs Args { get; }

        public double Average { get; private set; }


        public LWMA2(MovAvgArgs args)
        {
            Args = args;
            _cache = new MovAvgCache(args.Period);
        }


        public void Reset()
        {
            Average = double.NaN;
            _cache.Reset();
            _indexSum = 0;
            _partialSum = _sum = double.NaN;
        }

        public void Add(double value)
        {
            _cache.Add(value);

            var cache = _cache.Cache;
            var cacheSize = _cache.CacheSize;

            _indexSum = cacheSize * (cacheSize + 1) / 2;
            var sum = 0.0;
            for (var i = 0; i < cacheSize - 1; i++)
                sum += cache[i] * (i + 1);

            _partialSum = sum;
            _sum = sum + value * cacheSize;

            Average = Calculate();
        }

        public void UpdateLast(double value)
        {
            _cache.UpdateLast(value);

            _sum = _partialSum + value * _cache.CacheSize;

            Average = Calculate();
        }


        private double Calculate()
        {
            return _cache.CacheSize < Args.Period ? double.NaN : _sum / _indexSum;
        }
    }
}
