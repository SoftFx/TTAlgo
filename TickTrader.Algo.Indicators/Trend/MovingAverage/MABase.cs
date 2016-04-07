using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal abstract class MABase : IMA
    {
        public int Period { get; private set; }
        public int Shift { get; private set; }
        public AppliedPrice.Target TargetPrice { get; private set; }
        
        public int Accumulated { get; protected set; }

        public MABase(int period, int shift, AppliedPrice.Target targetPrice)
        {
            Period = period;
            Shift = shift;
            TargetPrice = targetPrice;
            Accumulated = 0;
        }

        public virtual void Init()
        {
            Accumulated = 0;
        }

        public virtual void Reset()
        {
            Accumulated = 0;
        }

        public abstract double Calculate(Bar bar);
    }
}
