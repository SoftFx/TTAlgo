using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.BullsPower
{
    [Indicator(Category = "Oscillators", DisplayName = "Bulls Power", Version = "1.1")]
    public class BullsPower : Indicator, IBullsPower
    {
        private IMovingAverage _ema;

        [Parameter(DefaultValue = 13, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Close, DisplayName = "Apply To")]
        public AppliedPrice TargetPrice { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Bulls", Target = OutputTargets.Window1, DefaultColor = Colors.Silver, PlotType = PlotType.Histogram)]
        public DataSeries Bulls { get; set; }

        public int LastPositionChanged { get { return _ema.LastPositionChanged; } }

        public BullsPower() { }

        public BullsPower(BarSeries bars, int period, AppliedPrice targetPrice = AppliedPrice.Close)
        {
            Bars = bars;
            Period = period;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _ema = Indicators.MovingAverage(AppliedPriceHelper.GetDataSeries(Bars, TargetPrice), Period, 0, MovingAverageMethod.Exponential);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            Bulls[pos] = Bars.High[pos] - _ema.Average[pos];
        }
    }
}
