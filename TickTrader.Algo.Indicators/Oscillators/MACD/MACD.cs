using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.MACD
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/MACD")]
    public class Macd : Indicator
    {
        private MovingAverage _fastEma, _slowEma;
        private IMA _macdSma;

        [Parameter(DefaultValue = 12, DisplayName = "Fast EMA")]
        public int FastEma { get; set; }

        [Parameter(DefaultValue = 26, DisplayName = "Slow EMA")]
        public int SlowEma { get; set; }

        [Parameter(DefaultValue = 9, DisplayName = "MACD SMA")]
        public int MacdSma { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "MACD", DefaultColor = Colors.Silver, PlotType = PlotType.Histogram)]
        public DataSeries MacdSeries { get; set; }

        [Output(DisplayName = "Signal", DefaultColor = Colors.Red, DefaultLineStyle = LineStyles.Lines)]
        public DataSeries Signal { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public Macd() { }

        public Macd(DataSeries<Bar> bars, int fastEma, int slowEma, int macdSma,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            FastEma = fastEma;
            SlowEma = slowEma;
            MacdSma = macdSma;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _fastEma = new MovingAverage(Bars, FastEma, 0, Method.Exponential, TargetPrice);
            _slowEma = new MovingAverage(Bars, SlowEma, 0, Method.Exponential, TargetPrice);
            _macdSma = MABase.CreateMaInstance(MacdSma, Method.Simple);
            _macdSma.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            MacdSeries[pos] = _fastEma.Average[pos] - _slowEma.Average[pos];
            if (IsUpdate)
            {
                _macdSma.UpdateLast(MacdSeries[pos]);
            }
            else
            {
                _macdSma.Add(MacdSeries[pos]);
            }
            Signal[pos] = _macdSma.Average;
        }
    }
}
