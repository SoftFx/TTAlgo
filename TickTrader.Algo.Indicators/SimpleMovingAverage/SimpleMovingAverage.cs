using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.SimpleMovingAverage
{
    [Indicator(IsOverlay = true)]
    public class MyIndicator : TickTrader.Algo.Api.Indicator
    {
        [Parameter(DefaultValue = 5.0)]
        public double Val { get; set; }

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
           
            ExtMovingBuffer[0] = Close[0];
            //--- calculate and write down StdDev
          
            //--- upper line
            
            ExtUpperBuffer[0] = Close[0] * 1.01;
            //--- lower line
  
            ExtLowerBuffer[0] = Close[0] * 0.99;
            //---
        }
    }


}
