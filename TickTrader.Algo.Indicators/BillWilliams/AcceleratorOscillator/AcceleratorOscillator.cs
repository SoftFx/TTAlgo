using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.BillWilliams.AcceleratorOscillator
{
    [Indicator(Category = "Bill Williams", DisplayName = "Bill Williams/Accelerator Oscillator")]
    public class AcceleratorOscillator : Indicator
    {
        private AwesomeOscillator.AwesomeOscillator _ao;
        private IMA _aoSma;

        [Input]
        public BarSeries Bars { get; set; }

        [Output(DisplayName = "Value Up", DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries ValueUp { get; set; }

        [Output(DisplayName = "Value Down", DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries ValueDown { get; set; }

        public int LastPositionChanged { get { return _ao.LastPositionChanged; } }

        public AcceleratorOscillator() { }

        public AcceleratorOscillator(BarSeries bars)
        {
            Bars = bars;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _ao = new AwesomeOscillator.AwesomeOscillator(Bars);
            _aoSma = MABase.CreateMaInstance(AwesomeOscillator.AwesomeOscillator.FastSmaPeriod, Method.Simple);
            _aoSma.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var ao = double.IsNaN(_ao.ValueUp[pos]) ? _ao.ValueDown[pos] : _ao.ValueUp[pos];
            if (!double.IsNaN(ao))
            {
                if (IsUpdate)
                {
                    _aoSma.UpdateLast(ao);
                }
                else
                {
                    _aoSma.Add(ao);
                }
            }
            var val = ao - _aoSma.Average;
            var prev = Bars.Count > 1
                ? (double.IsNaN(ValueUp[pos + 1]) ? ValueDown[pos + 1] : ValueUp[pos + 1])
                : double.NaN;
            if (Bars.Count ==
                AwesomeOscillator.AwesomeOscillator.FastSmaPeriod + AwesomeOscillator.AwesomeOscillator.SlowSmaPeriod)
            {
                prev = 0.0;
            }
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
