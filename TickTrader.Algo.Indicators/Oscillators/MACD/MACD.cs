using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.Oscillators.MACD
{
    [Indicator(Category = "Oscillators", DisplayName = "MACD", Version = "1.0")]
    public class Macd : Indicator, IMacd
    {
        private IMovingAverage _fastEma, _slowEma;
        private IMA _macdSma;

        [Parameter(DefaultValue = 12, DisplayName = "Fast EMA")]
        public int FastEma { get; set; }

        [Parameter(DefaultValue = 26, DisplayName = "Slow EMA")]
        public int SlowEma { get; set; }

        [Parameter(DefaultValue = 9, DisplayName = "MACD SMA")]
        public int MacdSma { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "MACD", Target = OutputTargets.Window1, DefaultColor = Colors.Silver, PlotType = PlotType.Histogram)]
        public DataSeries MacdSeries { get; set; }

        [Output(DisplayName = "Signal", Target = OutputTargets.Window1, DefaultColor = Colors.Red, DefaultLineStyle = LineStyles.DotsRare)]
        public DataSeries Signal { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public Macd() { }

        public Macd(DataSeries price, int fastEma, int slowEma, int macdSma)
        {
            Price = price;
            FastEma = fastEma;
            SlowEma = slowEma;
            MacdSma = macdSma;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _fastEma = Indicators.MovingAverage(Price, FastEma, 0, MovingAverageMethod.Exponential);
            _slowEma = Indicators.MovingAverage(Price, SlowEma, 0, MovingAverageMethod.Exponential);
            _macdSma = MABase.CreateMaInstance(MacdSma, MovingAverageMethod.Simple);
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
