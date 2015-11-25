using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.ATR
{

    [Indicator]
    public class ATR : Indicator
    {
        [Parameter(DefaultValue = 14)]
        public int InpAtrPeriod { get; set; }


        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtATRBuffer { get; set; }



        private List<double> ExtTRBuffer; 

        protected override void Calculate()
        {
            
            ExtATRBuffer[0] = Double.NaN;
            if (Bars.Count > InpAtrPeriod)
            {
                ExtTRBuffer = new List<double>(InpAtrPeriod);
                for (int i =0;i< InpAtrPeriod; i++)
                {
                    ExtTRBuffer.Add(Math.Max(Bars[i].High, Bars[i + 1].Close) - Math.Min(Bars[i].Low, Bars[i + 1].Close));
                }
                ExtATRBuffer[0] = MovingAverages.SimpleMA(0, InpAtrPeriod, ExtTRBuffer);
            }
        }



    }
}
