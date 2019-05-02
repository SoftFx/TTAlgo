using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Doing Nothing Bot", Version = "1.0", Category = "Test Plugin Info",
        Description = "This bot has no code and no purpose.")]
    public class DoingNothingBot : TradeBot
    {
    }
}
