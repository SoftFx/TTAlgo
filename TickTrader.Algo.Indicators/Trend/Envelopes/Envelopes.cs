﻿using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.Envelopes
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Envelopes")]
    public class Envelopes : Indicator
    {
        private MovingAverage.MovingAverage _middleLine;
        
        [Parameter(DefaultValue = 7, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = 0.1, DisplayName = "Deviation(%)")]
        public double Deviation { get; set; }

        [Parameter(DefaultValue = Method.Simple, DisplayName = "Method")]
        public Method TargetMethod { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DefaultColor = Colors.Blue)]
        public DataSeries TopLine { get; set; }

        [Output(DefaultColor = Colors.Red)]
        public DataSeries BottomLine { get; set; }

        public int LastPositionChanged { get { return _middleLine.LastPositionChanged; } }

        public Envelopes() { }

        public Envelopes(DataSeries<Bar> bars, int period, int shift, Method targetMethod = Method.Simple,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            Shift = shift;
            TargetMethod = targetMethod;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _middleLine = new MovingAverage.MovingAverage(Bars, Period, Shift, TargetMethod, TargetPrice);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var val = _middleLine.Average[pos];
            TopLine[pos] = val*(1.0 + Deviation/100);
            BottomLine[pos] = val*(1.0 - Deviation/100);
        }
    }
}
