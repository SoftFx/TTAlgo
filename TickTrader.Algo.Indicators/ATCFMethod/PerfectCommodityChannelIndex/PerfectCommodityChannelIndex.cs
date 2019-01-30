using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.ATCFMethod.PerfectCommodityChannelIndex
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Perfect Commodity Channel Index", Version = "1.0")]
    public class PerfectCommodityChannelIndex : Indicator, IPerfectCommodityChannelIndex
    {
        private IFastAdaptiveTrendLine _fatl;

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "PCCI", Target = OutputTargets.Window1, DefaultColor = Colors.Red)]
        public DataSeries Pcci { get; set; }

        public int LastPositionChanged { get { return _fatl.LastPositionChanged; } }

        public PerfectCommodityChannelIndex() { }

        public PerfectCommodityChannelIndex(DataSeries price, int countBars)
        {
            Price = price;
            CountBars = countBars;

            InitializeIndicator();
        }

        public bool HasEnoughBars(int barsCount)
        {
            return _fatl.HasEnoughBars(barsCount);
        }

        private void InitializeIndicator()
        {
            _fatl = Indicators.FastAdaptiveTrendLine(Price, CountBars);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Pcci[pos] = Price[pos] - _fatl.Fatl[pos];
            if (Price.Count > CountBars)
            {
                Pcci[CountBars] = double.NaN;
            }
        }
    }
}
