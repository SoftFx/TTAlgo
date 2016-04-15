using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.BullsPower
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Bulls Power")]
    public class BullsPower : Indicator
    {
        private MovingAverage _ema;

        [Parameter(DefaultValue = 13, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Bulls", DefaultColor = Colors.Silver)]
        public DataSeries Bulls { get; set; }

        public int LastPositionChanged { get { return _ema.LastPositionChanged; } }

        public BullsPower() { }

        public BullsPower(DataSeries<Bar> bars, int period, AppliedPrice.Target targetPrice)
        {
            Bars = bars;
            Period = period;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _ema = new MovingAverage(Bars, Period, 0, Method.Exponential, TargetPrice);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            Bulls[pos] = Bars[pos].High - _ema.Average[pos];
        }
    }
}
