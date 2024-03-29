﻿using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.Indicators.Other.HeikenAshi
{
    [Indicator(Category = "Other", DisplayName = "Heiken Ashi", Version = "1.0")]
    public class HeikenAshi : Indicator, IHeikenAshi
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Low/High", Target = OutputTargets.Overlay, DefaultColor = Colors.Red, PlotType = PlotType.Histogram)]
        public DataSeries HaLowHigh { get; set; }

        [Output(DisplayName = "High/Low", Target = OutputTargets.Overlay, DefaultColor = Colors.White, PlotType = PlotType.Histogram)]
        public DataSeries HaHighLow { get; set; }

        [Output(DisplayName = "Open", Target = OutputTargets.Overlay, DefaultColor = Colors.Red, PlotType = PlotType.Histogram, DefaultThickness = 3)]
        public DataSeries HaOpen { get; set; }

        [Output(DisplayName = "Close", Target = OutputTargets.Overlay, DefaultColor = Colors.White, PlotType = PlotType.Histogram, DefaultThickness = 3)]
        public DataSeries HaClose { get; set; }

        public int LastPositionChanged
        {
            get { return 0; }
        }

        public HeikenAshi()
        {
        }

        public HeikenAshi(BarSeries bars)
        {
            Bars = bars;

            InitializeIndicator();
        }

        protected void InitializeIndicator()
        {
        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate(bool isNewBar)
        {
            var pos = LastPositionChanged;
            if (Bars.Count > 1)
            {
                var haOpen = (HaOpen[pos + 1] + HaClose[pos + 1])/2;
                var haClose = (Bars.Open[pos] + Bars.High[pos] + Bars.Low[pos] + Bars.Close[pos])/4;
                var haHigh = Math.Max(Bars.High[pos], Math.Max(haOpen, haClose));
                var haLow = Math.Min(Bars.Low[pos], Math.Min(haOpen, haClose));
                if (haOpen < haClose)
                {
                    HaLowHigh[pos] = haLow;
                    HaHighLow[pos] = haHigh;
                }
                else
                {
                    HaLowHigh[pos] = haHigh;
                    HaHighLow[pos] = haLow;
                }
                HaOpen[pos] = haOpen;
                HaClose[pos] = haClose;
            }
            else
            {
                if (Bars.Open[pos] < Bars.Close[pos])
                {
                    HaLowHigh[pos] = Bars.Low[pos];
                    HaHighLow[pos] = Bars.High[pos];
                }
                else
                {
                    HaLowHigh[pos] = Bars.High[pos];
                    HaHighLow[pos] = Bars.Low[pos];
                }
                HaOpen[pos] = Bars.Open[pos];
                HaClose[pos] = Bars.Close[pos];
            }
        }
    }
}