using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class TriMA : MABase
    {
        private SMA innerSMA;
        private SMA outerSMA;

        public TriMA(int period, int shift, AppliedPrice.Target targetPrice) : base(period, shift, targetPrice)
        {
        }

        public override void Init()
        {
            base.Init();
            int SMAPeriod = (Period + Period%2)/2;
            innerSMA = new SMA(SMAPeriod, 0, TargetPrice);
            outerSMA = new SMA(SMAPeriod, 0, AppliedPrice.Target.Close);
            innerSMA.Init();
            outerSMA.Init();
        }

        public override void Reset()
        {
            base.Reset();
            int SMAPeriod = (Period + Period%2)/2;
            innerSMA = new SMA(SMAPeriod, 0, TargetPrice);
            outerSMA = new SMA(SMAPeriod, 0, AppliedPrice.Target.Close);
            innerSMA.Init();
            outerSMA.Init();
        }

        public override double Calculate(Bar bar)
        {
            var innerRes = innerSMA.Calculate(bar);
            if (double.IsNaN(innerRes))
            {
                return innerRes;
            }
            var outerRes = outerSMA.Calculate(new Bar {Close = innerRes});
            return outerRes;
        }
    }
}
