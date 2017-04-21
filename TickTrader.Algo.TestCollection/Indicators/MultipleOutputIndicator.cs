using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Setup", DisplayName = "[T] Multiple Output Indicator", Version = "1.0",
        Description = "Has outputs that target several panes and main chart window")]
    public class MultipleOutputIndicator : Indicator
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(IsOverlay = true, DisplayName = "High", DefaultColor = Colors.Blue)]
        public DataSeries S1 { get; set; }

        [Output(IsOverlay = true, DisplayName = "Low", DefaultColor = Colors.Cyan)]
        public DataSeries S2 { get; set; }

        [Output(DisplayName = "Move", DefaultColor = Colors.Green, PaneName = "Pane1")]
        public DataSeries S3 { get; set; }

        [Output(DisplayName = "Range", DefaultColor = Colors.Red, PaneName = "Pane2")]
        public DataSeries S4 { get; set; }


        protected override void Calculate()
        {
            S1[0] = Bars[0].High;
            S2[0] = Bars[0].Low;
            S3[0] = Bars.Move[0];
            S4[0] = Bars.Range[0];
        }
    }
}
