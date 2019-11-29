using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.WilliamsPercentRange
{
    [Indicator(Category = "Oscillators", DisplayName = "Williams Percent Range", Version = "1.0")]
    public class WilliamsPercentRange : Indicator, IWilliamsPercentRange
    {
        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "WPR", Target = OutputTargets.Window1, DefaultColor = Colors.Aqua)]
        public DataSeries Wpr { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public WilliamsPercentRange() { }

        public WilliamsPercentRange(BarSeries bars, int period)
        {
            Bars = bars;
            Period = period;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            Wpr[pos] = (PeriodHelper.FindMax(Bars.High, Period) - Bars.Close[pos])/
                       (PeriodHelper.FindMax(Bars.High, Period) - PeriodHelper.FindMin(Bars.Low, Period))*(-100.0);
        }
    }
}
