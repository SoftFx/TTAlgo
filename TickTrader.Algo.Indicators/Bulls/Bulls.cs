﻿using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.Bulls
{

    [Indicator]
    public class Bulls : Indicator
    {
        [Parameter(DefaultValue = 13)]
        public int BullsPeriod { get; set; }


        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtBullsBuffer { get; set; }

        private double PrevEMA;
        protected override void Calculate()
        {

            ExtBullsBuffer[0] = Double.NaN;

            if (Bars.Count == 0)
            {
                ExtBullsBuffer[0] = Bars[0].High - Bars[0].Close;
                PrevEMA = Bars[0].Close;
            }
            else
            {
                double buf = MovingAverages.ExponentialMA(0, BullsPeriod, PrevEMA,
                    Bars.Take(1).Select(b => b.Close).ToList());

                ExtBullsBuffer[0] = Bars[0].High - buf;
                PrevEMA = buf;
            }
        }
    }
}
