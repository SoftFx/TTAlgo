using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;

namespace TickTrader.Algo.Indicators.BillWilliams.AcceleratorOscillator
{
    [Indicator(Category = "Bill Williams", DisplayName = "Accelerator Oscillator", Version = "1.0")]
    public class AcceleratorOscillator : Indicator
    {
        private AwesomeOscillator.AwesomeOscillator _ao;
        private IMA _aoSma;

        [Parameter(DisplayName = "Fast SMA Period", DefaultValue = 5)]
        public int FastSmaPeriod { get; set; }

        [Parameter(DisplayName = "Slow SMA Period", DefaultValue = 34)]
        public int SlowSmaPeriod { get; set; }

        [Parameter(DisplayName = "Data Limit", DefaultValue = 34)]
        public int DataLimit { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Value Up", DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries ValueUp { get; set; }

        [Output(DisplayName = "Value Down", DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries ValueDown { get; set; }

        public int LastPositionChanged { get { return _ao.LastPositionChanged; } }

        public AcceleratorOscillator() { }

        public AcceleratorOscillator(BarSeries bars, int fastSmaPeriod, int slowSmaPeriod, int dataLimit)
        {
            Bars = bars;
            FastSmaPeriod = fastSmaPeriod;
            SlowSmaPeriod = slowSmaPeriod;
            DataLimit = dataLimit;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            _ao = new AwesomeOscillator.AwesomeOscillator(Bars, FastSmaPeriod, SlowSmaPeriod, DataLimit);
            _aoSma = MABase.CreateMaInstance(FastSmaPeriod, Method.Simple);
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
                FastSmaPeriod + SlowSmaPeriod + Math.Max(0, DataLimit - Math.Max(SlowSmaPeriod, FastSmaPeriod)))
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
