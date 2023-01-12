using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Setup", DisplayName = "[T] Full Output Indicator", Version = "1.0")]
    internal sealed class FullOutputIndicator : Indicator
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Avr", Target = OutputTargets.Overlay, DefaultColor = Colors.Violet, DefaultThickness = 2)]
        public DataSeries Avr { get; set; }

        [Output(DisplayName = "AvrF", Target = OutputTargets.Overlay, DefaultColor = Colors.Sienna, DefaultThickness = 5, DefaultLineStyle = LineStyles.Dots)]
        public DataSeries AvrF { get; set; }

        [Output(DisplayName = "Deviation", Target = OutputTargets.Window1, DefaultColor = Colors.Blue, PlotType = PlotType.Histogram, ZeroLine = 0.01, Precision = 2)]
        public DataSeries Deviation { get; set; }

        [Output(DisplayName = "Trend", Target = OutputTargets.Window2, DefaultColor = Colors.Red, PlotType = PlotType.Points)]
        public DataSeries Trend { get; set; }

        [Output(DisplayName = "Drop", Target = OutputTargets.Window3, DefaultColor = Colors.DarkGreen, PlotType = PlotType.DiscontinuousLine, DefaultLineStyle = LineStyles.LinesDots)]
        public DataSeries Drop { get; set; }

        [Output(DisplayName = "Volume", Target = OutputTargets.Window4, DefaultColor = Colors.Orange, DefaultLineStyle = LineStyles.Solid, Precision = 6)]
        public DataSeries Volume { get; set; }


        protected override void Calculate(bool isNewBar)
        {
            Avr[0] = (Bars[0].High + Bars[0].Low) / 2;
            AvrF[0] = (Bars[0].Open + Bars[0].Close) / 2;
            Deviation[0] = Bars[0].High - Bars[0].Low;
            Volume[0] = Bars[0].Volume;
            Trend[0] = Bars[0].Close > Bars[0].Open ? 1.0 : 0.0;
            Drop[0] = Bars[0].Close > Bars[0].Open ? double.NaN : Bars[0].Open;
        }
    }
}
