using System;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class SMA2 : IMovAvgAlgo
    {
        private readonly MovAvgCache _cache;
        private readonly Func<double> _calcAvgFunc;

        private double _partialSum, _sum;


        public MovAvgArgs Args { get; }

        public double Average { get; private set; }

        public double Sum => _sum;


        public SMA2(MovAvgArgs args, bool onlyFullMode = true)
        {
            Args = args;
            _cache = new MovAvgCache(args.Period);

            if (onlyFullMode)
                _calcAvgFunc = CalcAvgOnlyFull;
            else
                _calcAvgFunc = CalcAvgAny;
        }


        public void Reset()
        {
            Average = double.NaN;
            _cache.Reset();
            _partialSum = _sum = 0;
        }

        public void Add(double value)
        {
            _cache.Add(value);

            var cache = _cache.Cache;
            var cacheSize = _cache.CacheSize;

            var sum = 0.0;
            for (var i = 0; i < cacheSize - 1; i++)
                sum += cache[i];

            _partialSum = sum;
            _sum = sum + value;

            Average = _calcAvgFunc();
        }

        public void UpdateLast(double value)
        {
            _cache.UpdateLast(value);

            _sum = _partialSum + value;

            Average = _calcAvgFunc();
        }


        private double CalcAvgOnlyFull()
        {
            var period = Args.Period;
            return _cache.CacheSize < period ? double.NaN : _sum / period;
        }

        private double CalcAvgAny()
        {
            return _sum / Args.Period;
        }
    }
}
