using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Api
{
    public interface IndicatorProvider
    {
        #region ATCFMethod

        IFastAdaptiveTrendLine FastAdaptiveTrendLine(DataSeries price, int countBars = 300);

        IFastTrendLineMomentum FastTrendLineMomentum(DataSeries price, int countBars = 300);

        IFATLSignal FATLSignal(BarSeries bars, int countBars = 300, AppliedPrice targetPrice = AppliedPrice.Close);

        IFTLMSTLM FTLMSTLM(DataSeries price, int countBars = 300);

        IPerfectCommodityChannelIndex PerfectCommodityChannelIndex(DataSeries price, int countBars = 300);

        IRangeBoundChannelIndexAvg RangeBoundChannelIndexAvg(DataSeries price, int std = 18, int countBars = 300);

        IRangeBoundChannelIndexBBands RangeBoundChannelIndexBBands(DataSeries price, int deviationPeriod = 100, double deviationCoeff = 2.0);

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


        #region Oscillators

        IAverageTrueRange AverageTrueRange(BarSeries bars, int period = 14);

        IBearsPower BearsPower(BarSeries bars, int period = 13, AppliedPrice targetPrice = AppliedPrice.Close);

        IBullsPower BullsPower(BarSeries bars, int period = 13, AppliedPrice targetPrice = AppliedPrice.Close);

        ICommodityChannelIndex CommodityChannelIndex(DataSeries price, int period = 14);

        IMacd MACD(DataSeries price, int fastEma = 12, int slowEma = 26, int macdSma = 9);

        IDeMarker DeMarker(BarSeries bars, int period = 14);

        IForceIndex ForceIndex(BarSeries bars, int period = 13, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, AppliedPrice targetPrice = AppliedPrice.Close);

        IMomentum Momentum(DataSeries price, int period = 14);

        IMovingAverageOscillator MovingAverageOscillator(DataSeries price, int fastEma = 12, int slowEma = 26, int macdSma = 9);

        IRelativeStrenghtIndex RelativeStrenghtIndex(DataSeries price, int period = 14);

        IRelativeVigorIndex RelativeVigorIndex(BarSeries bars, int period = 10);

        IStochasticOscillator StochasticOscillator(BarSeries bars, int kPeriod = 5, int slowing = 3, int dPeriod = 3, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, PriceField targetPrice = PriceField.LowHigh);

        IWilliamsPercentRange WilliamsPercentRange(BarSeries bars, int period = 14);

        #endregion


        #region Other

        IHeikenAshi HeikenAshi(BarSeries bars);

        IZigZag ZigZag(BarSeries bars, int depth = 12, int deviation = 5, int backstep = 3);

        #endregion


        #region Trend

        IAverageDirectionalMovementIndex AverageDirectionalMovementIndex(BarSeries bars, int period = 14, AppliedPrice targetPrice = AppliedPrice.Close);

        IBoolingerBands BoolingerBands(DataSeries price, int period = 20, int shift = 0, double deviations = 2.0);

        IEnvelopes Envelopes(DataSeries price, int period = 7, int shift = 0, double deviation = 0.1, MovingAverageMethod targetMethod = MovingAverageMethod.Simple);

        IIchimokuKinkoHyo IchimokuKinHyo(BarSeries bars, int tenkanSen = 9, int kijunSen = 26, int senkouSpanB = 52);

        IMovingAverage MovingAverage(DataSeries price, int period = 14, int shift = 0, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, double smoothFactor = 0.0667);

        IParabolicSar ParabolicSar(BarSeries bars, double step = 0.02, double maximum = 0.2);

        IStandardDeviation StandardDeviation(DataSeries price, int period = 20, int shift = 0, MovingAverageMethod targtMethod = MovingAverageMethod.Simple);

        #endregion


        #region Volumes

        IAccumulationDistribution AccumulationDistribution(BarSeries bars);

        IMoneyFlowIndex MoneyFlowIndex(BarSeries bars, int period = 14);

        IOnBalanceVolume OnBalanceVolume(BarSeries bars, AppliedPrice targetPrice = AppliedPrice.Close);

        IVolumes Volumes(BarSeries bars);

        #endregion
    }
}
