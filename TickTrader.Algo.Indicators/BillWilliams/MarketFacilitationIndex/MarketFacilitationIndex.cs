using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.BillWilliams.MarketFacilitationIndex
{
    [Indicator(Category = "Bill Williams", DisplayName = "Bill Williams/Market Facilitation Index")]
    public class MarketFacilitationIndex : Indicator
    {
        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "MFI Up Volume Up", DefaultColor = Colors.Lime, PlotType = PlotType.Histogram)]
        public DataSeries MfiUpVolumeUp { get; set; }

        [Output(DisplayName = "MFI Down Volume Down", DefaultColor = Colors.SaddleBrown, PlotType = PlotType.Histogram)]
        public DataSeries MfiDownVolumeDown { get; set; }

        [Output(DisplayName = "MFI Up Volume Down", DefaultColor = Colors.Blue, PlotType = PlotType.Histogram)]
        public DataSeries MfiUpVolumeDown { get; set; }

        [Output(DisplayName = "MFI Down Volume Up", DefaultColor = Colors.Pink, PlotType = PlotType.Histogram)]
        public DataSeries MfiDownVolumeUp { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public MarketFacilitationIndex() { }

        public MarketFacilitationIndex(DataSeries<Bar> bars)
        {
            Bars = bars;

            InitializeIndicator();
        }

        private void InitializeIndicator()
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
