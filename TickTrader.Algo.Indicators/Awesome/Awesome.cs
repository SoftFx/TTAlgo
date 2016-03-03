using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.Awesome
{

    [Indicator]
    public class Awesome : Indicator
    {
        [Parameter(DefaultValue = 5)]
        public int PeriodFast { get; set; }

        [Parameter(DefaultValue = 34)]
        public int PeriodSlow { get; set; }


        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtAOBuffer { get; set; }
        [Output]
        public DataSeries ExtUpBuffer { get; set; }
        [Output]
        public DataSeries ExtDnBuffer { get; set; }




        protected override void Calculate()
        {



            if (Bars.Count >= Math.Max(PeriodFast,PeriodSlow))
            {
                ExtAOBuffer[0] = MovingAverages.SimpleMA(0, PeriodFast, Bars.Take(PeriodFast).Select(b => (b.High+b.Low)/2).ToList())
                    - MovingAverages.SimpleMA(0, PeriodSlow, Bars.Take(PeriodSlow).Select(b => (b.High + b.Low) / 2).ToList());
                
                if (Bars.Count >= 2)
                {
                    bool up;
                    if (ExtAOBuffer[0] > ExtAOBuffer[1])
                        up = true;
                    else
                        up = false;

                    if (!up)
                    {
                        ExtDnBuffer[0] = ExtAOBuffer[0];
                        ExtUpBuffer[0] = 0.0;
                        if(Bars.Count == 2)
                            ExtDnBuffer[1] = ExtAOBuffer[1];
                    }
                    else
                    {
                        ExtUpBuffer[0] = ExtAOBuffer[0];
                        ExtDnBuffer[0] = 0.0;
                        if (Bars.Count == 2)
                            ExtUpBuffer[1] = ExtAOBuffer[1];
                    }

                }
            }
        }



    }
}
