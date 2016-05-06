using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.FastTrendLineMomentum
{
    [Indicator(Category = "AT&CF Method", DisplayName = "AT&CF Method/Fast Trend Line Momentum")]
    public class FastTrendLineMomentum : Indicator
    {
        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "FTLM", DefaultColor = Colors.DarkKhaki)]
        public DataSeries Ftlm { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public FastTrendLineMomentum() { }

        public FastTrendLineMomentum(DataSeries price)
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
