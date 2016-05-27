using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    public enum CustomEnum { Val1, Val2, ValVal }
    public enum EmptyEnum { }

    [Indicator(DisplayName = "Custom Indicators / Simple Moving Average", IsOverlay = true)]
    public class SimpleMovingAverage : Indicator
    {
        [Parameter(DefaultValue = 5)]
        public int Window { get; set; }

        [Parameter(DefaultValue = 0.0)]
        public double Shift { get; set; }

        [Parameter(DefaultValue = CustomEnum.ValVal)]
        public CustomEnum Choice1 { get; set; }

        [Parameter(DefaultValue = CustomEnum.ValVal)]
        public EmptyEnum Choice2 { get; set; }

        [Input]
        public DataSeries Input { get; set; }

        [Output]
        public DataSeries Output { get; set; }

        protected override void Calculate()
        {
            if (Output.Count >= Window)
                Output[0] = Input.Take(Window).Average() + Shift;
        }
    }

    [Indicator(DisplayName = "Custom Indicators / Merker Test", IsOverlay = false)]
    public class MarkerTesetIndicator : Indicator
    {
        [Input]
        public DataSeries Input { get; set; }

        [Output]
        public DataSeries Level { get; set; }

        [Output(DefaultColor = Colors.MediumVioletRed)]
        public MarkerSeries Markers { get; set; }

        private Random rnd = new Random();

        protected override void Calculate()
        {
            var next = rnd.NextDouble();

            if (next < 0.03)
            {
                Markers[0].Icon = MarkerIcons.UpArrow;
                Markers[0].Color = Colors.MediumVioletRed;
                Markers[0].Y = Input[0];
                Markers[0].DisplayText = "UP! " + next;
            }
            else if (next >= 0.03 && next < 0.08)
            {
                Markers[0].Icon = MarkerIcons.DownArrow;
                Markers[0].Color = Colors.MediumVioletRed;
                Markers[0].Y = Input[0];
                Markers[0].DisplayText = "DOWN! " + next;
            }

            Level[0] = Input[0];
        }
    }
}
