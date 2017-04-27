using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Trend.BollingerBands
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Bollinger Bands", Version = "1.0")]
    public class BollingerBands : Indicator
    {
        private StandardDeviation.StandardDeviation _stdDev;

        [Parameter(DefaultValue = 20, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }
        
        [Parameter(DefaultValue = 2.0, DisplayName = "Deviations")]
        public double Deviations { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries MiddleLine { get; set; }

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries TopLine { get; set; }

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries BottomLine { get; set; }

        public int LastPositionChanged { get { return _stdDev.LastPositionChanged; } }

        public BollingerBands() { }

        public BollingerBands(DataSeries price, int period, int shift)
        {
            Price = price;
            Period = period;
            Shift = shift;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _stdDev = new StandardDeviation.StandardDeviation(Price, Period, Shift);
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            var pos = LastPositionChanged;
            var maVal = _stdDev.LastMaVal;
            var stdDev = _stdDev.StdDev[_stdDev.LastPositionChanged];

            MiddleLine[pos] = maVal;
            TopLine[pos] = maVal + Deviations*stdDev;
            BottomLine[pos] = maVal - Deviations*stdDev;
        }
    }
}
