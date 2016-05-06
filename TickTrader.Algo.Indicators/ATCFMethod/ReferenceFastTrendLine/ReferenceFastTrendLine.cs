using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.ReferenceFastTrendLine
{
    [Indicator(IsOverlay = true, Category = "AT&CF Method", DisplayName = "AT&CF Method/Reference Fast Trend Line")]
    public class ReferenceFastTrendLine : Indicator
    {
        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "RFTL", DefaultColor = Colors.Blue)]
        public DataSeries Rftl { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public ReferenceFastTrendLine() { }

        public ReferenceFastTrendLine(DataSeries price)
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
