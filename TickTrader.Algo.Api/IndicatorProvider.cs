using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Api
{
    public interface IndicatorProvider
    {
        IMacd MACD(DataSeries price, int fastEma = 12, int slowEma = 26, int macdSma = 9);


        #region ATCFMethod
        IFastAdaptiveTrendLine FastAdaptiveTrendLine(DataSeries price, int countBars = 300);

        IFastTrendLineMomentum FastTrendLineMomentum(DataSeries price, int countBars = 300);

        IFTLMSTLM FTLMSTLM(DataSeries price, int countBars = 300);

        IPerfectCommodityChannelIndex PerfectCommodityChannelIndex(DataSeries price, int countBars = 300);

        IRangeBoundChannelIndex RangeBoundChannelIndex(DataSeries price, int std = 18, int countBars = 300);

        IReferenceFastTrendLine ReferenceFastTrendLine(DataSeries price, int countBars = 300);

        IReferenceSlowTrendLine ReferenceSlowTrendLine(DataSeries price, int countBars = 300);

        ISlowAdaptiveTrendLine SlowAdaptiveTrendLine(DataSeries price, int countBars = 300);

        ISlowTrendLineMomentum SlowTrendLineMomentum(DataSeries price, int countBars = 300);
        #endregion


        #region BillWilliams
        IAcceleratorOscillator AcceleratorOscillator(BarSeries bars, int fastSmaPeriod = 5, int slowSmaPeriod = 34, int dataLimit = 34);

        IAlligator Alligator(DataSeries price, int jawsPeriod = 13, int jawsShift = 8, int teethPeriod = 8, int teethShift = 5, int lipsPeriod = 5, int lipsShift = 3, MovingAverageMethod targetMethod = MovingAverageMethod.Smoothed);

        IAwesomeOscillator AwesomeOscillator(BarSeries bars, int fastSmaPeriod = 5, int slowSmaPeriod = 34, int dataLimit = 34);

        IFractals Fractals(BarSeries bars);

        IGatorOscillator GatorOscillator(DataSeries price, int jawsPeriod = 13, int jawsShift = 8, int teethPeriod = 8, int teethShift = 5, int lipsPeriod = 5, int lipsShift = 3, MovingAverageMethod targetMethod = MovingAverageMethod.Smoothed);

        IMarketFacilitationIndex MarketFacilitationIndex(BarSeries bars, double pointSize = 10e-5);
        #endregion


        #region Trend
        IBoolingerBands BoolingerBands(DataSeries price, int period = 20, int shift = 0, double deviations = 2.0);

        IEnvelopes Envelopes(DataSeries price, int period = 7, int shift = 0, double deviation = 0.1, MovingAverageMethod targetMethod = MovingAverageMethod.Simple);

        IIchimokuKinkoHyo IchimokuKinHyo(BarSeries bars, int tenkanSen = 9, int kijunSen = 26, int senkouSpanB = 52);

        IMovingAverage MovingAverage(DataSeries price, int period = 14, int shift = 0, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, double smoothFactor = 0.0667);

        IParabolicSar ParabolicSar(BarSeries bars, double step = 0.02, double maximum = 0.2);

        IStandardDeviation StandardDeviation(DataSeries price, int period = 20, int shift = 0, MovingAverageMethod targtMethod = MovingAverageMethod.Simple);
        #endregion
    }
}
