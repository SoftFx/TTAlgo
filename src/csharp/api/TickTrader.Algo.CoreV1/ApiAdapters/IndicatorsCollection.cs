using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.CoreV1
{
    internal class IndicatorsCollection : IndicatorProvider
    {
        #region ATCFMethod

        public IFastAdaptiveTrendLine FastAdaptiveTrendLine(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.FastAdaptiveTrendLine.FastAdaptiveTrendLine(price, countBars);
        }

        public IFastTrendLineMomentum FastTrendLineMomentum(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.FastTrendLineMomentum.FastTrendLineMomentum(price, countBars);
        }

        public IFATLSignal FATLSignal(BarSeries bars, int countBars = 300, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            return new Indicators.ATCFMethod.FATLSignal.FatlSignal(bars, countBars, targetPrice);
        }

        public IFTLMSTLM FTLMSTLM(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.FTLMSTLM.FtlmStlm(price, countBars);
        }

        public IPerfectCommodityChannelIndex PerfectCommodityChannelIndex(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.PerfectCommodityChannelIndex.PerfectCommodityChannelIndex(price, countBars);
        }

        public IRangeBoundChannelIndexAvg RangeBoundChannelIndexAvg(DataSeries price, int std = 18, int countBars = 300, CalcFrequency frequency = CalcFrequency.M1)
        {
            return new Indicators.ATCFMethod.RangeBoundChannelIndex.RangeBoundChannelIndex(price, std, countBars, frequency);
        }

        public IRangeBoundChannelIndexBBands RangeBoundChannelIndexBBands(DataSeries price, int deviationPeriod = 100, double deviationCoeff = 2.0)
        {
            return new Indicators.ATCFMethod.RangeBoundChannelIndex.RangeBoundChannelIndexBBands(price, deviationPeriod, deviationCoeff);
        }

        public IReferenceFastTrendLine ReferenceFastTrendLine(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.ReferenceFastTrendLine.ReferenceFastTrendLine(price, countBars); 
        }

        public IReferenceSlowTrendLine ReferenceSlowTrendLine(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.ReferenceSlowTrendLine.ReferenceSlowTrendLine(price, countBars);
        }

        public ISlowAdaptiveTrendLine SlowAdaptiveTrendLine(DataSeries price, int countBras = 300)
        {
            return new Indicators.ATCFMethod.SlowAdaptiveTrendLine.SlowAdaptiveTrendLine(price, countBras);
        }

        public ISlowTrendLineMomentum SlowTrendLineMomentum(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.SlowTrendLineMomentum.SlowTrendLineMomentum(price, countBars);
        }

        public IMarketFacilitationIndex MarketFacilitationIndex(BarSeries bars, double pointSize = 0.0001)
        {
            return new Indicators.BillWilliams.MarketFacilitationIndex.MarketFacilitationIndex(bars, pointSize);
        }

        #endregion


        #region BillWilliams

        public IAcceleratorOscillator AcceleratorOscillator(BarSeries bars, int fastSmaPeriod = 5, int slowSmaPeriod = 34, int dataLimit = 34)
        {
            return new Indicators.BillWilliams.AcceleratorOscillator.AcceleratorOscillator(bars, fastSmaPeriod, slowSmaPeriod, dataLimit);
        }

        public IAlligator Alligator(DataSeries price, int jawsPeriod = 13, int jawsShift = 8, int teethPeriod = 8, int teethShift = 5, int lipsPeriod = 5, int lipsShift = 3, MovingAverageMethod targetMethod = MovingAverageMethod.Smoothed)
        {
            return new Indicators.BillWilliams.Alligator.Alligator(price, jawsPeriod, jawsShift, teethPeriod, teethShift, lipsPeriod, lipsShift, targetMethod);
        }

        public IAwesomeOscillator AwesomeOscillator(BarSeries bars, int fastSmaPeriod = 5, int slowSmaPeriod = 34, int dataLimit = 34)
        {
            return new Indicators.BillWilliams.AwesomeOscillator.AwesomeOscillator(bars, fastSmaPeriod, slowSmaPeriod, dataLimit);
        }

        public IFractals Fractals(BarSeries bars)
        {
            return new Indicators.BillWilliams.Fractals.Fractals(bars);
        }

        public IGatorOscillator GatorOscillator(DataSeries price, int jawsPeriod = 13, int jawsShift = 8, int teethPeriod = 8, int teethShift = 5, int lipsPeriod = 5, int lipsShift = 3, MovingAverageMethod targetMethod = MovingAverageMethod.Smoothed)
        {
            return new Indicators.BillWilliams.GatorOscillator.GatorOscillator(price, jawsPeriod, jawsShift, teethPeriod, teethShift, lipsPeriod, lipsShift, targetMethod);
        }

        #endregion


        #region Oscillators

        public IAverageTrueRange AverageTrueRange(BarSeries bars, int period = 14)
        {
            return new Indicators.Oscillators.AverageTrueRange.AverageTrueRange(bars, period);
        }

        public IBearsPower BearsPower(BarSeries bars, int period = 13, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            return new Indicators.Oscillators.BearsPower.BearsPower(bars, period, targetPrice);
        }

        public IBullsPower BullsPower(BarSeries bars, int period = 13, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            return new Indicators.Oscillators.BullsPower.BullsPower(bars, period, targetPrice);
        }

        public ICommodityChannelIndex CommodityChannelIndex(DataSeries price, int period = 14)
        {
            return new Indicators.Oscillators.CommodityChannelIndex.CommodityChannelIndex(price, period);
        }

        public IDeMarker DeMarker(BarSeries bars, int period = 14)
        {
            return new Indicators.Oscillators.DeMarker.DeMarker(bars, period);
        }

        public IForceIndex ForceIndex(BarSeries bars, int period = 13, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            return new Indicators.Oscillators.ForceIndex.ForceIndex(bars, period, targetMethod, targetPrice);
        }

        public IMacd MACD(DataSeries price, int fastEma = 12, int slowEma = 26, int macdSma = 9)
        {
            return new Indicators.Oscillators.MACD.Macd(price, fastEma, slowEma, macdSma);
        }

        public IMomentum Momentum(DataSeries price, int period = 14)
        {
            return new Indicators.Oscillators.Momentum.Momentum(price, period);
        }

        public IMovingAverageOscillator MovingAverageOscillator(DataSeries price, int fastEma = 12, int slowEma = 26, int macdSma = 9)
        {
            return new Indicators.Oscillators.MovingAverageOscillator.MovingAverageOscillator(price, fastEma, slowEma, macdSma);
        }

        public IRelativeStrenghtIndex RelativeStrenghtIndex(DataSeries price, int period = 14)
        {
            return new Indicators.Oscillators.RelativeStrengthIndex.RelativeStrengthIndex(price, period);
        }

        public IRelativeVigorIndex RelativeVigorIndex(BarSeries bars, int period = 10)
        {
            return new Indicators.Oscillators.RelativeVigorIndex.RelativeVigorIndex(bars, period);
        }

        public IStochasticOscillator StochasticOscillator(BarSeries bars, int kPeriod = 5, int slowing = 3, int dPeriod = 3, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, PriceField targetPrice = PriceField.LowHigh)
        {
            return new Indicators.Oscillators.StochasticOscillator.StochasticOscillator(bars, kPeriod, slowing, dPeriod, targetMethod, targetPrice);
        }

        public IWilliamsPercentRange WilliamsPercentRange(BarSeries bars, int period = 14)
        {
            return new Indicators.Oscillators.WilliamsPercentRange.WilliamsPercentRange(bars, period);
        }

        #endregion


        #region Other

        public IHeikenAshi HeikenAshi(BarSeries bars)
        {
            return new Indicators.Other.HeikenAshi.HeikenAshi(bars);
        }

        public IZigZag ZigZag(BarSeries bars, int depth = 12, int deviation = 5, int backstep = 3)
        {
            return new Indicators.Other.ZigZag.ZigZag(bars, depth, deviation, backstep);
        }

        #endregion


        #region Trend

        public IAverageDirectionalMovementIndex AverageDirectionalMovementIndex(BarSeries bars, int period = 14, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            return new Indicators.Trend.AverageDirectionalMovementIndex.AverageDirectionalMovementIndex(bars, period, targetPrice);
        }

        public IBoolingerBands BoolingerBands(DataSeries price, int period = 20, int shift = 0, double deviations = 2)
        {
            return new Indicators.Trend.BollingerBands.BollingerBands(price, period, shift, deviations);
        }

        public IMovingAverage MovingAverage(DataSeries price, int period = 14, int shift = 0, MovingAverageMethod targetMethod = MovingAverageMethod.Simple, double smoothFactor = 0.0667)
        {
            return new Indicators.Trend.MovingAverage.MovingAverage(price, period, shift, targetMethod, smoothFactor);
        }

        public IEnvelopes Envelopes(DataSeries price, int period = 7, int shift = 0, double deviation = 0.1, MovingAverageMethod targetMethod = MovingAverageMethod.Simple)
        {
            return new Indicators.Trend.Envelopes.Envelopes(price, period, shift, deviation, targetMethod);
        }

        public IIchimokuKinkoHyo IchimokuKinHyo(BarSeries bars, int tenkanSen = 9, int kijunSen = 26, int senkouSpanB = 52)
        {
            return new Indicators.Trend.IchimokuKinkoHyo.IchimokuKinkoHyo(bars, tenkanSen, kijunSen, senkouSpanB);
        }

        public IParabolicSar ParabolicSar(BarSeries bars, double step = 0.02, double maximum = 0.2)
        {
            return new Indicators.Trend.ParabolicSAR.ParabolicSar(bars, step, maximum);
        }

        public IStandardDeviation StandardDeviation(DataSeries price, int period = 20, int shift = 0, MovingAverageMethod targtMethod = MovingAverageMethod.Simple)
        {
            return new Indicators.Trend.StandardDeviation.StandardDeviation(price, period, shift, targtMethod);
        }

        #endregion


        #region Volumes

        public IAccumulationDistribution AccumulationDistribution(BarSeries bars)
        {
            return new Indicators.Volumes.AccumulationDistribution.AccumulationDistribution(bars);
        }

        public IMoneyFlowIndex MoneyFlowIndex(BarSeries bars, int period = 14)
        {
            return new Indicators.Volumes.MoneyFlowIndex.MoneyFlowIndex(bars, period);
        }

        public IOnBalanceVolume OnBalanceVolume(BarSeries bars, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            return new Indicators.Volumes.OnBalanceVolume.OnBalanceVolume(bars, targetPrice);
        }

        public IVolumes Volumes(BarSeries bars)
        {
            return new Indicators.Volumes.Volumes.Volumes(bars);
        }

        #endregion
    }
}
