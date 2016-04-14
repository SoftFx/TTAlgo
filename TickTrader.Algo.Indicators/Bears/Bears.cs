using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.Bears
{

    [Indicator]
    public class Bears : Indicator
    {
        [Parameter(DefaultValue = 13)]
        public int BearsPeriod { get; set; }


        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtBearsBuffer { get; set; }

        //private double PrevEMA;
        protected override void Calculate()
        {


            if (Bars.Count == 1)
            {
                ExtBearsBuffer[0] = Bars[0].Low - Bars[0].Close;
                //PrevEMA = Bars[0].Close;
            }
            if (Bars.Count > 1)
            {
                //double buf = MovingAverages.ExponentialMA(0, BearsPeriod, PrevEMA,
                //    Bars.Take(1).Select(b => b.Close).ToList());

                double buf = MovingAverages.ExponentialMAinPlace(0, BearsPeriod, Bars.Select(b => b.Close).ToList());

                ExtBearsBuffer[0] = Bars[0].Low - buf;
                //PrevEMA = buf;
            }
        }
    }
}
