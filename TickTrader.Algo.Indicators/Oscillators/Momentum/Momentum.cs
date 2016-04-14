using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.Momentum
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Momentum")]
    public class Momentum : Indicator
    {
        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        private AppliedPrice.Target _targetPrice;
        [Parameter(DefaultValue = 0, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int)_targetPrice; }
            set { _targetPrice = (AppliedPrice.Target)value; }
        }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Momentum", DefaultColor = Colors.DodgerBlue)]
        public DataSeries Moment { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public Momentum() { }

        public Momentum(DataSeries<Bar> bars, int period, AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            _targetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            if (Bars.Count > pos + Period)
            {
                Moment[pos] = AppliedPrice.Calculate(Bars[pos], _targetPrice)/
                              AppliedPrice.Calculate(Bars[pos + Period], _targetPrice)*100;
            }
            else
            {
                Moment[pos] = double.NaN;
            }
        }
    }
}
