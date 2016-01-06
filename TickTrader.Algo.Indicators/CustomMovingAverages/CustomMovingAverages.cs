using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.CustomMovingAverages
{

    [Indicator]
    public class CustomMovingAverages : Indicator
    {
        [Parameter(DefaultValue = 0)]
        public int Mode { get; set; } //  0 - MODE_EMA  1 - MODE_LWMA  2 - MODE_SMMA   3 - MODE_SMA

        [Parameter(DefaultValue = 0)]
        public int InpMAShift { get; set; } 


        [Parameter(DefaultValue = 13)]
        public int InpMAPeriod { get; set; } 



        [Input]
        public DataSeries Close { get; set; }


        [Output]
        public DataSeries ExtCCIBuffer { get; set; }

        private double MovCC;
        protected override void Calculate()
        {
            ExtCCIBuffer[0] = Double.NaN;
            if(InpMAShift >= 0)
            {
                if (Close.Count >= InpMAPeriod + InpMAShift)
                {
                    switch (Mode)
                    {
                        case 0:
                            ExtCCIBuffer[0] = MovingAverages.ExponentialMAinPlace(InpMAShift, InpMAPeriod,
                                Close.ToList());
                            break;
                        case 1:
                            ExtCCIBuffer[0] = MovingAverages.LinearWMA(InpMAShift, InpMAPeriod, Close.ToList());
                            break;
                        case 2:
                            ExtCCIBuffer[0] = MovingAverages.SmoothedMAinPlace(InpMAShift, InpMAPeriod,
                                Close.ToList());
                            break;
                        case 3:
                            ExtCCIBuffer[0] = MovingAverages.SimpleMA(InpMAShift, InpMAPeriod,
                                Close.ToList());
                            break;
                        default:
                            ExtCCIBuffer[0] = MovingAverages.SimpleMA(InpMAShift, InpMAPeriod,
                                Close.ToList());
                            break;
                    }
                }
            }
            else
            {
                if (Close.Count >= InpMAPeriod)
                {
                    switch (Mode)
                    {
                        case 0:
                            ExtCCIBuffer[-InpMAShift] = MovingAverages.ExponentialMAinPlace(0, InpMAPeriod,
                                Close.ToList());
                            break;
                        case 1:
                            MovingAverages.LinearWMA(0, InpMAPeriod, Close.ToList());
                            break;
                        case 2:
                            ExtCCIBuffer[-InpMAShift] = MovingAverages.SmoothedMAinPlace(0, InpMAPeriod,
                                Close.ToList());
                            break;
                        case 3:
                            ExtCCIBuffer[-InpMAShift] = MovingAverages.SimpleMA(0, InpMAPeriod,
                                Close.ToList());
                            break;
                        default:
                            ExtCCIBuffer[-InpMAShift] = MovingAverages.SimpleMA(0, InpMAPeriod,
                                Close.ToList());
                            break;
                    }
                }
            }

        }
    }
}
