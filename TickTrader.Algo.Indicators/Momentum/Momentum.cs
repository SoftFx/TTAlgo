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
        [Parameter(DefaultValue = 14)]
        public int Period { get; set; }

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
                double b = Close[Period];
                ExtMomBuffer[0] = Close[0] * 100 / Close[Period];
            }

        }
    }


}
