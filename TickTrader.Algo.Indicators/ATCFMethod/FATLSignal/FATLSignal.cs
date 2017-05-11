using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.ATCFMethod.FATLSignal
{
    [Indicator(Category = "AT&CF Method", DisplayName = "FATLs", Version = "1.0")]
    public class FatlSignal : Indicator
    {
        private FastAdaptiveTrendLine.FastAdaptiveTrendLine _fatl;
        private bool _trend, _prevTrend;

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Parameter(DisplayName = "Point Size", DefaultValue = 1e-5)]
        public double PointSize { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        public DataSeries Price { get; private set; }

        [Output(DisplayName = "Up", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue)]
        public DataSeries Up { get; set; }

        [Output(DisplayName = "Down", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries Down { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public FatlSignal() { }

        public FatlSignal(BarSeries bars, double pointSize, AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            PointSize = pointSize;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {
            Price = AppliedPrice.GetDataSeries(Bars, TargetPrice);
            _fatl = new FastAdaptiveTrendLine.FastAdaptiveTrendLine(Price);
            _prevTrend = false;
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Up[pos] = double.NaN;
            Down[pos] = double.NaN;
            if (!IsUpdate)
            {
                _prevTrend = _trend;
            }
            if (Bars.Count > _fatl.CoefficientsCount - 1)
            {
                var tmp = _fatl.Fatl[pos] - _fatl.Fatl[pos + 1];
                if (!double.IsNaN(tmp))
                {
                    if (tmp > 0)
                    {
                        _trend = true;
                    }
                    if (tmp < 0)
                    {
                        _trend = false;
                    }
                }
                if (_trend != _prevTrend)
                {
                    if (_trend)
                    {
                        Up[pos] = Bars.Low[pos] - 5*PointSize;
                    }
                    else
                    {
                        Down[pos] = Bars.High[pos] + 5*PointSize;
                    }
                }
            }
            if (Price.Count > CountBars)
            {
                Up[CountBars] = double.NaN;
                Down[CountBars] = double.NaN;
            }
        }
    }
}
