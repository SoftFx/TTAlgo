using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.MACD
{

    [Indicator]
    public class MACD : Indicator
    {


        [Parameter(DefaultValue = 12)]
        public int InpFastEMA { get; set; }
        [Parameter(DefaultValue = 26)]
        public int InpSlowEMA { get; set; }
        [Parameter(DefaultValue = 9)]
        public int InpSignalSMA { get; set; }

        [Input]
        public DataSeries Close { get; set; }


        [Output]
        public DataSeries ExtMacdBuffer { get; set; }
        [Output]
        public DataSeries ExtSignalBuffer { get; set; }


        private double prevTF;
        protected override void Calculate()
        {
            ExtMacdBuffer[0] = Double.NaN;
            ExtSignalBuffer[0] = Double.NaN;

            List<double> localSignalBuffer = new List<double>();

            if (Close.Count >= Math.Max(InpFastEMA, InpSlowEMA))
            {
                //double fastVal = MovingAverages.ExponentialMA(0, InpFastEMA, prevFastVal, Close.ToList());
                double fastVal = MovingAverages.ExponentialMAinPlace(0, InpFastEMA, Close.ToList());
                //double slowVal = MovingAverages.ExponentialMA(0, InpSlowEMA, prevSlowVal, Close.ToList());
                double slowVal = MovingAverages.ExponentialMAinPlace(0, InpSlowEMA, Close.ToList());
                ExtMacdBuffer[0] = fastVal - slowVal;
            }

            
            if (Close.Count >= Math.Max(InpFastEMA, InpSlowEMA) + InpSignalSMA - 1)
            {
                for (int i = 0; i < InpSignalSMA; i++)
                {
                    double fastVal = MovingAverages.ExponentialMAinPlace(i, InpFastEMA, Close.ToList());
                    double slowVal = MovingAverages.ExponentialMAinPlace(i, InpSlowEMA, Close.ToList());
                    localSignalBuffer.Add(fastVal - slowVal); 
                }

                ExtSignalBuffer[0] = MovingAverages.SimpleMA(0, InpSignalSMA, localSignalBuffer);
            }


        }
    }
}
