using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace LMMBot
{
    [TradeBot]
    public class LMMBot : TradeBot
    {
        bool isStopRequested;

        protected override void OnStart()
        {
            foreach (var symbol in Symbols)
                MmLoop(symbol.Name);
            
        }

        private async void MmLoop(string smb)
        {
            while (!isStopRequested)
            {
                int quote = await Task.Factory.StartNew(() => 5);
                await OpenOrderAsync(smb, OrderType.Limit, OrderSide.Buy, 5, quote);
                await Task.Delay(100);
            }
        }

        protected override void OnStop()
        {
            isStopRequested = true;
        }
    }
}
