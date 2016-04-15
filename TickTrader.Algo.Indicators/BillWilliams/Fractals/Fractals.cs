using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.BillWilliams.Fractals
{
    [Indicator(Category = "Bill Williams", DisplayName = "Bill Williams/Fractals")]
    public class Fractals : Indicator
    {
        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Fractals Up", DefaultColor = Colors.Gray, PlotType = PlotType.Points, DefaultThickness = 4)]
        public DataSeries FractalsUp { get; set; }

        [Output(DisplayName = "Fractals Down", DefaultColor = Colors.Gray, PlotType = PlotType.Points, DefaultThickness = 4)]
        public DataSeries FractalsDown { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public Fractals() { }

        public Fractals(DataSeries<Bar> bars)
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
