using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.ForceIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Force Index", Version = "1.0")]
    public class ForceIndex : Indicator
    {
        private MovingAverage _ma;

        [Parameter(DefaultValue = 13, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = Method.Simple, DisplayName = "Method")]
        public Method TargetMethod { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Force", Target = OutputTargets.Window1, DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Force { get; set; }

        public int LastPositionChanged { get { return _ma.LastPositionChanged; } }

        public ForceIndex() { }

        public ForceIndex(BarSeries bars, int period, Method targetMethod = Method.Simple,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            TargetMethod = targetMethod;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _ma = new MovingAverage(AppliedPrice.GetDataSeries(Bars, TargetPrice), Period, 0, TargetMethod);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
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
