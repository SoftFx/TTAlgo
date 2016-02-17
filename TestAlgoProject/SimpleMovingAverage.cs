using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [Indicator(DisplayName = "Simple Moving Average", IsOverlay = true)]
    public class SimpleMovingAverage : Indicator
    {
        [Parameter(DefaultValue = 5)]
        public int Window { get; set; }

        [Parameter(DefaultValue = 0.0)]
        public double Shift { get; set; }

        [Input]
        public DataSeries Input { get; set; }

        [Output]
        public DataSeries Output { get; set; }

        protected override void Calculate()
        {
            Output[0] = Input.Take(10).Average() + Shift;
        }
    }

    [Indicator]
    public class SlowSimpleMovingAverage : Indicator
    {
        [Input]
        public DataSeries Input { get; set; }

        [Output]    
        public DataSeries Output { get; set; }

        protected override void Calculate()
        {
            Output[0] = Input.Take(10).Average() + 0.01;
            System.Threading.Thread.Sleep(100);
        }
    }
}
