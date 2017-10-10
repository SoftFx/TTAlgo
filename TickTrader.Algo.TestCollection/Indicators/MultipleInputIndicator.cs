using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Setup", DisplayName = "[T] Multiple Input Indicator", Version = "1.0",
        Description = "Puts value from input to corresponding output")]
    public class MultipleInputIndicator : Indicator
    {
        [Input(DisplayName = "Input 1")]
        public DataSeries Input1 { get; set; }

        [Input(DisplayName = "Input 2")]
        public DataSeries Input2 { get; set; }

        [Input(DisplayName = "Input 3")]
        public DataSeries Input3 { get; set; }

        [Input(DisplayName = "Input 4")]
        public DataSeries Input4 { get; set; }

        [Output(DisplayName = "Output 1", Target = OutputTargets.Window1, DefaultColor = Colors.Blue)]
        public DataSeries Output1 { get; set; }

        [Output(DisplayName = "Output 2", Target = OutputTargets.Window2, DefaultColor = Colors.Magenta)]
        public DataSeries Output2 { get; set; }

        [Output(DisplayName = "Output 3", Target = OutputTargets.Window3, DefaultColor = Colors.Red)]
        public DataSeries Output3 { get; set; }

        [Output(DisplayName = "Output 4", Target = OutputTargets.Window4, DefaultColor = Colors.DarkGoldenrod)]
        public DataSeries Output4 { get; set; }


        protected override void Calculate()
        {
            Output1[0] = Input1[0];
            Output2[0] = Input2[0];
            Output3[0] = Input3[0];
            Output4[0] = Input4[0];
        }
    }
}
