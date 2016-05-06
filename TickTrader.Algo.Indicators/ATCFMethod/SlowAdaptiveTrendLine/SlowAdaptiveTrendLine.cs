using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.SlowAdaptiveTrendLine
{
    [Indicator(IsOverlay = true, Category = "AT&CF Method", DisplayName = "AT&CF Method/Slow Adaptive Trend Line")]
    public class SlowAdaptiveTrendLine : Indicator
    {
        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "SATL", DefaultColor = Colors.Cornsilk)]
        public DataSeries Satl { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public SlowAdaptiveTrendLine() { }

        public SlowAdaptiveTrendLine(DataSeries price)
        {
            Price = price;

            InitializeIndicator();
        }

        private void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            
        }
    }
}
