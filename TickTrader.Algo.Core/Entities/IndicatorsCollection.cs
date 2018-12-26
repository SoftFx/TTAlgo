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


        #region ATCFMethod
        public IFastAdaptiveTrendLine FastAdaptiveTrendLine(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.FastAdaptiveTrendLine.FastAdaptiveTrendLine(price, countBars);
        }

        public IFastTrendLineMomentum FastTrendLineMomentum(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.FastTrendLineMomentum.FastTrendLineMomentum(price, countBars);
        }

        public IFTLMSTLM FTLMSTLM(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.FTLMSTLM.FtlmStlm(price, countBars);
        }

        public IPerfectCommodityChannelIndex PerfectCommodityChannelIndex(DataSeries price, int countBars = 300)
        {
            return new Indicators.ATCFMethod.PerfectCommodityChannelIndex.PerfectCommodityChannelIndex(price, countBars);
        }

        public IRangeBoundChannelIndex RangeBoundChannelIndex(DataSeries price, int std = 18, int countBars = 300)
        {
            return new Indicators.ATCFMethod.RangeBoundChannelIndex.RangeBoundChannelIndex(price, std, countBars);
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
        #endregion


        #region Trend
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
    }
}
