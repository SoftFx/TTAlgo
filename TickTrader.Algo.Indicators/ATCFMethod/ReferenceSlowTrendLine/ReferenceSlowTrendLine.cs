using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.ReferenceSlowTrendLine
{
    [Indicator(IsOverlay = true, Category = "AT&CF Method", DisplayName = "AT&CF Method/Reference Slow Trend Line")]
    public class ReferenceSlowTrendLine : Indicator
    {
        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "RSTL", DefaultColor = Colors.DeepSkyBlue)]
        public DataSeries Rstl { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public ReferenceSlowTrendLine() { }

        public ReferenceSlowTrendLine(DataSeries price)
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
