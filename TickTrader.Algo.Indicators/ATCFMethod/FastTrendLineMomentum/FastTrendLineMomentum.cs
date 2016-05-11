using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.FastTrendLineMomentum
{
    [Indicator(Category = "AT&CF Method", DisplayName = "AT&CF Method/Fast Trend Line Momentum")]
    public class FastTrendLineMomentum : Indicator
    {
        private FastAdaptiveTrendLine.FastAdaptiveTrendLine _fatl;
        private ReferenceFastTrendLine.ReferenceFastTrendLine _rftl;

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "FTLM", DefaultColor = Colors.DarkKhaki)]
        public DataSeries Ftlm { get; set; }

        public int LastPositionChanged { get { return _fatl.LastPositionChanged; } }

        public FastTrendLineMomentum() { }

        public FastTrendLineMomentum(DataSeries price)
        {
            Price = price;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _fatl = new FastAdaptiveTrendLine.FastAdaptiveTrendLine(Price);
            _rftl = new ReferenceFastTrendLine.ReferenceFastTrendLine(Price);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Ftlm[pos] = _fatl.Fatl[pos] - _rftl.Rftl[pos];
        }
    }
}
