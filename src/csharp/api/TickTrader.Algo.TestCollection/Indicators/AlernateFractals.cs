using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Routine", DisplayName = "[T] Alternate Fractals", Version = "1.0")]
    public class AlernateFractals : Indicator
    {
        private IFractals _fractals;
        private int _cnt;


        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Fractals Up", Target = OutputTargets.Overlay, DefaultColor = Colors.Gray)]
        public DataSeries<Marker> FractalsUp { get; set; }

        [Output(DisplayName = "Fractals Down", Target = OutputTargets.Overlay, DefaultColor = Colors.Gray)]
        public DataSeries<Marker> FractalsDown { get; set; }


        protected override void Init()
        {
            _fractals = Indicators.Fractals(Bars);
        }


        protected override void Calculate(bool isNewBar)
        {
            var pos = 2;

            if (!double.IsNaN(_fractals.FractalsUp[pos].Y))
                CopyMarker(_fractals.FractalsUp[pos], FractalsUp[pos], 217);
            else
                FractalsUp[pos].Clear();

            if (!double.IsNaN(_fractals.FractalsDown[pos].Y))
                CopyMarker(_fractals.FractalsDown[pos], FractalsDown[pos], 218);
            else
                FractalsDown[pos].Clear();

        }

        private void CopyMarker(Marker src, Marker dst, ushort code)
        {
            dst.Y = src.Y;
            dst.DisplayText = src.DisplayText;
            dst.Icon = MarkerIcons.Wingdings;
            dst.IconCode = code;

            // ugly hack to test normal and auto colors
            _cnt++;
            if (_cnt % 5 == 0)
                dst.Color = Colors.Green;
            if (_cnt % 7 == 0)
                dst.Color = Colors.Blue;
        }
    }
}
