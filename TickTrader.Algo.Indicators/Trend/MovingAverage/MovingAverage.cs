using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Moving Average")]
    public class MovingAverage : Indicator
    {
        private IMA _maInstance;
        private IShift _shifter;

        [Parameter(DefaultValue = 7, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        private Method _targetMethod;
        [Parameter(DefaultValue = 0, DisplayName = "Method")]
        public int TargetMethod
        {
            get { return (int) _targetMethod; }
            set { _targetMethod = (Method) value; }
        }

        private AppliedPrice.Target _targetPrice;
        [Parameter(DefaultValue = 0, DisplayName = "Apply To")]
        public int TargetPrice
        {
            get { return (int) _targetPrice; }
            set { _targetPrice = (AppliedPrice.Target) value; }
        }

        [Parameter(DefaultValue = 0.25, DisplayName = "Smooth Factor(CustomEMA)")]
        public double SmoothFactor { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output]
        public DataSeries Average { get; set; }

        public int LastPositionChanged { get { return _shifter.Position; } }

        public MovingAverage() { }

        public MovingAverage(DataSeries<Bar> bars, int period, int shift, Method targetMethod = Method.Simple,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close, double smoothFactor = double.NaN)
        {
            Bars = bars;
            Period = period;
            Shift = shift;
            _targetMethod = targetMethod;
            _targetPrice = targetPrice;
            SmoothFactor = double.IsNaN(smoothFactor) ? 2.0/(period + 1) : smoothFactor;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _maInstance = MABase.CreateMaInstance(Period, _targetMethod, SmoothFactor);
            _maInstance.Init();
            _shifter = new SimpleShifter(Shift);
            _shifter.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            if (IsUpdate)
            {
                _maInstance.UpdateLast(AppliedPrice.Calculate(Bars[0], _targetPrice));
                _shifter.UpdateLast(_maInstance.Average);
            }
            else
            {
                _maInstance.Add(AppliedPrice.Calculate(Bars[0], _targetPrice));
                _shifter.Add(_maInstance.Average);
            }
            Average[_shifter.Position] = _shifter.Result;
        }
    }
}
