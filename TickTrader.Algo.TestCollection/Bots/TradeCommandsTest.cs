using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Trade Commands Test", Version = "1.0", Category = "Test Orders",
        Description = "Performs all possible order operations depedning on account type and verifies results. Uses both sync and async order commands.")]
    public class TradeCommandsTest : TradeBot
    {
        private int operationCount;

        protected async override void OnStart()
        {
            var sequences = new Task[10];

            for (int i = 0; i < 10; i++)
                sequences[i] = DoMarketSequence();

            await Task.WhenAll(sequences);

            Exit();
        }

        private async Task DoMarketSequence()
        {
            // open order

            OrderCmdResult result;

            if (++operationCount % 3 > 0)
                result = await OpenOrderAsync(Symbol.Name, OrderType.Market, OrderSide.Buy, 0.1, Symbol.Ask);
            else
                result = OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Buy, 0.1, Symbol.Ask);

            // check

            if (result.ResultCode == OrderCmdResultCodes.Ok)
            {
                var orderId = result.ResultingOrder.Id;
                var openedOrder = Account.Orders[orderId];
                if (openedOrder.IsNull)
                    PrintError("Cannot find order #" + orderId + " after opening!");
            }
        }

        private async void DoLimitSequence()
        {
            var price = Symbol.Ask + Symbol.Point * 500;
            var volume = 0.1;

            // open order

            OrderCmdResult result;

            if (++operationCount % 3 > 0)
                result = await OpenOrderAsync(Symbol.Name, OrderType.Limit, OrderSide.Buy, volume, price);
            else
                result = OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Buy, 0.1, Symbol.Ask);

            // check

            if (result.ResultCode == OrderCmdResultCodes.Ok)
            {
                var orderId = result.ResultingOrder.Id;
                var openedOrder = Account.Orders[orderId];
                if (openedOrder.IsNull)
                    PrintError("Cannot find order #" + orderId + " after opening!");
            }
        }
    }
}
