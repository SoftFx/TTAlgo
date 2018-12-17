using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Core
{
    internal class IndicatorsCollection : IndicatorProvider
    {
        public IMovingAverage MovingAverage(DataSeries price, int period, int shift = 0, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, double smoothFactor = 0.0667)
        {
            return new Indicators.Trend.MovingAverage.MovingAverage(price, period, shift, targetMethod, smoothFactor);
        }
    }
}
