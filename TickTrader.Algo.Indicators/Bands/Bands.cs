using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;

namespace TickTrader.Algo.Indicators.Bands
{

    [Indicator]
    public class Bands : Indicator
    {
        [Parameter(DefaultValue = 20)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 0.0)]
        public double Shift { get; set; }

        [Parameter(DefaultValue = 2.0)]
        public double Deviations { get; set; }


        [Input]
        public DataSeries Close { get; set; }


        [Output]
        public DataSeries ExtMovingBuffer { get; set; }

        [Output]
        public DataSeries ExtUpperBuffer { get; set; }

        [Output]
        public DataSeries ExtLowerBuffer { get; set; }




        protected override void Calculate()
        {

            //--- middle line

            ExtMovingBuffer[0] = MovingAverages.SimpleMA(0,Period, Close.Take(Period).ToList());
            List<double> bufferForSTDcalc = new List<double>();
            //SimpleMovAv(0, Convert.ToInt32(Period), Close);
            //--- calculate and write down StdDev
            double stdDev = StdDev_Func(0, Close, ExtMovingBuffer[0], Convert.ToInt32(Period));
            //--- upper line
            ExtUpperBuffer[0] = ExtMovingBuffer[0] + Deviations * stdDev;
            //--- lower line
            ExtLowerBuffer[0] = ExtMovingBuffer[0] - Deviations * stdDev;
            //---
        }


        protected double StdDev_Func(int position, DataSeries price, double averageValue, int period)
        {

            //--- variables
            double stdDevDTmp = 0.0;
            //--- check for position
            if (position <= price.Count - Period)
            {
                //--- calcualte StdDev
                for (int i = 0; i < period; i++)
                    stdDevDTmp += Math.Pow(price[position + i] - averageValue, 2);
                stdDevDTmp = Math.Sqrt(stdDevDTmp / period);
            }
            //--- return calculated value
            return stdDevDTmp;
        }
    }
}
