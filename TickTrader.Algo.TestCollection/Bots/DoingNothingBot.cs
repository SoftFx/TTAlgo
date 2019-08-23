using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Doing Nothing Bot", Version = "1.1", Category = "Test Plugin Info",
        Description = "This bot has no code and no purpose.")]
    public class DoingNothingBot : TradeBot
    {
        [Parameter]
        public int IgnoredParam1 { get; set; }

        [Parameter]
        public double IgnoredParam2 { get; set; }
    }
}
