using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    public enum Options { Option1, Option2 }

    [TradeBot(DisplayName = "Exit Bot")]
    public class ExitBot : TradeBot
    {
        protected override void OnStart()
        {
            Exit();
        }

        protected override void OnQuote(Quote quote)
        {
            Exit();
        }
    }
}
