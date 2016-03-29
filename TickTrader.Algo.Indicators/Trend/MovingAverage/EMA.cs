using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class EMA : MABase
    {
        private double _prev;

        public double SmoothFactor { get; private set; }

        public EMA(int period, int shift, AppliedPrice.Target targetPrice) : base(period, shift, targetPrice)
        {
            SmoothFactor = 2.0/(period + 1.0);
        }

        public override void Init()
        {
            base.Init();
            _prev = double.NaN;
        }

        public override void Reset()
        {
            base.Reset();
            _prev = double.NaN;
        }

        public override double Calculate(Bar bar)
        {
            Accumulated++;
            var appliedPrice = AppliedPrice.Calculate(bar, TargetPrice);
            var res = double.IsNaN(_prev) ? appliedPrice : SmoothFactor*appliedPrice + (1 - SmoothFactor)*_prev;
            _prev = res;
            return res;
        }
    }
}
