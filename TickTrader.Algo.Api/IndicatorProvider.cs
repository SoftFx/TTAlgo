using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Api
{
    public interface IndicatorProvider
    {
        IMovingAverage MovingAverage(DataSeries price, int period, int shift = 0, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, double smoothFactor = 0.0667);
    }
}
