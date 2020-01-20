using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{

    [TradeBot(DisplayName = "[T] Main Token Test Bot", Version = "1.0", Category = "Test Bot Routine")]
    public class MainTokenTestBot : TradeBot
    {
        [Input]
        public DataSeries Token { get; set; }

        protected override void OnQuote(Quote quote)
        {
            var token = Token[0];

            string str = $"Token: {token}, Quote: {quote.Bid}";

            Print(str);

            if (token != quote.Bid)
                PrintError($"Error: {str}");
        }
    }
}
