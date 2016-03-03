using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.Parabolic
{

    [Indicator(IsOverlay = true)]
    public class Parabolic : Indicator
    {
        [Parameter(DefaultValue = 0.02)]
        public double InpSARStep { get; set; }

        [Parameter(DefaultValue = 0.2)]
        public double InpSARMaximum { get; set; }
        
        [Input]
        public DataSeries<Bar> Bars { get; set; }

        [Output]
        public DataSeries ExtSARBuffer { get; set; }

        double ExtSarStep;
        double ExtSarMaximum;
        int ExtLastReverse;
        bool ExtDirectionLong;
        double ExtLastStep, ExtLastEP, ExtLastSAR;
        double ExtLastHigh, ExtLastLow;

        bool dir_long;
        double last_high, last_low, ep, sar, step;

        private bool first_calc;
        protected override void Calculate()
        {
            if (Bars.Count == 1)
            {
                if (InpSARStep < 0.0)
                {
                    ExtSarStep = 0.02;
                    /* Print("Input parametr InpSARStep has incorrect value. Indicator will use value ",
                           ExtSarStep, " for calculations.");*/
                }
                else
                    ExtSarStep = InpSARStep;
                if (InpSARMaximum < 0.0)
                {
                    ExtSarMaximum = 0.2;
                    /* Print("Input parametr InpSARMaximum has incorrect value. Indicator will use value ",
                           ExtSarMaximum, " for calculations.");*/
                }
                else
                    ExtSarMaximum = InpSARMaximum;

                ExtLastReverse = 0;
                ExtDirectionLong = false;
                ExtLastStep = ExtLastEP = ExtLastSAR = 0.0;
                ExtLastHigh = ExtLastLow = 0.0;
                first_calc = true;
                ExtLastReverse = 0;
                dir_long = true;
                step = ExtSarStep;
                last_high = -10000000.0;
                last_low = 10000000.0;
                sar = 0;

               

            }

            if (Bars.Count > 1 && first_calc)
            {
                ExtLastReverse = Bars.Count-1;
                if (last_low > Bars[0].Low)
                    last_low = Bars[0].Low;
                if (last_high < Bars[0].High)
                    last_high = Bars[0].High;
                if (Bars[0].High > Bars[1].High && Bars[0].Low > Bars[1].Low)
                    first_calc = false;
                if (Bars[0].High > Bars[1].High && Bars[0].Low < Bars[1].Low)
                {
                    dir_long = false;
                    first_calc = false;
                }

                if (first_calc == false)
                {
                    if (dir_long)
                    {
                        ExtSARBuffer[0] = Bars[1].Low;
                        ep = Bars[0].High;
                    }
                    else
                    {
                        ExtSARBuffer[0] = Bars[1].High;
                        ep = Bars[0].Low;
                       
                    }
                    return;
                }

            }

            if (!first_calc)
            {
                //--- check for reverse
                if (dir_long && Bars[0].Low < ExtSARBuffer[1])
                {
                    SaveLastReverse(Bars.Count-1, true, step, Bars[0].Low, last_high, ep, sar);
                    step = ExtSarStep;
                    dir_long = false;
                    ep = Bars[0].Low;
                    last_low = Bars[0].Low;
                    ExtSARBuffer[0] = last_high;
                    return; 
                }
                if (!dir_long && Bars[0].High > ExtSARBuffer[1])
                {
                    SaveLastReverse(Bars.Count - 1, false, step, last_low, Bars[0].High, ep, sar);
                    step = ExtSarStep;
                    dir_long = true;
                    ep = Bars[0].High;
                    last_high = Bars[0].High;
                    ExtSARBuffer[0] = last_low;
                    return;
                }
                //---
                sar = ExtSARBuffer[1] + step * (ep - ExtSARBuffer[1]);
                //--- LONG?
                if (dir_long)
                {
                    if (ep < Bars[0].High)
                    {
                        if ((step + ExtSarStep) <= ExtSarMaximum)
                            step += ExtSarStep;
                    }
                    if (Bars[0].High < Bars[1].High && Bars.Count == 3)
                        sar = ExtSARBuffer[1];
                    if (sar > Bars[1].Low)
                        sar = Bars[1].Low;
                    if (sar > Bars[2].Low)
                        sar = Bars[2].Low;
                    if (sar > Bars[0].Low)
                    {
                        SaveLastReverse(Bars.Count - 1, true, step, Bars[0].Low, last_high, ep, sar);
                        step = ExtSarStep; dir_long = false; ep = Bars[0].Low;
                        last_low = Bars[0].Low;
                        ExtSARBuffer[0] = last_high;
                        return;
                    }
                    if (ep < Bars[0].High)
                        ep = last_high = Bars[0].High;
                }
                else // SHORT
                {
                    if (ep > Bars[0].Low)
                    {
                        if ((step + ExtSarStep) <= ExtSarMaximum)
                            step += ExtSarStep;
                    }
                    if (Bars[0].Low < Bars[1].Low && Bars.Count == 3)
                        sar = ExtSARBuffer[1];
                    if (sar < Bars[1].High)
                        sar = Bars[1].High;
                    if (sar < Bars[2].High)
                        sar = Bars[2].High;
                    if (sar < Bars[0].High)
                    {
                        SaveLastReverse(Bars.Count-1, false, step, last_low, Bars[0].High, ep, sar);
                        step = ExtSarStep;
                        dir_long = true;
                        ep = Bars[0].High;
                        last_high = Bars[0].High;
                        ExtSARBuffer[0] = last_low;
                        return;
                    }
                    if (ep > Bars[0].Low)
                        ep = last_low = Bars[0].Low;
                }
                ExtSARBuffer[0] = sar;
            }
        }

        private void SaveLastReverse(int reverse, bool dir, double step, double last_low, double last_high, double ep, double sar)
        {
            ExtLastReverse = reverse;
            if (ExtLastReverse < 2)
                ExtLastReverse = 2;
            ExtDirectionLong = dir;
            ExtLastStep = step;
            ExtLastLow = last_low;
            ExtLastHigh = last_high;
            ExtLastEP = ep;
            ExtLastSAR = sar;
        }
    }
}
