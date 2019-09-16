using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] All Trade Commands Test", Version = "1.1", Category = "Test Orders",
        Description = "Performs all possible order operations depedning on account type and verifies results. Uses both sync and async order commands.")]
    public class TradeCommandsTest : TradeBot
    {
        [Parameter(DefaultValue = 0.1)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 100)]
        public int PriceDelta { get; set; }

        private double _defaultVolume;

        private int _testCount;
        private int _errorCount;
        private List<string> _errorTextList;
        private double _diff;

        protected override async void OnStart()
        {
            await Test();
            Exit();
        }

        private void InitSidesAndTypes(ICollection<OrderSide> sides, ICollection<OrderType> types)
        {
            sides.Add(OrderSide.Buy);
            sides.Add(OrderSide.Sell);

            //if (Account.Type == AccountTypes.Gross)
            types.Add(OrderType.Market);
            types.Add(OrderType.Stop);
            types.Add(OrderType.Limit);
            types.Add(OrderType.StopLimit);
        }

        private void GetPrices(OrderType orderType, OrderSide orderSide, OrderExecOptions options, out double? price, out double? stopPrice,
            out double? modifyPrice, out double? modifyStopPrice)
        {
            bool isIoc = orderType == OrderType.Limit && options == OrderExecOptions.ImmediateOrCancel;
            bool isInstantOrder = orderType == OrderType.Market || isIoc;

            price = stopPrice = modifyPrice = modifyStopPrice = null;

            if (orderSide == OrderSide.Buy)
            {
                var basePrice = Symbol.Ask;

                if (isInstantOrder)
                    price = Symbol.Ask + _diff;
                else
                {
                    if (orderType == OrderType.Limit)
                    {
                        price = Symbol.Ask - _diff * 3;
                        modifyPrice = Symbol.Ask - _diff;
                    }
                    if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                    {
                        stopPrice = Symbol.Ask + _diff * 2;
                        modifyStopPrice = Symbol.Ask + _diff;
                    }
                    if (orderType == OrderType.StopLimit)
                    {
                        price = Symbol.Ask + _diff * 3;
                        modifyPrice = Symbol.Ask + _diff * 4;
                    }
                }   
            }
            else
            {
                if (isInstantOrder)
                    price = Symbol.Bid - _diff;
                else
                {
                    if (orderType == OrderType.Limit)
                    {
                        price = Symbol.Bid + _diff * 3;
                        modifyPrice = Symbol.Bid + _diff;
                    }
                    if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                    {
                        stopPrice = Symbol.Bid - _diff * 2;
                        modifyStopPrice = Symbol.Bid - _diff;
                    }
                    if (orderType == OrderType.StopLimit)
                    {
                        price = Symbol.Bid - _diff * 3;
                        modifyPrice = Symbol.Bid - _diff * 4;
                    }
                }
            }
        }

        private async Task PerfomOrder(OrderType orderType, OrderSide orderSide, bool isAsync, OrderExecOptions options = OrderExecOptions.None, string tag = null)
        {
            bool isMarket = orderType == OrderType.Market;
            bool isIoc = orderType == OrderType.Limit && options == OrderExecOptions.ImmediateOrCancel;
            bool isInstantOrder = isMarket || isIoc;

            GetPrices(orderType, orderSide, options, out var price, out var stopPrice, out var newPrice, out var newStopPrice);
            var title = (isAsync) ? "Async test: " : "Test: ";
            var postTitle = (tag == null) ? "" : " with tag";
            postTitle += (options == OrderExecOptions.ImmediateOrCancel) ? " with IoC" : "";
            Order accOrder = null;

            try
            {
                _testCount++;
                Print(title + "open " + orderSide + " " + orderType + " order" + postTitle);
                accOrder = (isAsync)
                    ? ThrowOnError(await OpenOrderAsync(Symbol.Name, orderType, orderSide, Volume, null, price,
                        stopPrice, null, null, null, options, tag))
                    : ThrowOnError(OpenOrder(Symbol.Name, orderType, orderSide, Volume, null, price, stopPrice, null,
                        null, null, options, tag));

                var realOrderType = orderType;
                if (Account.Type == AccountTypes.Cash && orderType == OrderType.Stop)
                    realOrderType = OrderType.StopLimit;
                else if (Account.Type == AccountTypes.Gross && isInstantOrder)
                    realOrderType = OrderType.Position;

                if (!isInstantOrder || Account.Type == AccountTypes.Gross)
                    VerifyOrder(accOrder.Id, realOrderType, orderSide, Volume, price, isInstantOrder, stopPrice, null, null, null, null, tag);

                if (!isInstantOrder)
                {
                    Volume *= 2;
                    await TestModifyVolume(accOrder.Id, isAsync, Volume, postTitle);
                }

                if (!isInstantOrder || Account.Type == AccountTypes.Gross)
                    await TestAddModifyComment(accOrder.Id, isAsync, postTitle);

                if (Account.Type == AccountTypes.Gross)
                {
                    await TestAddModifyStopLoss(accOrder.Id, isAsync, postTitle);
                    await TestAddModifyTakeProfit(accOrder.Id, isAsync, postTitle);
                }

                if (!isInstantOrder)
                {
                    await TestAddModifyExpiration(accOrder.Id, isAsync);

                    if (orderType != OrderType.Stop)
                    {
                        await TestAddModifyMaxVisibleVolume(accOrder.Id, isAsync, postTitle);
                        await TestModifyLimitPrice(accOrder.Id, newPrice.Value, stopPrice, isAsync, postTitle);
                    }

                    if (orderType != OrderType.Limit)
                        await TestModifyStopPrice(accOrder.Id, price, newStopPrice.Value, isAsync, postTitle);

                    if (accOrder != null)
                        await TestCancelOrder(accOrder.Id, orderSide, orderType, tag, false, isAsync);
                }
            }
            catch (Exception e)
            {
                _errorCount++;
                _errorTextList.Add(e.Message + " in " + title + orderSide + " " + orderType + " order" + postTitle);
                PrintError(e.Message);
            }
            finally
            {
                Volume = _defaultVolume;
            }
        }

        private async Task Test()
        {
            _defaultVolume = Volume;
            const string tag = "TAG";
            var sides = new List<OrderSide>();
            var types = new List<OrderType>();
            InitSidesAndTypes(sides, types);
            bool[] asyncModes = { false, true };
            string[] tags = { null, tag };
            _errorTextList = new List<string>();
            _diff = PriceDelta * Symbol.Point;


            foreach (var orderSide in sides)
                foreach (var orderType in types)
                    foreach (var asyncMode in asyncModes)
                        foreach (var someTag in tags)
                        {
                            await PerfomOrder(orderType, orderSide, asyncMode, OrderExecOptions.None, someTag);
                            if (orderType == OrderType.StopLimit || orderType == OrderType.Limit)
                                await PerfomOrder(orderType, orderSide, asyncMode, OrderExecOptions.ImmediateOrCancel, someTag);
                        }

            //await TestMarketOrder(Account.Type);

            PrintStatus();
        }

        private void PrintStatus()
        {
            var stringBuilder = new StringBuilder();
            foreach (var str in _errorTextList)
                stringBuilder.AppendLine(str);

            if (_errorCount == 0)
            {
                var msg = "Test success: " + _testCount + " passed";
                Status.WriteLine(msg);
                Print(msg);
            }
            else
            {
                var msg = "Test failed: " + _errorCount + " errors in " + _testCount + " tests";
                Status.WriteLine(msg);
                Status.WriteLine(stringBuilder.ToString());
                PrintError(msg);
            }
        }

        private void OpenMarketOrderGross(OrderSide orderSide)
        {
            _testCount++;
            Print("Test - Open " + orderSide + " Market 1");
            var pos1 = ThrowOnError(OpenMarketOrder(orderSide, Volume * 2));
            VerifyOrder(pos1.Id, OrderType.Position, orderSide, Volume * 2);

            _testCount++;
            Print("Test - Close " + orderSide + " Market 1 (partially)");
            ThrowOnError(CloseOrder(pos1.Id, Volume));
            VerifyOrder(pos1.Id, OrderType.Position, orderSide, Volume);

            _testCount++;
            Print("Test - Close " + orderSide + " Market 1");
            ThrowOnError(CloseOrder(pos1.Id, Volume));
            VerifyOrderDeleted(pos1.Id);

            _testCount++;
            Print("Test - Open " + orderSide + " Market 2");
            var pos2 = ThrowOnError(OpenMarketOrder(orderSide, Volume));
            VerifyOrder(pos2.Id, OrderType.Position, orderSide, Volume);

            _testCount++;
            Print("Test - Open " + orderSide + " Market 3");
            var oppositeOrderSide = (orderSide == OrderSide.Sell) ? OrderSide.Buy : OrderSide.Sell;
            var pos3 = ThrowOnError(OpenMarketOrder(oppositeOrderSide, Volume));
            VerifyOrder(pos3.Id, OrderType.Position, oppositeOrderSide, Volume);

            _testCount++;
            Print("Test - Close " + orderSide + " Market 2 by " + oppositeOrderSide + " Market 3");
            ThrowOnError(CloseOrderBy(pos2.Id, pos3.Id));

            VerifyOrderDeleted(pos2.Id);
            VerifyOrderDeleted(pos3.Id);

        }

        private async Task TestMarketOrder(AccountTypes curType)
        {
            try
            {
                switch (curType)
                {
                    case AccountTypes.Gross:
                        OpenMarketOrderGross(OrderSide.Buy);
                        OpenMarketOrderGross(OrderSide.Sell);
                        break;

                    case AccountTypes.Net:
                        await OpenMarketOrderNet();
                        break;

                    case AccountTypes.Cash:
                        break;

                    default:
                        throw new Exception("Invalid account type " + Account.Type);
                }
            }
            catch (Exception e)
            {
                _errorCount++;
                _errorTextList.Add(e.Message + "     market order");

            }
        }

        private async Task OpenMarketOrderNet()
        {
            _testCount++;
            Print("Test - Open " + OrderSide.Buy + " Market 1");
            ThrowOnError(OpenMarketOrder(OrderSide.Buy, Volume));

            _testCount++;
            Print("Test - Open opposite " + OrderSide.Sell + " Market 2");
            ThrowOnError(OpenMarketOrder(OrderSide.Sell, Volume));

            _testCount++;
            Print("Async test - Open " + OrderSide.Buy + " Market 3");
            ThrowOnError(await OpenOrderAsync(Symbol.Name, OrderType.Market, OrderSide.Buy, Volume, null, Symbol.Ask, null));

            _testCount++;
            Print("Async test - Open opposite " + OrderSide.Sell + " Market 4");
            ThrowOnError(await OpenOrderAsync(Symbol.Name, OrderType.Market, OrderSide.Sell, Volume, null, Symbol.Bid, null));
        }

        private async Task TestAddModifyComment(string orderId, bool isAsync, string postTitle = "")
        {
            const string comment = "Comment";
            const string newComment = "New comment";
            var title = (isAsync) ? "Async test: " : "Test: ";
            var order = Account.Orders[orderId];

            _testCount++;
            Print(title + "add comment " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, comment));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, comment));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, comment);

            _testCount++;
            Print(title + "modify comment " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, newComment));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, newComment));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, newComment);

        }

        private async Task TestAddModifyExpiration(string orderId, bool isAsync, string postTitle = "")
        {
            var year = DateTime.Today.Year;
            var month = DateTime.Today.Month;
            var day = DateTime.Today.Day;
            var expiration = new DateTime(year + 1, month, day);
            var newExpiration = new DateTime(year + 2, month, day);

            var title = (isAsync) ? "Async test: " : "Test: ";
            var order = Account.Orders[orderId];

            _testCount++;
            Print(title + "add expiration " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, null, expiration));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, null, expiration));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, null, null, null, null, null, expiration);

            _testCount++;
            Print(title + "modify expiration " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, null, newExpiration));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, null, newExpiration));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, null, null, null, null, null, newExpiration);

        }

        private async Task TestAddModifyMaxVisibleVolume(string orderId, bool isAsync, string postTitle = "")
        {
            var order = Account.Orders[orderId];
            var maxVisibleVolume = Volume;
            var newMaxVisibleVolume = Symbol.MinTradeVolume;
            var title = (isAsync) ? "Async test: " : "Test: ";

            _testCount++;
            Print(title + "add maxVisibleVolume " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, maxVisibleVolume, null, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, maxVisibleVolume, null, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, null, maxVisibleVolume);

            order = Account.Orders[orderId];

            _testCount++;
            Print(title + "modify maxVisibleVolume " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, newMaxVisibleVolume, null, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, newMaxVisibleVolume, null, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, null, newMaxVisibleVolume);

        }

        private async Task TestAddModifyTakeProfit(string orderId, bool isAsync, string postTitle = "")
        {
            var order = Account.Orders[orderId];
            var takeProfit = (order.Side == OrderSide.Buy) ? (Symbol.Ask + _diff * 4) : (Symbol.Bid - _diff * 4);
            var newTakeProfit = (order.Side == OrderSide.Buy) ? (Symbol.Ask + _diff * 5) : (Symbol.Bid - _diff * 5);
            var title = (isAsync) ? "Async test: " : "Test: ";

            _testCount++;
            Print(title + "add takeProfit " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, takeProfit, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, takeProfit, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, null, null, takeProfit);

            _testCount++;
            Print(title + "modify takeProfit " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, newTakeProfit, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, newTakeProfit, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, null, null, newTakeProfit);

        }

        private async Task TestAddModifyStopLoss(string orderId, bool isAsync, string postTitle = "")
        {
            var order = Account.Orders[orderId];
            var stopLoss = (order.Side == OrderSide.Buy) ? (Symbol.Bid - _diff * 4) : (Symbol.Ask + _diff * 4);
            var newStopLoss = (order.Side == OrderSide.Buy) ? (Symbol.Bid - _diff * 5) : (Symbol.Ask + _diff * 5);
            var title = (isAsync) ? "Async test: " : "Test: ";

            _testCount++;
            Print(title + "add stopLoss " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, stopLoss, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, stopLoss, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, null, null, null, stopLoss);

            _testCount++;
            Print(title + "modify stopLoss " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, newStopLoss, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, newStopLoss, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, false, null, null, null, null, newStopLoss);

        }

        private async Task TestModifyVolume(string orderId, bool isAsync, double newVolume, string postTitle = "")
        {
            var title = (isAsync) ? "Async test: " : "Test: ";
            var order = Account.Orders[orderId];

            _testCount++;
            Print(title + "modify volume " + order.Side + " " + order.Type + " order" + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, null, null, newVolume));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, null, null, newVolume));
            VerifyOrder(order.Id, order.Type, order.Side, newVolume, null, false, null);
        }

        private async Task TestModifyLimitPrice(string orderId, double newPrice, double? stopPrice, bool isAsync, string postTitle = "")
        {
            var title = (isAsync) ? "Async test: " : "Test: ";
            var order = Account.Orders[orderId];

            _testCount++;
            Print(title + "modify limit price " + order.Side + " " + order.Type + " order" + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, newPrice, stopPrice, null, null, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, newPrice, stopPrice, null, null, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, newPrice, false, stopPrice);
        }

        private async Task TestModifyStopPrice(string orderId, double? price, double newStopPrice, bool isAsync, string postTittle = "")
        {
            var title = (isAsync) ? "Async test: " : "Test: ";
            var order = Account.Orders[orderId];

            _testCount++;
            Print(title + "modify stopPrice " + order.Side + " " + order.Type + " order" + postTittle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, price, newStopPrice, null, null, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, price, newStopPrice, null, null, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, price, false, newStopPrice);
        }

        private async Task TestCancelOrder(string orderId, OrderSide orderSide, OrderType orderType, string tag, bool isIoc, bool isAsync)
        {
            if (Account.Orders[orderId] == null)
                return;

            _testCount++;

            Print((isAsync ? "Async test: " : "Test: ")
                + "cancel " + orderSide + " " + orderType + " order"
                + (tag != null ? " with tag" : "")
                + (isIoc ? " with IoC" : ""));

            if (Account.Orders[orderId].Type == OrderType.Position || Account.Orders[orderId].Type == OrderType.Market)
            {
                if (isAsync)
                    ThrowOnError(await CloseOrderAsync(orderId));
                else
                    ThrowOnError(CloseOrder(orderId));
                VerifyOrderDeleted(orderId);
            }
            else
            {
                _testCount++;
                if (isAsync)
                    ThrowOnError(await CancelOrderAsync(orderId));
                else
                    ThrowOnError(CancelOrder(orderId));
                VerifyOrderDeleted(orderId);
            }
        }

        private static Order ThrowOnError(OrderCmdResult cmdResult)
        {
            if (cmdResult.ResultCode != OrderCmdResultCodes.Ok)
                throw new ApplicationException("Operation failed! Code=" + cmdResult.ResultCode);

            return cmdResult.ResultingOrder;
        }

        private void VerifyOrder(
            string orderId,
            OrderType type,
            OrderSide side,
            double? orderVolume = null,
            double? price = null,
            bool isInstantOrder = false,
            double? stopPrice = null,
            string comment = null,
            double? maxVisibleVolume = null,
            double? takeProfit = null,
            double? stopLoss = null,
            string tag = null,
            DateTime? expiration = null)

        {
            var order = Account.Orders[orderId];

            if (type == OrderType.Position && !(Account.Type == AccountTypes.Gross)) // only in case of Gross account type market orders appear in Orders collection
                return;

            if (order.IsNull)
                throw new ApplicationException("Verification failed - order #" + orderId + " does not exis in order collection");

            if (order.Type != type)
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong order type: " + type);

            if (order.Side != side)
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong side: " + side);

            if (orderVolume != null && !order.RemainingVolume.E(orderVolume.Value))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong volume");

            if (price != null && !CheckPrice(price.Value, order.Price, side, isInstantOrder))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong price: required = " + price + ", current = " + order.Price);

            if (stopPrice != null && !EqualPrices(order.StopPrice, stopPrice.Value))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong stopPrice: required = " + stopPrice + ", current = " + order.StopPrice);

            if (comment != null && !comment.Equals(order.Comment))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong comment: " + comment);

            if (maxVisibleVolume != null && !order.MaxVisibleVolume.E(maxVisibleVolume.Value))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong maxVisibleVolume");

            if (takeProfit != null && !EqualPrices(order.TakeProfit, takeProfit.Value))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong takeProfit: required = " + takeProfit + ", current = " + order.TakeProfit);

            if (stopLoss != null && !EqualPrices(order.StopLoss, stopLoss.Value))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong stopLoss: required = " + stopLoss + ", current = " + order.StopLoss);

            if (tag != null && !tag.Equals(order.Tag))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong tag: " + tag);

            if (expiration != null && !order.Expiration.Equals(expiration))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong expiration: " + expiration);
        }

        private bool EqualPrices(double a, double b)
        {
            return Math.Abs(a - b) <= Symbol.Point;
        }

        private bool CheckPrice(double expectedPrice, double actualPrice, OrderSide side, bool isInstantOrder)
        {
            if (isInstantOrder)
            {
                if (side == OrderSide.Buy)
                    return actualPrice.Lte(expectedPrice);
                else
                    return actualPrice.Gte(expectedPrice);
            }
            else
                return EqualPrices(expectedPrice, actualPrice);
        }

        private void VerifyOrderDeleted(string orderId)
        {
            var order = Account.Orders[orderId];
            if (!order.IsNull)
                throw new ApplicationException("Verification failed - order #" + orderId + " still exist in order collection!");
        }
    }
}
