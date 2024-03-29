﻿using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Volumes.Volumes
{
    [Indicator(Category = "Volumes", DisplayName = "Volumes", Version = "1.0")]
    public class Volumes : Indicator, IVolumes
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Value Up", Target = OutputTargets.Window1, DefaultColor = Colors.Green, PlotType = PlotType.Histogram)]
        public DataSeries ValueUp { get; set; }

        [Output(DisplayName = "Value Down", Target = OutputTargets.Window1, DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries ValueDown { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public Volumes() { }

        public Volumes(BarSeries bars)
        {
            Bars = bars;

            InitializeIndicator();
        }

        protected void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            if (Bars.Count > 1)
            {
                if (Bars.Volume[pos] > Bars.Volume[pos + 1])
                {
                    ValueUp[pos] = Bars.Volume[pos];
                    ValueDown[pos] = double.NaN;
                }
                else
                {
                    ValueUp[pos] = double.NaN;
                    ValueDown[pos] = Bars.Volume[pos];
                }
            }
            else
            {
                ValueUp[pos] = Bars.Volume[pos];
                ValueDown[pos] = double.NaN;
            }
        }
    }
}
