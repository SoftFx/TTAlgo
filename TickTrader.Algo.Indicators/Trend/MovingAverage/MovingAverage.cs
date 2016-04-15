using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Moving Average")]
    public class MovingAverage : Indicator
    {
        private IMA _maInstance;
        private IShift _shifter;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = Method.Simple, DisplayName = "Method")]
        public Method TargetMethod { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Parameter(DefaultValue = 0.0667, DisplayName = "Smooth Factor(CustomEMA)")]
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
            TargetMethod = targetMethod;
            TargetPrice = targetPrice;
            SmoothFactor = double.IsNaN(smoothFactor) ? 2.0/(period + 1) : smoothFactor;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _maInstance = MABase.CreateMaInstance(Period, TargetMethod, SmoothFactor);
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
                _maInstance.UpdateLast(AppliedPrice.Calculate(Bars[0], TargetPrice));
                _shifter.UpdateLast(_maInstance.Average);
            }
            else
            {
                _maInstance.Add(AppliedPrice.Calculate(Bars[0], TargetPrice));
                _shifter.Add(_maInstance.Average);
            }
            Average[_shifter.Position] = _shifter.Result;
        }
    }
}
