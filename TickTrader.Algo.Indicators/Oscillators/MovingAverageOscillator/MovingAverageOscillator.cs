using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Oscillators.MACD;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.MovingAverageOscillator
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Moving Average of Oscillator")]
    public class MovingAverageOscillator : Indicator
    {
        private Macd _macd;

        [Parameter(DefaultValue = 12, DisplayName = "Fast EMA")]
        public int FastEma { get; set; }

        [Parameter(DefaultValue = 26, DisplayName = "Slow EMA")]
        public int SlowEma { get; set; }

        [Parameter(DefaultValue = 9, DisplayName = "MACD SMA")]
        public int MacdSma { get; set; }

        private AppliedPrice.Target _targetPrice;
        [Parameter(DefaultValue = 0, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int)_targetPrice; }
            set { _targetPrice = (AppliedPrice.Target)value; }
        }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "OsMA", DefaultColor = Colors.Silver, PlotType = PlotType.Histogram)]
        public DataSeries OsMa { get; set; }

        public int LastPositionChanged { get { return _macd.LastPositionChanged; } }

        public MovingAverageOscillator() { }

        public MovingAverageOscillator(DataSeries<Bar> bars, int fastEma, int slowEma, int macdSma,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            FastEma = fastEma;
            SlowEma = slowEma;
            MacdSma = macdSma;
            _targetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _macd = new Macd(Bars, FastEma, SlowEma, MacdSma, _targetPrice);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            OsMa[pos] = _macd.MacdSeries[pos] - (double.IsNaN(_macd.Signal[pos]) ? 0.0 : _macd.Signal[pos]);
        }
    }
}
