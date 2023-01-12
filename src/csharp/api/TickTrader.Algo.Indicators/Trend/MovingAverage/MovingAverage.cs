using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.MovingAverage
{
    [Indicator(Category = "Trend", DisplayName = "Moving Average", Version = "1.1")]
    public class MovingAverage : Indicator, IMovingAverage
    {
        private IMovAvgAlgo _maInstance;
        private IShift _shifter;

        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = MovingAverageMethod.Simple, DisplayName = "Method")]
        public MovingAverageMethod TargetMethod { get; set; }

        [Parameter(DefaultValue = 0.0667, DisplayName = "Smooth Factor(CustomEMA)")]
        public double SmoothFactor { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "Average", Target = OutputTargets.Overlay)]
        public DataSeries Average { get; set; }

        public int LastPositionChanged { get { return _shifter.Position; } }

        public MovingAverage() { }

        public MovingAverage(DataSeries price, int period, int shift, MovingAverageMethod method = MovingAverageMethod.Simple,
            double smoothFactor = double.NaN)
        {
            Price = price;
            Period = period;
            Shift = shift;
            TargetMethod = method;
            SmoothFactor = method == MovingAverageMethod.Exponential ? 2.0 / (period + 1) :
                (method == MovingAverageMethod.CustomExponential ? smoothFactor : double.NaN);

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _maInstance = MovAvg.Create(Period, TargetMethod, SmoothFactor);
            _shifter = new SimpleShifter(Shift);
            _shifter.Init();
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            if (!isNewBar)
            {
                _maInstance.UpdateLast(Price[0]);
                _shifter.UpdateLast(_maInstance.Average);
            }
            else
            {
                _maInstance.Add(Price[0]);
                _shifter.Add(_maInstance.Average);
            }
            Average[_shifter.Position] = _shifter.Result;
        }
    }
}
