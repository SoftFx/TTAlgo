using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.ATCFMethod.FATLSignal
{
    [Indicator(Category = "AT&CF Method", DisplayName = "FATLs", Version = "1.0")]
    public class FatlSignal : Indicator, IFATLSignal
    {
        private IFastAdaptiveTrendLine _fatl;
        private bool _trend, _prevTrend;

        [Parameter(DefaultValue = 300, DisplayName = "CountBars")]
        public int CountBars { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Close, DisplayName = "Apply To")]
        public AppliedPrice TargetPrice { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        public DataSeries Price { get; private set; }

        [Output(DisplayName = "Up", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue)]
        public DataSeries Up { get; set; }

        [Output(DisplayName = "Down", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries Down { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public FatlSignal() { }

        public FatlSignal(BarSeries bars, int countBars, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            Bars = bars;
            CountBars = countBars;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        public bool HasEnoughBars(int barsCount)
        {
            return _fatl.HasEnoughBars(barsCount);
        }

        private void InitializeIndicator()
        {
            Price = AppliedPriceHelper.GetDataSeries(Bars, TargetPrice);
            _fatl = Indicators.FastAdaptiveTrendLine(Price, CountBars);
            _prevTrend = false;
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            Up[pos] = double.NaN;
            Down[pos] = double.NaN;
            if (isNewBar)
            {
                _prevTrend = _trend;
            }
            if (HasEnoughBars(Bars.Count))
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
                        Up[pos] = Bars.Low[pos] - 5*Symbol.Point;
                    }
                    else
                    {
                        Down[pos] = Bars.High[pos] + 5*Symbol.Point;
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
