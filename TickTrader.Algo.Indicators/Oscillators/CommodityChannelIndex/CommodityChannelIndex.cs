using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.CommodityChannelIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Commodity Channel Index")]
    public class CommodityChannelIndex : Indicator
    {
        private MovingAverage _sma;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Typical, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "CCI", DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Cci { get; set; }

        public int LastPositionChanged { get { return _sma.LastPositionChanged; } }

        public CommodityChannelIndex() { }

        public CommodityChannelIndex(DataSeries<Bar> bars, int period, AppliedPrice.Target targetPrice)
        {
            Bars = bars;
            Period = period;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _sma = new MovingAverage(Bars, Period, 0, Method.Simple, TargetPrice);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var appliedPrice = AppliedPrice.Calculate(Bars[pos], TargetPrice);
            var average = _sma.Average[pos];
            var meanDeviation = 0.0;
            if (double.IsNaN(average))
            {
                meanDeviation = double.NaN;
            }
            else
            {
                for (var i = pos; i < pos + Period; i++)
                {
                    meanDeviation += Math.Abs(average - AppliedPrice.Calculate(Bars[i], TargetPrice));
                }
                meanDeviation /= Period;
            }
            Cci[pos] = (appliedPrice - average)/(0.015*meanDeviation);
        }
    }
}
