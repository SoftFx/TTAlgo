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

        private double prevFastVal = 0;
        private double prevSlowVal = 0;

        protected override void Calculate()
        {
            ExtMacdBuffer[0] = Double.NaN;
            ExtSignalBuffer[0] = Double.NaN;

            if (Close.Count == Math.Max(InpFastEMA, InpSlowEMA))
            {
                prevFastVal = Close[0];
                prevSlowVal = Close[0];
            }

            if (Close.Count >= Math.Max(InpFastEMA, InpSlowEMA))
            {
                double fastVal = MovingAverages.ExponentialMA(0, InpFastEMA, prevFastVal, Close.ToList());
                double slowVal = MovingAverages.ExponentialMA(0, InpSlowEMA, prevSlowVal, Close.ToList());
                ExtMacdBuffer[0] = fastVal - slowVal;
                prevFastVal = fastVal;
                prevSlowVal = slowVal;
            }

            if (Close.Count >= Math.Max(InpFastEMA, InpSlowEMA) + InpSignalSMA - 1)
            {
                ExtSignalBuffer[0] = MovingAverages.SimpleMA(0, InpSignalSMA, ExtMacdBuffer.ToList());
            }


        }
    }
}
