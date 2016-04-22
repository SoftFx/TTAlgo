using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Volumes.OnBalanceVolume
{
    [Indicator(Category = "Volumes", DisplayName = "Volumes/On Balance Volume")]
    public class OnBalanceVolume : Indicator
    {
        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "OBV", DefaultColor = Colors.LightSeaGreen)]
        public DataSeries Obv { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public OnBalanceVolume() { }

        public OnBalanceVolume(DataSeries price)
        {
            Price = price;

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
            
        }
    }
}
