using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class CustomEMA : MABase
    {
        private double _prev;
        private double _multiplier;
        private Queue<double> _cache;

        public double SmoothFactor { get; private set; }

        public CustomEMA(int period, int shift, AppliedPrice.Target targetPrice, double smoothFactor) : base(period, shift, targetPrice)
        {
            SmoothFactor = smoothFactor;
        }

        public override void Init()
        {
            base.Init();
            _prev = double.NaN;
            _multiplier = SmoothFactor;
            _cache = new Queue<double>(Period + 1);
        }

        public override void Reset()
        {
            base.Reset();
            _prev = double.NaN;
            _multiplier = 1;
            _cache.Clear();
        }

        public override double Calculate(Bar bar)
        {
            if (Accumulated < Period)
            {
                Accumulated++;
                if (Accumulated < Period)
                {
                    _multiplier *= 1 - SmoothFactor;
                }
            }
            if (_cache.Count + 1 > Period)
            {
                _prev -= _cache.Dequeue()*_multiplier - _cache.Peek()*_multiplier;
            }
            var appliedPrice = AppliedPrice.Calculate(bar, TargetPrice);
            double res;
            if (double.IsNaN(_prev))
            {
                res = appliedPrice;
            }
            else
            {
                res = SmoothFactor*appliedPrice + (1 - SmoothFactor)*_prev;
            }
            _cache.Enqueue(appliedPrice);
            _prev = res;
            return res;
        }
    }
}
