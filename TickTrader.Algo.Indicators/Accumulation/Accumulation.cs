using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TickTrader.Algo.Api;


namespace TickTrader.Algo.Indicators.Accumulation
{
    public class Accumulation : Indicator
    {

        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtADBuffer { get; set; }
        
        protected override void Calculate()
        {
            ExtADBuffer[0] = (Bars[0].Close - Bars[0].Low) - (Bars[0].High - Bars[0].Close);
            if (ExtADBuffer[0] != 0.0)
            {
                double diff = Bars[0].High - Bars[0].Low;
                if (diff < 0.000000001)
                    ExtADBuffer[0] = 0.0;
                else
                {
                    ExtADBuffer[0] /= diff;
                    ExtADBuffer[0] *= (double)Bars[0].Volume;
                }
            }
            if (Bars.Count > 1)
                ExtADBuffer[0] += ExtADBuffer[1];
        }

    }
}
