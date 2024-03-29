﻿using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Oscillators.AverageTrueRange
{
    [Indicator(Category = "Oscillators", DisplayName = "Average True Range", Version = "1.1")]
    public class AverageTrueRange : Indicator, IAverageTrueRange
    {
        private IMovAvgAlgo _ma;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "ATR", Target = OutputTargets.Window1, DefaultColor = Colors.DodgerBlue)]
        public DataSeries Atr { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AverageTrueRange() { }

        public AverageTrueRange(BarSeries bars, int period)
        {
            Bars = bars;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _ma = MovAvg.Create(Period, MovingAverageMethod.Simple);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var tr = Math.Abs(Bars[0].High - Bars[0].Low);
            if (Bars.Count > 1)
            {
                tr = Math.Max(tr, Math.Abs(Bars[0].High - Bars[1].Close));
                tr = Math.Max(tr, Math.Abs(Bars[0].Low - Bars[1].Close));
            }
            if (!isNewBar)
            {
                _ma.UpdateLast(tr);
            }
            else
            {
                _ma.Add(tr);
            }
            Atr[LastPositionChanged] = _ma.Average;
        }
    }
}
