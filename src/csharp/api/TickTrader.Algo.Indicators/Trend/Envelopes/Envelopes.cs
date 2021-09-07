using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Trend.Envelopes
{
    [Indicator(Category = "Trend", DisplayName = "Envelopes", Version = "1.0")]
    public class Envelopes : Indicator, IEnvelopes
    {
        private IMovingAverage _middleLine;
        
        [Parameter(DefaultValue = 7, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }

        [Parameter(DefaultValue = 0.1, DisplayName = "Deviation(%)")]
        public double Deviation { get; set; }

        [Parameter(DefaultValue = MovingAverageMethod.Simple, DisplayName = "Method")]
        public MovingAverageMethod TargetMethod { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "Top Line", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue)]
        public DataSeries TopLine { get; set; }

        [Output(DisplayName = "Bottom Line", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries BottomLine { get; set; }

        public int LastPositionChanged { get { return _middleLine.LastPositionChanged; } }

        public Envelopes() { }

        public Envelopes(DataSeries price, int period, int shift, double deviation, MovingAverageMethod targetMethod = MovingAverageMethod.Simple)
        {
            Price = price;
            Period = period;
            Shift = shift;
            TargetMethod = targetMethod;
            Deviation = deviation;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _middleLine = Indicators.MovingAverage(Price, Period, Shift, TargetMethod);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            var val = _middleLine.Average[pos];
            TopLine[pos] = val*(1.0 + Deviation/100);
            BottomLine[pos] = val*(1.0 - Deviation/100);
        }
    }
}
