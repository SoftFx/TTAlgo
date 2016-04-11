using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.Envelopes
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Envelopes")]
    public class Envelopes : Indicator
    {
        private MovingAverage.MovingAverage _middleLine;
        
        [Parameter(DefaultValue = 7, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = 0.25, DisplayName = "Deviation(%)")]
        public double Deviation { get; set; }

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

        [Output(DefaultColor = Colors.Blue)]
        public DataSeries TopLine { get; set; }

        [Output(DefaultColor = Colors.Red)]
        public DataSeries BottomLine { get; set; }

        public int LastPositionChanged { get { return _middleLine.LastPositionChanged; } }

        public Envelopes() { }

        public Envelopes(DataSeries<Bar> bars, int period, int shift, Method targetMethod = Method.Simple,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            Shift = shift;
            _targetMethod = targetMethod;
            _targetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _middleLine = new MovingAverage.MovingAverage(Bars, Period, Shift, _targetMethod, _targetPrice);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var val = _middleLine.Average[pos];
            TopLine[pos] = val*(1.0 + Deviation/100);
            BottomLine[pos] = val*(1.0 - Deviation/100);
        }
    }
}
