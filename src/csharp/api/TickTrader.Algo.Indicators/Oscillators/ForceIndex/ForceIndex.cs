using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.ForceIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Force Index", Version = "1.0")]
    public class ForceIndex : Indicator, IForceIndex
    {
        private IMovingAverage _ma;

        [Parameter(DefaultValue = 13, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = MovingAverageMethod.Simple, DisplayName = "Method")]
        public MovingAverageMethod TargetMethod { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Close, DisplayName = "Apply To")]
        public AppliedPrice TargetPrice { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Force", Target = OutputTargets.Window1, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Force { get; set; }

        public int LastPositionChanged { get { return _ma.LastPositionChanged; } }

        public ForceIndex() { }

        public ForceIndex(BarSeries bars, int period, MovingAverageMethod targetMethod = MovingAverageMethod.Simple,
            AppliedPrice targetPrice = AppliedPrice.Close)
        {
            Bars = bars;
            Period = period;
            TargetMethod = targetMethod;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _ma = Indicators.MovingAverage(AppliedPriceHelper.GetDataSeries(Bars, TargetPrice), Period, 0, TargetMethod);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            if (Bars.Count > pos + 1)
            {
                Force[pos] = Bars.Volume[pos]*(_ma.Average[pos] - _ma.Average[pos + 1]);
            }
            else
            {
                Force[pos] = double.NaN;
            }
        }
    }
}
