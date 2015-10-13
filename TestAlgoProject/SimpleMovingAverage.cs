using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [Indicator]
    public class SimpleMovingAverage : Indicator
    {
        [Parameter]
        public int Range { get; set; }

        [Input]
        public DataSeries Input { get; set; }

        [Output]
        public DataSeries Output { get; set; }

        protected override void Calculate()
        {
            Output[0] = Input.Take(Range).Average();
        }
    }
}
