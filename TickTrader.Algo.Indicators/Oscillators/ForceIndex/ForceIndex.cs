using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.ForceIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Force Index")]
    public class ForceIndex : Indicator
    {
        private MovingAverage _ma;

        [Parameter(DefaultValue = 13, DisplayName = "Period")]
        public int Period { get; set; }

        private Method _targetMethod;
        [Parameter(DefaultValue = 0, DisplayName = "Method")]
        public int TargetMethod
        {
            get { return (int)_targetMethod; }
            set { _targetMethod = (Method)value; }
        }

        private AppliedPrice.Target _targetPrice;
        [Parameter(DefaultValue = 0, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int)_targetPrice; }
            set { _targetPrice = (AppliedPrice.Target)value; }
        }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Force", DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Force { get; set; }

        public int LastPositionChanged { get { return _ma.LastPositionChanged; } }

        public ForceIndex() { }

        public ForceIndex(DataSeries<Bar> bars, int period, Method targetMethod = Method.Simple,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            _targetMethod = targetMethod;
            _targetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _ma = new MovingAverage(Bars, Period, 0, _targetMethod, _targetPrice);
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
                Force[pos] = Bars[pos].Volume*(_ma.Average[pos] - _ma.Average[pos + 1]);
            }
            else
            {
                Force[pos] = double.NaN;
            }
        }
    }
}
