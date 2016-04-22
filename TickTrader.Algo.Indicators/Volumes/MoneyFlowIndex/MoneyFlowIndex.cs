using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Volumes.MoneyFlowIndex
{
    [Indicator(Category = "Volumes", DisplayName = "Volumes/Money Flow Index")]
    public class MoneyFlowIndex : Indicator
    {
        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "MFI", DefaultColor = Colors.DodgerBlue)]
        public DataSeries Mfi { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public MoneyFlowIndex() { }

        public MoneyFlowIndex(BarSeries bars, int period)
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
            
        }
    }
}
