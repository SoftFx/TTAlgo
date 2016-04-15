using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.BillWilliams.AwesomeOscillator
{
    [Indicator(Category = "Bill Williams", DisplayName = "Bill Williams/Awesome Oscillator")]
    public class AwesomeOscillator : Indicator
    {
        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Value Up", DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries ValueUp { get; set; }

        [Output(DisplayName = "Value Down", DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries ValueDown { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AwesomeOscillator() { }

        public AwesomeOscillator(DataSeries<Bar> bars)
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
