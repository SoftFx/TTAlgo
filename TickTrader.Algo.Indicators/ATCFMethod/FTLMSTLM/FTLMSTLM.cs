using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.FTLMSTLM
{
    [Indicator(Category = "AT&CF Method", DisplayName = "FTLM-STLM", Version = "1.0")]
    public class FtlmStlm : Indicator
    {
        private FastTrendLineMomentum.FastTrendLineMomentum _ftlm;
        private SlowTrendLineMomentum.SlowTrendLineMomentum _stlm;

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "FTLM", Target = OutputTargets.Window1, DefaultColor = Colors.DarkKhaki, Precision = 6)]
        public DataSeries Ftlm { get; set; }

        [Output(DisplayName = "STLM", Target = OutputTargets.Window1, DefaultColor = Colors.DarkSalmon, Precision = 6)]
        public DataSeries Stlm { get; set; }

        public int LastPositionChanged { get { return _ftlm.LastPositionChanged; } }

        public FtlmStlm() { }

        public FtlmStlm(DataSeries price)
        {
            Price = price;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _ftlm = new FastTrendLineMomentum.FastTrendLineMomentum(Price);
            _stlm = new SlowTrendLineMomentum.SlowTrendLineMomentum(Price);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Ftlm[pos] = _ftlm.Ftlm[pos];
            Stlm[pos] = _stlm.Stlm[pos];
            if (Price.Count > CountBars)
            {
                Ftlm[CountBars] = double.NaN;
                Stlm[CountBars] = double.NaN;
            }
        }
    }
}
