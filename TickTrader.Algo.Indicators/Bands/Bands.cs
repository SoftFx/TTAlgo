using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Bands
{

    [Indicator]
    public class Bands : Indicator
    {
        [Parameter(DefaultValue = 20.0)]
        public double Period { get; set; }

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

            ExtMovingBuffer[0] = SimpleMovAv(0, Convert.ToInt32(Period), Close);
            //--- calculate and write down StdDev
            double stdDev = StdDev_Func(0, Close, ExtMovingBuffer, Convert.ToInt32(Period));
            //--- upper line
            ExtUpperBuffer[0] = ExtMovingBuffer[0] + Deviations * stdDev;
            //--- lower line
            ExtLowerBuffer[0] = ExtMovingBuffer[0] - Deviations * stdDev;
            //---
        }

        protected double SimpleMovAv(int position, int period, DataSeries price)
        {
            //---
            double result = 0.0;
            //--- check position
            if (position <= price.Count - Period && period > 0)
            {
                //--- calculate value
                for (int i = 0; i < period; i++)
                    result += price[position + i];
                result /= period;
            }
            //---
            return (result);
        }

        protected double StdDev_Func(int position, DataSeries price, DataSeries movAvPrice, int period)
        {

            //--- variables
            double stdDevDTmp = 0.0;
            //--- check for position
            if (position <= price.Count - Period)
            {
                //--- calcualte StdDev
                for (int i = 0; i < period; i++)
                    stdDevDTmp += Math.Pow(price[position + i] - movAvPrice[position], 2);
                stdDevDTmp = Math.Sqrt(stdDevDTmp / period);
            }
            //--- return calculated value
            return stdDevDTmp;
        }
    }
}
