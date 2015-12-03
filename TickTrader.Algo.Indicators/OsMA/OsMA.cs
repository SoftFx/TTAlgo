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

        
        private double prevFastVal;
        private double prevSlowVal;

        private List<double> ExtMacdBuffer = new List<double>();
        private List<double> ExtSignalBuffer = new List<double>();


        protected override void Calculate()
        {


            if (Close.Count == Math.Max(InpFastEMA, InpSlowEMA))
            {
                prevFastVal = Close[0];
                prevSlowVal = Close[0];
            }

            if (Close.Count >= Math.Max(InpFastEMA, InpSlowEMA))
            {
                double fastVal = MovingAverages.ExponentialMA(0, InpFastEMA, prevFastVal, Close.ToList());
                double slowVal = MovingAverages.ExponentialMA(0, InpSlowEMA, prevSlowVal, Close.ToList());
                ExtMacdBuffer.Add(fastVal - slowVal);
                prevFastVal = fastVal;
                prevSlowVal = slowVal;
            }

            if (Close.Count >= Math.Max(InpFastEMA, InpSlowEMA) + InpSignalSMA - 1)
            {
                ExtSignalBuffer.Add(MovingAverages.SimpleMA(ExtMacdBuffer.Count - InpSignalSMA, InpSignalSMA,
                    ExtMacdBuffer));

                ExtOsmaBuffer[0] = ExtMacdBuffer.Last() - ExtSignalBuffer.Last();
            }
        }
    }
}
