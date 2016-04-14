using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Oscillators.AverageTrueRange
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Average True Range")]
    public class AverageTrueRange : Indicator
    {
        private IMA _ma;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "ATR", DefaultColor = Colors.DodgerBlue)]
        public DataSeries Atr { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AverageTrueRange() { }

        public AverageTrueRange(DataSeries<Bar> bars, int period)
        {
            Bars = bars;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _ma = MABase.CreateMaInstance(Period, Method.Simple);
            _ma.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var tr = Math.Abs(Bars[0].High - Bars[0].Low);
            if (Bars.Count > 1)
            {
                tr = Math.Max(tr, Math.Abs(Bars[0].High - Bars[1].Close));
                tr = Math.Max(tr, Math.Abs(Bars[0].Low - Bars[1].Close));
            }
            if (IsUpdate)
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
