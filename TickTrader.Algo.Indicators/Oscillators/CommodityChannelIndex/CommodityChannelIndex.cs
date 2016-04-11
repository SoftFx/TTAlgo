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
        private IMA _meanDeviation;

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
            _meanDeviation = MABase.CreateMaInstance(Period, Method.Simple);
            _meanDeviation.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var appliedPrice = AppliedPrice.Calculate(Bars[pos], _targetPrice);
            var val = double.IsNaN(_sma.Average[pos]) ? appliedPrice : _sma.Average[pos] - appliedPrice;
            if (IsUpdate)
            {
                _meanDeviation.UpdateLast(Math.Abs(val));
            }
            else
            {
                _meanDeviation.Add(Math.Abs(val));
            }
            Cci[pos] = (appliedPrice - _sma.Average[pos])/(0.015*_meanDeviation.Average);
        }
    }
}
