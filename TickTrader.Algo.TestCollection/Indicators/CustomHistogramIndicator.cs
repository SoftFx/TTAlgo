using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Setup", DisplayName = "[T] Custom Histogram Indicator", Version = "1.0",
        Description = "Display several random histograms with different zero line")]
    public class CustomHistogramIndicator : Indicator
    {
        private Random _rand;


        [Output(DisplayName = "Zero Line 1", PlotType = PlotType.Histogram, Target = OutputTargets.Window1, DefaultColor = Colors.Red, ZeroLine = 1)]
        public DataSeries ZeroLine1 { get; set; }

        [Output(DisplayName = "Zero Line 2", PlotType = PlotType.Histogram, Target = OutputTargets.Window2, DefaultColor = Colors.Green, ZeroLine = 2)]
        public DataSeries ZeroLine2 { get; set; }

        [Output(DisplayName = "Zero Line -3", PlotType = PlotType.Histogram, Target = OutputTargets.Window3, DefaultColor = Colors.Blue, ZeroLine = -3)]
        public DataSeries ZeroLine3 { get; set; }


        protected override void Init()
        {
            _rand = new Random();
        }

        protected override void Calculate()
        {
            if (!IsUpdate)
            {
                ZeroLine1[0] = _rand.NextDouble() * 2;
                ZeroLine2[0] = _rand.NextDouble() * 4;
                ZeroLine3[0] = _rand.NextDouble() * -6;
            }
        }
    }
}
