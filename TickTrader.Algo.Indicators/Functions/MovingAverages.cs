using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.Functions
{
    public static class MovingAverages
    {
        public static double SimpleMA(int position, int period, List<double> price, int isSeries = 1)
        {
        //---
            double result = 0.0;
            //--- check position

            if (isSeries == 1)
            {
                if (position + period <= price.Count && period > 0)
                {
                    //--- calculate value

                    for (int i = 0; i < period; i++)
                        result += price[position + i];
                    result /= period;
                }
            }
            else
            {
                
               if (position >= period - 1 && period > 0)
               {
                    //--- calculate value
                    for (int i = 0; i < period; i++) result += price[position - i];
                    result /= period;
                    }
            }
        //---
           return(result);
          }


        public static double SmoothedMA(int position, int period, double prev_value, List<double> price, int isSeries = 1)
          {
        //---
            double result = 0.0;
        //--- check position
            if (price.Count == 91)
            {
                int a = 0;
            }

            if(period>0)
            {
                 if (isSeries == 1)
                 {
                    if (position == price.Count - period)
                    {
                        for (int i = 0; i < period; i++) result += price[position + i];
                        result /= period;
                    }
                    if (position < price.Count - period)
                        result = (prev_value * (period - 1) + price[position]) / period;
                }
                 else
                {
                    if (position==period-1)
                    {
                         for(int i = 0; i<period;i++) result+=price[position - i];
                         result/=period;
                    }
                    if (position >= period)
                        result = (prev_value * (period - 1) + price[position]) / period;
                }
              
             }
        //---
           return(result);
          }
            }
}
