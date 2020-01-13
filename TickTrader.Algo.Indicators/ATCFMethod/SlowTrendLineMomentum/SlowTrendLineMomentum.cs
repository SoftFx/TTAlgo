using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.ATCFMethod.SlowTrendLineMomentum
{
    [Indicator(Category = "AT&CF Method", DisplayName = "Slow Trend Line Momentum", Version = "1.0")]
    public class SlowTrendLineMomentum : Indicator, ISlowTrendLineMomentum
    {
        private ISlowAdaptiveTrendLine _satl;
        private IReferenceSlowTrendLine _rstl;

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "STLM", Target = OutputTargets.Window1, DefaultColor = Colors.DarkSalmon)]
        public DataSeries Stlm { get; set; }

        public int LastPositionChanged { get { return _satl.LastPositionChanged; } }

        public SlowTrendLineMomentum() { }

        public SlowTrendLineMomentum(DataSeries price, int countBars)
        {
            Price = price;
            CountBars = countBars;

            InitializeIndicator();
        }

        public bool HasEnoughBars(int barsCount)
        {
            return _satl.HasEnoughBars(barsCount) && _rstl.HasEnoughBars(barsCount);
        }

        private void InitializeIndicator()
        {
            _satl = Indicators.SlowAdaptiveTrendLine(Price, CountBars);
            _rstl = Indicators.ReferenceSlowTrendLine(Price, CountBars);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
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
