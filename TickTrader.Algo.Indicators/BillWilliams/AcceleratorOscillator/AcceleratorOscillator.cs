using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.BillWilliams.AcceleratorOscillator
{
    [Indicator(Category = "Bill Williams", DisplayName = "Bill Williams/Accelerator Oscillator")]
    public class AcceleratorOscillator : Indicator
    {
        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output(DisplayName = "Value Up", DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries ValueUp { get; set; }

        [Output(DisplayName = "Value Down", DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries ValueDown { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public AcceleratorOscillator() { }

        public AcceleratorOscillator(DataSeries<Bar> bars)
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
