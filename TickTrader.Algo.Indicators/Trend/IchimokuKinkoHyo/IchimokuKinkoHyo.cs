using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Trend.IchimokuKinkoHyo
{
    [Indicator(Category = "Trend", DisplayName = "Trend/Ichimoku Kinko Hyo")]
    public class IchimokuKinkoHyo : Indicator
    {
        [Parameter(DefaultValue = 9, DisplayName = "Tenkan-sen")]
        public int TenkanSen { get; set; }

        [Parameter(DefaultValue = 26, DisplayName = "Kijun-sen")]
        public int KijunSen { get; set; }

        [Parameter(DefaultValue = 52, DisplayName = "Senkou Span B")]
        public int SenkouSpanB { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Tenkan-sen", DefaultColor = Colors.Red)]
        public DataSeries Tenkan { get; set; }

        [Output(DisplayName = "Kijun-sen", DefaultColor = Colors.Blue)]
        public DataSeries Kijun { get; set; }

        [Output(DisplayName = "Senkou Span A", DefaultColor = Colors.SandyBrown, DefaultLineStyle = LineStyles.Lines)]
        public DataSeries SenkouA { get; set; }

        [Output(DisplayName = "Senkou Span B", DefaultColor = Colors.Thistle, DefaultLineStyle = LineStyles.Lines)]
        public DataSeries SenkouB { get; set; }

        [Output(DisplayName = "Chikou Span", DefaultColor = Colors.Lime)]
        public DataSeries Chikou { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public IchimokuKinkoHyo() { }

        public IchimokuKinkoHyo(DataSeries<Bar> bars, int tenkanSen, int kijunSen, int senkouSpanB)
        {
            Bars = bars;
            TenkanSen = tenkanSen;
            KijunSen = kijunSen;
            SenkouSpanB = senkouSpanB;
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
