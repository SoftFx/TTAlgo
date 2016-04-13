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

        private AppliedPrice.Target _targetPrice;
        [Parameter(DefaultValue = 5, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int)_targetPrice; }
            set { _targetPrice = (AppliedPrice.Target)value; }
        }

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
            _targetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _sma = new MovingAverage(Bars, Period, 0, Method.Simple, _targetPrice);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var appliedPrice = AppliedPrice.Calculate(Bars[pos], _targetPrice);
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
                    meanDeviation += Math.Abs(average - AppliedPrice.Calculate(Bars[i], _targetPrice));
                }
                meanDeviation /= Period;
            }
            Cci[pos] = (appliedPrice - average)/(0.015*meanDeviation);
        }
    }
}
