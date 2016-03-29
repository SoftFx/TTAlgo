using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class SMA : MABase
    {
        private double _sum;
        private Queue<double> _cache;

        public SMA(int period, int shift, AppliedPrice.Target targetPrice) : base(period, shift, targetPrice)
        {
        }

        public override void Init()
        {
            base.Init();
            _sum = 0;
            _cache = new Queue<double>(Period + 1);
        }

        public override void Reset()
        {
            base.Reset();
            _sum = 0;
            _cache.Clear();
        }

        public override double Calculate(Bar bar)
        {
            var appliedPrice = AppliedPrice.Calculate(bar, TargetPrice);
            _sum += appliedPrice;
            _cache.Enqueue(appliedPrice);
            if (Accumulated < Period)
            {
                Accumulated++;
                if (Accumulated < Period)
                {
                    return double.NaN;
                }
            }
            else
            {
                _sum -= _cache.Dequeue();
            }
            return _sum / Period;

        }
    }
}
