using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class LWMA : MABase
    {
        private int _indexSum;
        private Queue<double> _cache;

        public LWMA(int period, int shift, AppliedPrice.Target targetPrice) : base(period, shift, targetPrice)
        {
        }

        public override void Init()
        {
            base.Init();
            _indexSum = 0;
            _cache = new Queue<double>(Period + 1);
        }

        public override void Reset()
        {
            base.Reset();
            _indexSum = 0;
            _cache.Clear();
        }

        public override double Calculate(Bar bar)
        {
            var appliedPrice = AppliedPrice.Calculate(bar, TargetPrice);
            _cache.Enqueue(appliedPrice);
            if (Accumulated < Period)
            {
                Accumulated++;
                _indexSum += Accumulated;
                if (Accumulated < Period)
                {
                    return double.NaN;
                }
            }
            else
            {
                _cache.Dequeue();
            }
            var sum = 0.0;
            var index = 1;
            foreach (var price in _cache)
            {
                sum += price*index;
                index++;
            }
            return sum/_indexSum;
        }
    }
}
