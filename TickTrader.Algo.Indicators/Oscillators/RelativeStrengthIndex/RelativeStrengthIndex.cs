using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Oscillators.RelativeStrengthIndex
{
    [Indicator(Category = "Oscillators", DisplayName = "Oscillators/Relative Strength Index")]
    public class RelativeStrengthIndex : Indicator
    {
        private IMA _uMa, _dMa;

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

        [Output(DisplayName = "RSI", DefaultColor = Colors.DodgerBlue)]
        public DataSeries Rsi { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public RelativeStrengthIndex() { }

        public RelativeStrengthIndex(DataSeries<Bar> bars, int period,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            _targetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _uMa = MABase.CreateMaInstance(Period, Method.Simple);
            _uMa.Init();
            _dMa = MABase.CreateMaInstance(Period, Method.Simple);
            _dMa.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var d = 0.0;
            var u = 0.0;
            if (Bars.Count > 1)
            {
                var curAppliedPrice = AppliedPrice.Calculate(Bars[0], _targetPrice);
                var prevAppliedPrice = AppliedPrice.Calculate(Bars[1], _targetPrice);
                d = curAppliedPrice > prevAppliedPrice ? curAppliedPrice - prevAppliedPrice : 0.0;
                u = curAppliedPrice < prevAppliedPrice ? prevAppliedPrice - curAppliedPrice : 0.0;
            }
            if (IsUpdate)
            {
                _uMa.UpdateLast(u);
                _dMa.UpdateLast(d);
            }
            else
            {
                _uMa.Add(u);
                _dMa.Add(d);
            }
            var rs = _uMa.Average/_dMa.Average;
            Rsi[pos] = 100.0 - 100.0/(1.0 + rs);
        }
    }
}
