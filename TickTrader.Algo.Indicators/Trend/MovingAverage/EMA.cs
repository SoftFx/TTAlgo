using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal class EMA : MABase
    {
        public EMA(int period, int shift, AppliedPrice.Target targetPrice) : base(period, shift, targetPrice)
        {
        }

        public override void Init()
        {
            throw new System.NotImplementedException();
        }

        public override void Reset()
        {
            throw new System.NotImplementedException();
        }

        public override double Calculate(Bar bar)
        {
            throw new System.NotImplementedException();
        }
    }
}
