using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace  TickTrader.Algo.Indicators.Momentum
{
    [Indicator]
    public class Momentum : TickTrader.Algo.Api.Indicator
    {
        [Parameter(DefaultValue = 14.0)]
        public double Period { get; set; }

        [Input]
        public DataSeries Close { get; set; }


        [Output]
        public DataSeries ExtMomBuffer { get; set; }

       
        protected override void Calculate()
        {
            if (Close.Count < Period + 1)
            {
                ExtMomBuffer[0] = Double.NaN;
            }
            else
            {
                double a = Close[0];
                double b = Close[Convert.ToInt32(Period)];
                ExtMomBuffer[0] = Close[0] * 100 / Close[Convert.ToInt32(Period)];
            }

        }
    }


}
