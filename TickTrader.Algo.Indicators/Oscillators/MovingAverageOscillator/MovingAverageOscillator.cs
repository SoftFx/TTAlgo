using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Oscillators.MACD;

namespace TickTrader.Algo.Indicators.Oscillators.MovingAverageOscillator
{
    [Indicator(Category = "Oscillators", DisplayName = "Moving Average of Oscillator", Version = "1.0")]
    public class MovingAverageOscillator : Indicator, IMovingAverageOscillator
    {
        private Macd _macd;

        [Parameter(DefaultValue = 12, DisplayName = "Fast EMA")]
        public int FastEma { get; set; }

        [Parameter(DefaultValue = 26, DisplayName = "Slow EMA")]
        public int SlowEma { get; set; }

        [Parameter(DefaultValue = 9, DisplayName = "MACD SMA")]
        public int MacdSma { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "OsMA", Target = OutputTargets.Window1, DefaultColor = Colors.Silver, PlotType = PlotType.Histogram)]
        public DataSeries OsMa { get; set; }

        public int LastPositionChanged { get { return _macd.LastPositionChanged; } }

        public MovingAverageOscillator() { }

        public MovingAverageOscillator(DataSeries price, int fastEma, int slowEma, int macdSma)
        {
            Price = price;
            FastEma = fastEma;
            SlowEma = slowEma;
            MacdSma = macdSma;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _macd = new Macd(Price, FastEma, SlowEma, MacdSma);
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
