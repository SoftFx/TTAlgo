using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] All Trade Commands Test", Version = "1.0", Category = "Test Orders",
        Description = "Performs all possible order operations depedning on account type and verifies results. Uses both sync and async order commands.")]
    public class TradeCommandsTest : TradeBot
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        protected async override void OnStart()
        {
            try
            {
                TestSync();
                await TestAsync();
            }
            catch (Exception ex)
            {
                PrintError("Test failed: " + ex.Message);
            }

            Exit();
        }

        private void TestSync()
        {
            Print("Test - Open Limit");

            var limit = ThrowOnError(OpenOrder(Symbol.Name, OrderType.Limit, OrderSide.Sell, Volume, Symbol.Bid + Symbol.Point * 500));
            VerifyOrder(limit.Id, OrderType.Limit, OrderSide.Sell, Volume);

            Print("Test - Modify Limit");

            var newPrice = Symbol.RoundPriceUp(limit.Price + Symbol.Point * 2);
            ThrowOnError(ModifyOrder(limit.Id, newPrice));
            VerifyOrder(limit.Id, OrderType.Limit, OrderSide.Sell, Volume, newPrice);

            Print("Test - Cancel Limit");

            ThrowOnError(CancelOrder(limit.Id));

			Print("Test - Open Stop");

			var stop = ThrowOnError(OpenOrder(Symbol.Name, OrderType.Stop, OrderSide.Sell, Volume, Symbol.Bid - Symbol.Point * 1000));
			
			if (Account.Type == AccountTypes.Gross || Account.Type == AccountTypes.Net)
			{
				VerifyOrder(stop.Id, OrderType.Stop, OrderSide.Sell, Volume);
			}

			if (Account.Type == AccountTypes.Cash)
			{
				VerifyOrder(stop.Id, OrderType.StopLimit, OrderSide.Sell, Volume);
			}

			Print("Test - Modify Stop");

			var newStopPrice = Symbol.RoundPriceUp(stop.Price - Symbol.Point * 500);
			ThrowOnError(ModifyOrder(stop.Id, newStopPrice));

			if (Account.Type == AccountTypes.Gross || Account.Type == AccountTypes.Net)
			{
				VerifyOrder(stop.Id, OrderType.Stop, OrderSide.Sell, Volume);
			}

			Print("Test - Cancel Stop");

			ThrowOnError(CancelOrder(stop.Id));

			if (Account.Type == AccountTypes.Gross || Account.Type == AccountTypes.Net)
			{
				VerifyOrderDeleted(stop.Id);
			}
			
			Print("Test - Open Market 1");

            var pos1 = ThrowOnError(OpenMarketOrder(OrderSide.Buy, Volume * 2));
            
            if (Account.Type == AccountTypes.Gross)
            {
                VerifyOrder(pos1.Id, OrderType.Position, OrderSide.Buy, Volume * 2);

                Print("Test - Close Market 1 (partially)");

                ThrowOnError(CloseOrder(pos1.Id, Volume));
                VerifyOrder(pos1.Id, OrderType.Position, OrderSide.Buy, Volume);

                Print("Test - Close Market 1");

                ThrowOnError(CloseOrder(pos1.Id, Volume));
                VerifyOrderDeleted(pos1.Id);
            }

            Print("Test - Open Market 2");

            var pos2 = ThrowOnError(OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Sell, Volume, Symbol.Ask));
            
            if (Account.Type == AccountTypes.Gross)
            {
                VerifyOrder(pos2.Id, OrderType.Position, OrderSide.Sell, Volume);

                Print("Test - Open Market 3");

                var pos3 = ThrowOnError(OpenMarketOrder(Symbol.Name, OrderSide.Buy, Volume, Symbol.Ask));
                VerifyOrder(pos3.Id, OrderType.Position, OrderSide.Buy, Volume);

                Print("Test - Close Market 2 by Market 3");

                ThrowOnError(CloseOrderBy(pos2.Id, pos3.Id));

                VerifyOrderDeleted(pos2.Id);
                VerifyOrderDeleted(pos3.Id);
            }
        }

        private async Task TestAsync()
        {
            Print("Test - Open Limit Async");

            var limit = ThrowOnError(await OpenOrderAsync(Symbol.Name, OrderType.Limit, OrderSide.Sell, Volume, Symbol.Bid + Symbol.Point * 500));
            VerifyOrder(limit.Id, OrderType.Limit, OrderSide.Sell, Volume);

            Print("Test - Modify Limit Async");

            var newPrice = Symbol.RoundPriceUp(limit.Price + Symbol.Point * 2);
            ThrowOnError(await ModifyOrderAsync(limit.Id, newPrice));
            VerifyOrder(limit.Id, OrderType.Limit, OrderSide.Sell, Volume, newPrice);

            Print("Test - Cancel Limit Async");

            ThrowOnError(await CancelOrderAsync(limit.Id));
            Print("Test - Open Market 1 Async");

            var pos1 = ThrowOnError(await OpenOrderAsync(Symbol.Name, OrderType.Market, OrderSide.Buy, Volume * 2, Symbol.Ask));
            
            if (Account.Type == AccountTypes.Gross)
            {
                VerifyOrder(pos1.Id, OrderType.Position, OrderSide.Buy, Volume * 2);

                Print("Test - Close Market 1  Async (partially)");

                ThrowOnError(await CloseOrderAsync(pos1.Id, Volume));
                VerifyOrder(pos1.Id, OrderType.Position, OrderSide.Buy, Volume);

                Print("Test - Close Market 1 Async");

                ThrowOnError(CloseOrder(pos1.Id, Volume));
                VerifyOrderDeleted(pos1.Id);
            }

            Print("Test - Open Market 2 Async");

            var pos2 = ThrowOnError(await OpenOrderAsync(Symbol.Name, OrderType.Market, OrderSide.Sell, Volume, Symbol.Ask));

            if (Account.Type == AccountTypes.Gross)
            {
                VerifyOrder(pos2.Id, OrderType.Position, OrderSide.Sell, Volume);

                Print("Test - Open Market 3 Async");

                var pos3 = ThrowOnError(await OpenOrderAsync(Symbol.Name, OrderType.Market, OrderSide.Buy, Volume, Symbol.Ask));
                VerifyOrder(pos3.Id, OrderType.Position, OrderSide.Buy, Volume);

                Print("Test - Close Market 2 by Market 3 Async");

                ThrowOnError(await CloseOrderByAsync(pos2.Id, pos3.Id));

                VerifyOrderDeleted(pos2.Id);
                VerifyOrderDeleted(pos3.Id);
            }
        }

        private void VerifyOrder(string orderId, OrderType type, OrderSide side, double orderVolume, double? price = null)
        {
            var order = Account.Orders[orderId];
            if (order.IsNull)
                throw new ApplicationException("Verification failed - order #" + orderId + " does not exis in order collection!");

            if (order.Type != type)
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong order type!");

            if (order.Side != side)
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong side!");

            if (order.RemainingVolume != orderVolume)
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong volume!");

            if (price != null && !ComparisonExtensions.E(order.Price, price.Value))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong price!");
        }

        private void VerifyOrderDeleted(string orderId)
        {
            var order = Account.Orders[orderId];
            if (!order.IsNull)
                throw new ApplicationException("Verification failed - order #" + orderId + " still exist in order collection!");
        }

        private Order ThrowOnError(OrderCmdResult cmdResult)
        {
            if (cmdResult.ResultCode != OrderCmdResultCodes.Ok)
                throw new ApplicationException("Operation failed! Code=" + cmdResult.ResultCode);

            return cmdResult.ResultingOrder;
        }

        //protected async override void OnStart()
        //{
        //    var sequences = new Task[10];

        //    for (int i = 0; i < 10; i++)
        //        sequences[i] = DoMarketSequence();

        //    await Task.WhenAll(sequences);

        //    Exit();
        //}

        //private async Task DoMarketSequence()
        //{
        //     open order

        //    OrderCmdResult result;

        //    if (++operationCount % 3 > 0)
        //        result = await OpenOrderAsync(Symbol.Name, OrderType.Market, OrderSide.Buy, 0.1, Symbol.Ask);
        //    else
        //        result = OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Buy, 0.1, Symbol.Ask);

        //     check

        //    if (result.ResultCode == OrderCmdResultCodes.Ok)
        //    {
        //        var orderId = result.ResultingOrder.Id;
        //        var openedOrder = Account.Orders[orderId];
        //        if (openedOrder.IsNull)
        //            PrintError("Cannot find order #" + orderId + " after opening!");
        //    }
        //}

        //private async void DoLimitSequence()
        //{
        //    var price = Symbol.Ask + Symbol.Point * 500;
        //    var volume = 0.1;

        //     open order

        //    OrderCmdResult result;

        //    if (++operationCount % 3 > 0)
        //        result = await OpenOrderAsync(Symbol.Name, OrderType.Limit, OrderSide.Buy, volume, price);
        //    else
        //        result = OpenOrder(Symbol.Name, OrderType.Market, OrderSide.Buy, 0.1, Symbol.Ask);

        //     check

        //    if (result.ResultCode == OrderCmdResultCodes.Ok)
        //    {
        //        var orderId = result.ResultingOrder.Id;
        //        var openedOrder = Account.Orders[orderId];
        //        if (openedOrder.IsNull)
        //            PrintError("Cannot find order #" + orderId + " after opening!");
        //    }
        //}
    }
}
