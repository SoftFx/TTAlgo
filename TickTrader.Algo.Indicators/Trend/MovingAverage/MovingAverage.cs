using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Moving Average")]
    public class MovingAverage : Indicator
    {
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

        private IMA _maInstance;
        private IShift _shifter;

        protected override void Init()
        {
            _maInstance = MABase.CreateMaInstance(Period, _targetMethod, SmoothFactor);
            _maInstance.Init();
            _shifter = new SimpleShifter(Shift);
            _shifter.Init();
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
