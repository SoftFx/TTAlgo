using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.RangeBoundChannelIndex
{
    [Indicator(Category = "AT&CF Method", DisplayName = "AT&CF Method/Range Bound Channel Index")]
    public class RangeBoundChannelIndex : Indicator
    {
        [Parameter(DefaultValue = 18, DisplayName = "STD")]
        public int Std { get; set; }

        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "RBCI", DefaultColor = Colors.Teal)]
        public DataSeries Rbci { get; set; }

        [Output(DisplayName = "Upper Bound", DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare)]
        public DataSeries UpperBound { get; set; }

        [Output(DisplayName = "Lower Bound", DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare)]
        public DataSeries LowerBound { get; set; }

        [Output(DisplayName = "Upper Bound 2", DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare)]
        public DataSeries UpperBound2 { get; set; }

        [Output(DisplayName = "Lower Bound 2", DefaultColor = Colors.DarkOrange, DefaultLineStyle = LineStyles.DotsRare)]
        public DataSeries LowerBound2 { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public RangeBoundChannelIndex() { }

        public RangeBoundChannelIndex(DataSeries price)
        {
            Price = price;

            InitializeIndicator();
        }

        private void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            
        }
    }
}
