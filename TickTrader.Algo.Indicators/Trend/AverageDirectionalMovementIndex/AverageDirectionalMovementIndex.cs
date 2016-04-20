using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Trend.AverageDirectionalMovementIndex
{
    [Indicator(Category = "Trend", DisplayName = "Trend/Average Directional Movement Index")]
    public class AverageDirectionalMovementIndex : Indicator
    {
        [Parameter(DefaultValue = 14, DisplayName = "Period")]
        public int Period { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "ADX", DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Adx { get; set; }

        [Output(DisplayName = "+DMI", DefaultColor = Colors.YellowGreen, DefaultLineStyle = LineStyles.Lines)]
        public DataSeries PlusDmi { get; set; }

        [Output(DisplayName = "-DMI", DefaultColor = Colors.Wheat, DefaultLineStyle = LineStyles.Lines)]
        public DataSeries MinusDmi { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AverageDirectionalMovementIndex() { }

        public AverageDirectionalMovementIndex(DataSeries price, int period)
        {
            Price = price;
            Period = period;

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
