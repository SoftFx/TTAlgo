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
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtRSIBuffer { get; set; }



        private double prevPos;
        private double prevNeg;
        private double Pos;
        private double Neg;

        private double diff;
        double sump;
        double sumn;
        protected override void Calculate()
        {
            if (Bars.Count == 1)
            {
                sumn = 0.0;
                sump = 0.0;
            }

            if (Bars.Count > 1 && Bars.Count <= InpRSIPeriod + 1)
            {
                diff = Bars[0].Close - Bars[1].Close;
                if (diff > 0)
                    sump += diff;
                else
                    sumn -= diff;
            }

            if (Bars.Count == InpRSIPeriod + 1)
            {
                Pos = sump / InpRSIPeriod;
                Neg = sumn / InpRSIPeriod;
                prevPos = Pos;
                prevNeg = Neg;
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
                diff = Bars[0].Close - Bars[1].Close;

                Pos = (prevPos * (InpRSIPeriod - 1) + (diff > 0.0 ? diff : 0.0)) / InpRSIPeriod;
                Neg = (prevNeg * (InpRSIPeriod - 1) + (diff < 0.0 ? -diff : 0.0)) / InpRSIPeriod;
                prevPos = Pos;
                prevNeg = Neg;
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
