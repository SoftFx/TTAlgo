using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Utility;

namespace TickTrader.Algo.Indicators.Trend.AverageDirectionalMovementIndex
{
    [Indicator(Category = "Trend", DisplayName = "Trend/Average Directional Movement Index")]
    public class AverageDirectionalMovementIndex : Indicator
    {
        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Parameter(DefaultValue = AppliedPrice.Target.Close, DisplayName = "Apply To")]
        public AppliedPrice.Target TargetPrice { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "ADX", DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Adx { get; set; }

        [Output(DisplayName = "+DMI", DefaultColor = Colors.YellowGreen, DefaultLineStyle = LineStyles.Lines)]
        public DataSeries PlusDmi { get; set; }

        [Output(DisplayName = "-DMI", DefaultColor = Colors.Wheat, DefaultLineStyle = LineStyles.Lines)]
        public DataSeries MinusDmi { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AverageDirectionalMovementIndex() { }

        public AverageDirectionalMovementIndex(DataSeries<Bar> bars, int period,
            AppliedPrice.Target targetPrice = AppliedPrice.Target.Close)
        {
            Bars = bars;
            Period = period;
            TargetPrice = targetPrice;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {

        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            throw new System.NotImplementedException();
        }
    }
}
