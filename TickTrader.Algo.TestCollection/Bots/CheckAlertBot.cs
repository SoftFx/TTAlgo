using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Alert Bot", Version = "1.0.0", Category = "Test Plugin Info")]
    public class CheckAlertBot : TradeBot
    {
        [Parameter(DisplayName = "Delay (ms)", DefaultValue = 100)]
        public int DelayMc { get; set; }

        [Parameter(DisplayName = "Message", DefaultValue = "I'm Alert bot!")]
        public string Message { get; set; }

        private int _count = 0;
        private bool ok = false;

        protected async override void OnStart()
        {
            while (!ok)
            {
                await Task.Delay(DelayMc);
                Alert.Print($"{Message} {++_count}");
            }
        }

        protected override void OnStop()
        {
            ok = false;
        }

        protected override void OnQuote(Quote quote)
        {

        }
    }
}
