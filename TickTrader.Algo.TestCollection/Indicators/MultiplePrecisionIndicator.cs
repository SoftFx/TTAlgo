using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Setup", DisplayName = "[T] Multiple Precision Indicator", Version = "1.0",
        Description = "Has outputs that have different precision on several panes and main chart window")]
    public class MultiplePrecisionIndicator : Indicator
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "High (8)", Target = OutputTargets.Overlay, DefaultColor = Colors.Blue, Precision = 8)]
        public DataSeries S1 { get; set; }

        [Output(DisplayName = "Low (3)", Target = OutputTargets.Overlay, DefaultColor = Colors.Cyan, Precision = 3)]
        public DataSeries S2 { get; set; }

        [Output(DisplayName = "Move (4)", Target = OutputTargets.Window1, DefaultColor = Colors.Green, PlotType = PlotType.Histogram, Precision = 4)]
        public DataSeries S3 { get; set; }

        [Output(DisplayName = "Range (6)", Target = OutputTargets.Window1, DefaultColor = Colors.Red, Precision = 6)]
        public DataSeries S4 { get; set; }

        [Output(DisplayName = "Median (2)", Target = OutputTargets.Window2, DefaultColor = Colors.DarkGoldenrod, PlotType = PlotType.Histogram, Precision = 2)]
        public DataSeries S5 { get; set; }

        [Output(DisplayName = "Typical (by symbol)", Target = OutputTargets.Window3, DefaultColor = Colors.Magenta, PlotType = PlotType.Histogram)]
        public DataSeries S6 { get; set; }

        protected override void Calculate(bool isNewBar)
        {
            S1[0] = Bars.High[0];
            S2[0] = Bars.Low[0];
            S3[0] = Bars.Move[0];
            S4[0] = Bars.Range[0];
            S5[0] = Bars.Median[0];
            S6[0] = Bars.Typical[0];
        }
    }
}