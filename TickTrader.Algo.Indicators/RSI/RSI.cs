using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.RSI
{

    [Indicator]
    public class RSI : Indicator
    {


        [Parameter(DefaultValue = 14)]
        public int InpRSIPeriod { get; set; }

        [Input]
        public BarSeries Bars { get; set; }


        [Output]
        public DataSeries ExtRSIBuffer { get; set; }




        private double diff;
        private double Pos;
        private double Neg;

        protected override void Calculate()
        {


/*            if (Bars.Count > 1 && Bars.Count <= InpRSIPeriod + 1)
            {
                diff = Bars[0].Close - Bars[1].Close;
                if (diff > 0)
                    sump += diff;
                else
                    sumn -= diff;
            }
            */
            if (Bars.Count == InpRSIPeriod + 1)
            {
                
                double sump=0;
                double sumn=0;

                for (int i = 0; i < InpRSIPeriod; i++)
                {
                    diff = Bars[i].Close - Bars[i+1].Close;
                    if (diff > 0)
                        sump += diff;
                    else
                        sumn -= diff;
                }
                Pos = sump / InpRSIPeriod;
                Neg = sumn / InpRSIPeriod;

                if (Neg != 0.0)
                    ExtRSIBuffer[0] = 100.0 - (100.0 / (1.0 + Pos / Neg));
                else
                {
                    if (Pos != 0.0)
                        ExtRSIBuffer[0] = 100.0;
                    else
                        ExtRSIBuffer[0] = 50.0;
                }

            }

            if (Bars.Count > InpRSIPeriod + 1)
            {
                double mult = 1.0/ InpRSIPeriod;
                Pos = 0;
                Neg = 0;
                for (int i = 0; i < Bars.Count - 1 && mult > 0.0000000001; i++)
                {
                    diff = Bars[i].Close - Bars[i+1].Close;
                    Pos += mult*(diff > 0.0 ? diff : 0.0);
                    Neg += mult*(diff < 0.0 ? -diff : 0.0);
                    mult *= ((InpRSIPeriod - 1)*1.0)/InpRSIPeriod;
                }
               
                if (Neg != 0.0)
                    ExtRSIBuffer[0] = 100.0 - 100.0 / (1 + Pos / Neg);
                else
                {
                    if (Pos != 0.0)
                        ExtRSIBuffer[0] = 100.0;
                    else
                        ExtRSIBuffer[0] = 50.0;
                }

            }



        }
    }
}
