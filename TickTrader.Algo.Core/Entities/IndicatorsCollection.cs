using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Core
{
    internal class IndicatorsCollection : IndicatorProvider
    {
        public IMacd MACD(DataSeries price, int fastEma = 12, int slowEma = 26, int macdSma = 9)
        {
            return new Indicators.Oscillators.MACD.Macd(price, fastEma, slowEma, macdSma);
        }

        public IMovingAverage MovingAverage(DataSeries price, int period = 14, int shift = 0, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, double smoothFactor = 0.0667)
        {
            return new Indicators.Trend.MovingAverage.MovingAverage(price, period, shift, targetMethod, smoothFactor);
        }

        
    }
}
