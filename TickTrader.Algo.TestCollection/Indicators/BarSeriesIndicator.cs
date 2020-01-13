using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Setup", DisplayName = "[T] Bar Series Indicator", Version = "1.0",
        Description = "Has outputs which are mapped to corresponding fields of Bars property")]
    public class BarSeriesIndicator : Indicator
    {
        [Output(DisplayName = "High", Target = OutputTargets.Overlay, DefaultColor = Colors.Magenta)]
        public DataSeries High { get; set; }

        [Output(DisplayName = "Low", Target = OutputTargets.Overlay, DefaultColor = Colors.Goldenrod)]
        public DataSeries Low { get; set; }

        [Output(DisplayName = "Open", Target = OutputTargets.Overlay, DefaultColor = Colors.Green)]
        public DataSeries Open { get; set; }

        [Output(DisplayName = "Close", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries Close { get; set; }


        protected override void Calculate(bool isNewBar)
        {
            if (Bars.Count == High.Count)
            {
                High[0] = Bars[0].High;
                Low[0] = Bars[0].Low;
                Open[0] = Bars[0].Open;
                Close[0] = Bars[0].Close;
            }
            else
            {
                High[0] = double.NaN;
                Low[0] = double.NaN;
                Open[0] = double.NaN;
                Close[0] = double.NaN;
            }
        }
    }
}
