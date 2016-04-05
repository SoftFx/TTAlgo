using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    internal interface IMA
    {
        int Period { get; }
        int Shift { get; }
        AppliedPrice.Target TargetPrice { get; }

        int Accumulated { get; }

        void Init();
        void Reset();
        double Calculate(Bar bar);
    }
}
