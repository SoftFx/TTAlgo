using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.ATCFMethod.FastTrendLineMomentum
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Fast Trend Line Momentum", Version = "1.0")]
    public class FastTrendLineMomentum : Indicator, IFastTrendLineMomentum
    {
        private FastAdaptiveTrendLine.FastAdaptiveTrendLine _fatl;
        private ReferenceFastTrendLine.ReferenceFastTrendLine _rftl;

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "FTLM", Target = OutputTargets.Window1, DefaultColor = Colors.DarkKhaki)]
        public DataSeries Ftlm { get; set; }

        public int LastPositionChanged { get { return _fatl.LastPositionChanged; } }

        public FastTrendLineMomentum() { }

        public FastTrendLineMomentum(DataSeries price, int countBars)
        {
            Price = price;
            CountBars = countBars;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _fatl = new FastAdaptiveTrendLine.FastAdaptiveTrendLine(Price, CountBars);
            _rftl = new ReferenceFastTrendLine.ReferenceFastTrendLine(Price, CountBars);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Ftlm[pos] = _fatl.Fatl[pos] - _rftl.Rftl[pos];
            if (Price.Count > CountBars)
            {
                Ftlm[CountBars] = double.NaN;
            }
        }
    }
}
