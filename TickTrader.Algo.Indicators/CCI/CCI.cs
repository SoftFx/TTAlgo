using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.CCI
{

    [Indicator]
    public class CCI : Indicator
    {
        [Parameter(DefaultValue = 14)]
        public int CCIPeriod { get; set; }


        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtCCIBuffer{ get; set; }

        private double MovCC;
        protected override void Calculate()
        {
            ExtCCIBuffer[0] = Double.NaN;
            if (CCIPeriod > 0)
            {

                if (Bars.Count >= CCIPeriod)
                {
                    MovCC = MovingAverages.SimpleMA(0, CCIPeriod,
                        Bars.Take(CCIPeriod).Select(b => (b.High + b.Low + b.Close)/3).ToList());
                    double dSum = 0.0;
                    for (int i = 0; i < CCIPeriod; i++)
                    {
                        dSum +=
                            Math.Abs((Bars[i].High + Bars[i].Low + Bars[i].Close)/3 -
                                     MovCC);
                    }

                    if (dSum < 0.000000001)
                    {
                        ExtCCIBuffer[0] = 0.0;
                    }
                    else
                    {
                        double MMA = dSum/CCIPeriod;
                        double t = (Bars[0].High + Bars[0].Low + Bars[0].Close)/3 - MovCC;
                        ExtCCIBuffer[0] = (1/0.015)*(t/MMA);
                    }
                }
            }


        }
    }
}
