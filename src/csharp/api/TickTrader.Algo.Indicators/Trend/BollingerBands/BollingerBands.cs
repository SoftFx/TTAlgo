using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Trend.BollingerBands
{
    [Indicator(Category = "Trend", DisplayName = "Bollinger Bands", Version = "1.0")]
    public class BollingerBands : Indicator, IBoolingerBands
    {
        private IStandardDeviation _stdDev;
        private IMovingAverage _ma;

        [Parameter(DefaultValue = 20, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }
        
        [Parameter(DefaultValue = 2.0, DisplayName = "Deviations")]
        public double Deviations { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "Middle Line", Target = OutputTargets.Overlay, DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries MiddleLine { get; set; }

        [Output(DisplayName = "Top Line", Target = OutputTargets.Overlay, DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries TopLine { get; set; }

        [Output(DisplayName = "Bottom Line", Target = OutputTargets.Overlay, DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries BottomLine { get; set; }

        public int LastPositionChanged { get { return _stdDev.LastPositionChanged; } }

        public BollingerBands() { }

        public BollingerBands(DataSeries price, int period, int shift, double deviations)
        {
            Price = price;
            Period = period;
            Shift = shift;
            Deviations = deviations;
            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _stdDev = Indicators.StandardDeviation(Price, Period, Shift);
            _ma = Indicators.MovingAverage(Price, Period, Shift);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            var maVal = _ma.Average[_stdDev.LastPositionChanged];
            var stdDev = _stdDev.StdDev[_stdDev.LastPositionChanged];

            MiddleLine[pos] = maVal;
            TopLine[pos] = maVal + Deviations*stdDev;
            BottomLine[pos] = maVal - Deviations*stdDev;
        }
    }
}
