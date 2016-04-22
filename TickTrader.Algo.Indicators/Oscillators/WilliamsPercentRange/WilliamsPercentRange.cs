using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.WilliamsPercentRange
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Williams Percent Range")]
    public class WilliamsPercentRange : Indicator
    {
        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "WPR", DefaultColor = Colors.Aqua)]
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

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Wpr[pos] = (PeriodHelper.FindMax(Bars.High, Period) - Bars.Close[pos])/
                       (PeriodHelper.FindMax(Bars.High, Period) - PeriodHelper.FindMin(Bars.Low, Period))*(-100.0);
        }
    }
}
