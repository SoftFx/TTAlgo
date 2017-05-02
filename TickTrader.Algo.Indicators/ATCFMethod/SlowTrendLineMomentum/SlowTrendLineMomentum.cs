using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.SlowTrendLineMomentum
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Slow Trend Line Momentum", Version = "1.0")]
    public class SlowTrendLineMomentum : Indicator
    {
        private SlowAdaptiveTrendLine.SlowAdaptiveTrendLine _satl;
        private ReferenceSlowTrendLine.ReferenceSlowTrendLine _rstl;

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "STLM", Target = OutputTargets.Window1, DefaultColor = Colors.DarkSalmon)]
        public DataSeries Stlm { get; set; }

        public int LastPositionChanged { get { return _satl.LastPositionChanged; } }

        public SlowTrendLineMomentum() { }

        public SlowTrendLineMomentum(DataSeries price)
        {
            Price = price;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _satl = new SlowAdaptiveTrendLine.SlowAdaptiveTrendLine(Price);
            _rstl = new ReferenceSlowTrendLine.ReferenceSlowTrendLine(Price);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Stlm[pos] = _satl.Satl[pos] - _rstl.Rstl[pos];
            if (Price.Count > CountBars)
            {
                Stlm[CountBars] = double.NaN;
            }
        }
    }
}
