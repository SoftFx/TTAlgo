using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TickTrader.Algo.Api;


namespace TickTrader.Algo.Indicators.Accelerator
{
    public class Accelerator : Indicator
    {
        [Parameter(DefaultValue = 5.0)]
        public double PeriodFast { get; set; }

        [Parameter(DefaultValue = 34.0)]
        public double PeriodSlow { get; set; }

        [Parameter(DefaultValue = 38.0)]
        public double DataLimit { get; set; }


        [Input]
        public DataSeries<Bar> Bars { get; set; }


        [Output]
        public DataSeries ExtACBuffer { get; set; }

        [Output]
        public DataSeries ExtUpBuffer { get; set; }

        [Output]
        public DataSeries ExtDnBuffer { get; set; }

        private List<double> ExtMacdBuf;

        protected override void Calculate()
        {
            if(ExtMacdBuf==null)
                ExtMacdBuf=new List<double>(Bars.Count);
            //--- check for rates total
            if (Bars.Count <= DataLimit)
            {
                ExtUpBuffer[0] = Double.NaN;
                ExtDnBuffer[0] = Double.NaN;
                ExtACBuffer[0] = Double.NaN;
                return;
            }

            ExtMacdBuf.Add(Bars.Take(Convert.ToInt32(PeriodFast)).Select(b => (b.High+b.Low)/2).Average()
                - Bars.Take(Convert.ToInt32(PeriodSlow)).Select(b => (b.High + b.Low) / 2).Average());

            double Signal = 0.0;
            if (ExtMacdBuf.Count <= 5)
            {
                Signal = ExtMacdBuf.Take(5).Average();
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    Signal += ExtMacdBuf[ExtMacdBuf.Count - 1 - i];
                }
                Signal /= 5;
            }

            //--- signal line counted in the 2-nd additional buffer

            //--- dispatch values between 2 buffers
            bool up = true;

            double current = ExtMacdBuf[ExtMacdBuf.Count - 1] - Signal;
            if(ExtACBuffer[1]!=Double.NaN)
                up = current > ExtACBuffer[1];
            else
                up = current > 0.0;

            if (!up)
                {
                    ExtUpBuffer[0] = 0.0;
                    ExtDnBuffer[0] = current;
                }
                else
                {
                    ExtUpBuffer[0] = current;
                    ExtDnBuffer[0] = 0.0;
                }
                ExtACBuffer[0] = current;
            }

        }
}
