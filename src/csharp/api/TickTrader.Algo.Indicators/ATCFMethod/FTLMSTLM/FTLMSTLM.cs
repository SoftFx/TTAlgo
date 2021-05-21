using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.ATCFMethod.FTLMSTLM
{
    [Indicator(Category = "AT&CF Method", DisplayName = "FTLM-STLM", Version = "1.0")]
    public class FtlmStlm : Indicator, IFTLMSTLM
    {
        private IFastTrendLineMomentum _ftlm;
        private ISlowTrendLineMomentum _stlm;

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

        public FtlmStlm(DataSeries price, int countBars)
        {
            Price = price;
            CountBars = countBars;

            InitializeIndicator();
        }

        public bool HasEnoughBars(int barsCount)
        {
            return _ftlm.HasEnoughBars(barsCount) && _stlm.HasEnoughBars(barsCount);
        }

        private void InitializeIndicator()
        {
            _ftlm = Indicators.FastTrendLineMomentum(Price, CountBars);
            _stlm = Indicators.SlowTrendLineMomentum(Price, CountBars);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
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
