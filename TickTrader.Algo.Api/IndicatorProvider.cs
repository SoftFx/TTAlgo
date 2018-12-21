using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Api
{
    public interface IndicatorProvider
    {
        IMovingAverage MovingAverage(DataSeries price, int period = 14, int shift = 0, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, double smoothFactor = 0.0667);

        IMacd MACD(DataSeries price, int fastEma = 12, int slowEma = 26, int macdSma = 9);
    }
}
