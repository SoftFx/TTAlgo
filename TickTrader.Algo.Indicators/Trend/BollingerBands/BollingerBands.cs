using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.BollingerBands
{
    [Indicator(IsOverlay = true, Category = "Trend", DisplayName = "Trend/Bollinger Bands")]
    public class BollingerBands : Indicator
    {
        private StandardDeviation.StandardDeviation _stdDev;

        [Parameter(DefaultValue = 20, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0, DisplayName = "Shift")]
        public int Shift { get; set; }
        
        [Parameter(DefaultValue = 2.0, DisplayName = "Deviations")]
        public double Deviations { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries MiddleLine { get; set; }

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries TopLine { get; set; }

        [Output(DefaultColor = Colors.MediumSeaGreen)]
        public DataSeries BottomLine { get; set; }

        public int LastPositionChanged { get { return _stdDev.LastPositionChanged; } }

        public BollingerBands() { }

        public BollingerBands(DataSeries<Bar> bars, int period, int shift,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            Shift = shift;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
            _stdDev = new StandardDeviation.StandardDeviation(Bars, Period, Shift, Method.Simple, TargetPrice);
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
