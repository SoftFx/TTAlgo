using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Indicators.Functions;
namespace TickTrader.Algo.Indicators.Stochastic
{

    [Indicator]
    public class Stochastic : Indicator
    {


        [Parameter(DefaultValue = 5)]
        public int InpKPeriod { get; set; }

        [Parameter(DefaultValue = 3)]
        public int InpDPeriod { get; set; }

        [Parameter(DefaultValue = 3)]
        public int InpSlowing { get; set; }

        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtMainBuffer { get; set; }

        [Output]
        public DataSeries ExtSignalBuffer { get; set; }

        protected override void Calculate()
        {

            if (Bars.Count >= InpKPeriod + InpDPeriod + InpSlowing - 2)
            {
                List<double> MainBuf = new List<double>();
                for (int i = 0; i < InpDPeriod; i++)
                    MainBuf.Add(GetMainLineVal(i));
                ExtMainBuffer[0] = MainBuf[0];
                ExtSignalBuffer[0] = MovingAverages.SimpleMA(0, InpDPeriod, MainBuf);
            }

        }

        private double GetMaxHigh(int pos)
        {
            double res = 0.0;
            for (int i = pos; i < pos + InpKPeriod; i++)
            {
                if (res < Bars[i].High)
                {
                    res = Bars[i].High;
                }
            }
            return res;
        }

        private double GetMinLow(int pos)
        {
            double res = 100000000.0;
            for (int i = pos; i < pos + InpKPeriod; i++)
            {
                if (res > Bars[i].Low)
                {
                    res = Bars[i].Low;
                }
            }
            return res;
        }

        private double GetMainLineVal(int pos)
        {
            double res = 0.0;
            double sumlow = 0.0;
            double sumhigh = 0.0;
            for (int i = pos; i < pos + InpSlowing; i++)
            {
                double l = GetMinLow(i);
                double h = GetMaxHigh(i);

                sumlow += (Bars[i].Close - GetMinLow(i));
                sumhigh += (GetMaxHigh(i) - GetMinLow(i));
            }
            if (sumhigh == 0.0)
                res = 100.0;
            else
                res = sumlow / sumhigh * 100.0;
            return res;
        }




    }
}
