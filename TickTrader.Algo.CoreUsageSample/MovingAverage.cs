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
        public DataSeries<Bar> Input1 { get; set; }

        [Input]
        public DataSeries Input2 { get; set; }

        [Output]
        public DataSeries Output1 { get; set; }

        [Output]
        public DataSeries Output2 { get; set; }

        [Output]
        public DataSeries Output3 { get; set; }

        [Parameter]
        public int Range { get; set; }

        protected override void Init()
        {
            Account.Orders.Opened += Orders_Opened;
        } 

        private void Orders_Opened(OrderOpenedEventArgs obj)
        {
            var order1 = Account.Orders[11];
        }

        protected override void Calculate()
        {
            Output1[0] = Input1.Take(Range).Select(b => b.High).Average();
            Output2[0] = Input2.Take(Range).Average();
            Output3[0] = MarketSeries.Bars.High.Take(Range).Average();
        }
    }
}
