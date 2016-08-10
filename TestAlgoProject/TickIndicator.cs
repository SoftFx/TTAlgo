using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [Indicator(DisplayName = "Test Tick Indicator", IsOverlay = true)]
    public class TickIndicator : Indicator
    {
        [Input]
        public DataSeries<QuoteL2> QuoteInput { get; set; }

        //[Input]
        //public QuoteL2Series QuoteInputL2 { get; set; }

        [Output]
        public DataSeries Out1 { get; set; }

        protected override void Calculate()
        {
            Out1[0] = (QuoteInput[0].Ask + QuoteInput[0].Bid) / 2;
        }
    }
}
