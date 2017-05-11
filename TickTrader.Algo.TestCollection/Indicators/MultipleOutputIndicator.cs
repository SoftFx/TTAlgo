using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Setup", DisplayName = "[T] Multiple Output Indicator", Version = "1.0",
        Description = "Has outputs that target several panes and main chart window")]
    public class MultipleOutputIndicator : Indicator
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "High", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue)]
        public DataSeries S1 { get; set; }

        [Output(DisplayName = "Low", Target = OutputTargets.Overlay, DefaultColor = Colors.Cyan)]
        public DataSeries S2 { get; set; }

        [Output(DisplayName = "Move", Target = OutputTargets.Window1, DefaultColor = Colors.Green)]
        public DataSeries S3 { get; set; }

        [Output(DisplayName = "Range", Target = OutputTargets.Window2, DefaultColor = Colors.Red)]
        public DataSeries S4 { get; set; }

        [Output(DisplayName = "Volume", Target = OutputTargets.Window3, DefaultColor = Colors.DarkGoldenrod)]
        public DataSeries S5 { get; set; }

        [Output(DisplayName = "Typical", Target = OutputTargets.Window4, DefaultColor = Colors.Magenta)]
        public DataSeries S6 { get; set; }


        protected override void Calculate()
        {
            S1[0] = Bars[0].High;
            S2[0] = Bars[0].Low;
            S3[0] = Bars.Move[0];
            S4[0] = Bars.Range[0];
            S5[0] = Bars.Volume[0];
            S6[0] = Bars.Typical[0];
        }
    }
}
