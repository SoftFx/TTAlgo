using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.PerfectCommodityChannelIndex
{
    [Indicator(Category = "AT&CF Method", DisplayName = "AT&CF Method/Perfect Commodity Channel Index")]
    public class PerfectCommodityChannelIndex : Indicator
    {
        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "PCCI", DefaultColor = Colors.Red)]
        public DataSeries Pcci { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public PerfectCommodityChannelIndex() { }

        public PerfectCommodityChannelIndex(DataSeries price)
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
