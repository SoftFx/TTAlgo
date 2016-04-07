using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreUsageSample
{
    [Indicator]
    public class MovingAverage : Indicator
    {
        [Input]
        public DataSeries Input { get; set; }

        [Output]
        public DataSeries Output { get; set; }

        [Parameter]
        public int Period { get; set; }

        protected override void Calculate()
        {
            Output[0] = Input.Take(Period).Average();
        }
    }
}
