using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.PerfectCommodityChannelIndex
{
    [Indicator(Category = "AT&CF Method", DisplayName = "AT&CF Method/Perfect Commodity Channel Index")]
    public class PerfectCommodityChannelIndex : Indicator
    {
        private FastAdaptiveTrendLine.FastAdaptiveTrendLine _fatl;

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "PCCI", DefaultColor = Colors.Red)]
        public DataSeries Pcci { get; set; }

        public int LastPositionChanged { get { return _fatl.LastPositionChanged; } }

        public PerfectCommodityChannelIndex() { }

        public PerfectCommodityChannelIndex(DataSeries price)
        {
            Price = price;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _fatl = new FastAdaptiveTrendLine.FastAdaptiveTrendLine(Price);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Pcci[pos] = Price[pos] - _fatl.Fatl[pos];
        }
    }
}
