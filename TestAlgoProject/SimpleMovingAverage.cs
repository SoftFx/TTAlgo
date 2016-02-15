using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [Indicator(DisplayName = "Simple Moving Average")]
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
            if (Output.Count >= Window)
                Output[0] = Input.Take(Window).Average() + Shift;
        }
    }

    [Indicator]
    public class SimpleTime : Indicator
    {
        [Parameter(DefaultValue = 1.1)]
        public double Scale { get; set; }

        [Input]
        public DataSeries<Bar> Input { get; set; }

        [Output]
        public DataSeries Output { get; set; }

        protected override void Calculate()
        {
            Output[0] = Input[0].High * Scale;
        }
    }

    [Indicator]
    public class TimeBased : Indicator
    {
        private SimpleTime simpleTimeIndicator;

        [Input]
        public DataSeries<Bar> Input { get; set; }

        [Output]
        public DataSeries Output { get; set; }

        protected override void Init()
        {
            simpleTimeIndicator = new SimpleTime() { Input = Input, Scale = 1.1 };
        }

        protected override void Calculate()
        {
            Output[0] = 2;
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
