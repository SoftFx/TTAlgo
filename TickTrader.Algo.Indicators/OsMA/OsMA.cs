using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.OsMA
{

    [Indicator]
    public class OsMA : Indicator
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
        public DataSeries ExtOsmaBuffer { get; set; }
        
        private List<double> ExtMacdBuffer = new List<double>();
        //private List<double> ExtSignalBuffer = new List<double>();
        private double fastVal;
        private double slowVal;
        private double signal;
        protected override void Calculate()
        {


            ExtMacdBuffer.Clear();
            
            if (Close.Count >= Math.Max(InpFastEMA, InpSlowEMA))
            {
                for (int i = InpSignalSMA - 1; i >= 0; i--)
                {
                    fastVal = MovingAverages.ExponentialMAinPlace(i, InpFastEMA, Close.ToList());
                    slowVal = MovingAverages.ExponentialMAinPlace(i, InpSlowEMA, Close.ToList());
                    ExtMacdBuffer.Add(fastVal - slowVal);
                }
            }

            if (Close.Count >= Math.Max(InpFastEMA, InpSlowEMA) + InpSignalSMA - 1)
            {
                signal = MovingAverages.SimpleMA(ExtMacdBuffer.Count - InpSignalSMA, InpSignalSMA,
                    ExtMacdBuffer);

                ExtOsmaBuffer[0] = ExtMacdBuffer.Last() - signal;
            }
        }
    }
}
