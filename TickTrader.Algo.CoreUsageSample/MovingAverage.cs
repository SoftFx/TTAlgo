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
        public MovingAverage() { }

        public MovingAverage(DataSeries input, int period)
        {
            this.Input = input;
            this.Period = period;
        }

        [Input]
        public DataSeries Input { get; set; }

        [Output]
        public DataSeries Output { get; set; }

        [Parameter]
        public int Period { get; set; }

        protected override void Calculate()
        {
            if (Input.Count >= Period)
            {
                Output[0] = Input.Take(Period).Average();
            }
            else
            {
                Output[0] = double.NaN;
            }
        }
    }
}
