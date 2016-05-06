using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.FastAdaptiveTrendLine
{
    [Indicator(IsOverlay = true, Category = "AT&CF Method", DisplayName = "AT&CF Method/Fast Adaptive Trend Line")]
    public class FastAdaptiveTrendLine : Indicator
    {
        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "FATL", DefaultColor = Colors.Red)]
        public DataSeries Fatl { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public FastAdaptiveTrendLine() { }

        public FastAdaptiveTrendLine(DataSeries price)
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
