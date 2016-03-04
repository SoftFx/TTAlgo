﻿using System;
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
            if (position >= 0)
            {
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
            }
            //---
           return(result);
          }


        public static double SmoothedMA(int position, int period, double prev_value, List<double> price, int isSeries = 1)
        {
        //---
            double result = 0.0;
        //--- check position

            if(period>0 && position >= 0)
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

        public static double SmoothedMAinPlace(int position, int period,  List<double> price, int isSeries = 1)
        {
            //---
            double result = 0.0;
            //--- check position
            double reg = 0;
            if (period > 0 && position >= 0)
            {
                double pr = 1.0 / period;
                double mul = 1;
                
                int i = position;
                while (i < price.Count && mul > 0.000000001)
                {
                    result += price[i] * pr * mul;
                    reg += pr*mul;
                    mul *= (1 - pr);
                    i++;
                }
            }
            //---
            return (result/reg);
        }

        public static double ExponentialMA(int position, int period, double prev_value, List<double> price, int isSeries = 1)
        {
            //---
            double result = 0.0;
            //--- check position

            if (period > 0 && position >= 0)
            {
                double pr = 2.0 / (period + 1.0);
                if (position < price.Count)
                    result = prev_value * (1-pr) + price[position] *pr;
            }
            //---
            return (result);
        }

        public static double ExponentialMAinPlace(int position, int period, List<double> price, int isSeries = 1)
        {
            //---
            double result = 0.0;
            //--- check position



            if (period > 0 && position >= 0)
            {
                double wSum = 0;
                double pr = 2.0 / (period + 1.0);
                double mul = 1;
                int i = position;
                while (i < price.Count && mul > 0.000000001)
                {
                    wSum+= pr * mul;
                    result += price[i]*pr*mul;
                    mul *= (1 - pr);
                    i++;
                }
                result /= wSum;
            }
            //---
            return (result);
        }

        public static double LinearWMA(int position, int period, List<double> price, int isSeries = 1)
        {
            //---
            double result = 0.0;
            //--- check position

            if (position>=0 && period > 0 && price.Count >= position + period)
            {
                for (int i = 0; i < period; i++)
                {
                    double mul = 2.0/(period*(period + 1));
                    result += price[position+i] *mul*(period - i);
                }
            }
            //---
            return (result);
        }




    }
}
