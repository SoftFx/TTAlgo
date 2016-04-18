using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.BillWilliams.AwesomeOscillator
{
    [Indicator(Category = "Bill Williams", DisplayName = "Bill Williams/Awesome Oscillator")]
    public class AwesomeOscillator : Indicator
    {
        public const int SlowSmaPeriod = 34, FastSmaPeriod = 5;

        private MovingAverage _slowSma, _fastSma;

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Value Up", DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries ValueUp { get; set; }

        [Output(DisplayName = "Value Down", DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries ValueDown { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AwesomeOscillator() { }

        public AwesomeOscillator(DataSeries<Bar> bars)
        {
            Bars = bars;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _fastSma = new MovingAverage(Bars, FastSmaPeriod, 0, Method.Simple, AppliedPrice.Target.Median);
            _slowSma = new MovingAverage(Bars, SlowSmaPeriod, 0, Method.Simple, AppliedPrice.Target.Median);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var val = _fastSma.Average[pos] - _slowSma.Average[pos];
            var prev = Bars.Count > SlowSmaPeriod
                ? (Bars.Count == SlowSmaPeriod + 1 ? 0.0 : _fastSma.Average[pos + 1] - _slowSma.Average[pos + 1])
                : double.NaN;
            if (!double.IsNaN(prev))
            {
                if (val > prev)
                {
                    ValueUp[pos] = val;
                    ValueDown[pos] = double.NaN;
                }
                else
                {
                    ValueUp[pos] = double.NaN;
                    ValueDown[pos] = val;
                }
            }
            else
            {
                ValueUp[pos] = double.NaN;
                ValueDown[pos] = double.NaN;
            }
        }
    }
}
