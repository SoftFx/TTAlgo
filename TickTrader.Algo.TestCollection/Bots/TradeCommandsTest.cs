using System;
using System.Collections.Generic;
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

        private int _testCount;
<<<<<<< HEAD
        private List<string> _errors = new List<string>();
=======
        private int _errorCount;
        private List<string> _errorTextList;
        private double _diff;
>>>>>>> origin

        protected override async void OnStart()
        {
            await Test();
            Exit();
        }

        private void InitSidesAndTypes(ICollection<OrderSide> sides, ICollection<OrderType> types)
        {
            sides.Add(OrderSide.Buy);
            sides.Add(OrderSide.Sell);

            if (Account.Type == AccountTypes.Gross)
                types.Add(OrderType.Market);
            types.Add(OrderType.Stop);
            types.Add(OrderType.Limit);
            types.Add(OrderType.StopLimit);
        }

        private double[] GetPrices(OrderType orderType, OrderSide orderSide)
        {
            double price;
            double secondPrice;
            double newPrice;

            if (orderSide == OrderSide.Buy)
            {
                price = secondPrice = newPrice = Symbol.Ask;

                if (orderType == OrderType.Limit)
                {
<<<<<<< HEAD
                    price -= Symbol.Point * 1000;
                    secondPrice -= Symbol.Point * 500;
                    newPrice -= Symbol.Point * 250;
                }
                else
                {
                    price += Symbol.Point * 1000;
                    secondPrice += Symbol.Point * 500;
                    newPrice += Symbol.Point * 250;
=======
                    price -= _diff * 3;
                    secondPrice -= _diff * 2;
                    newPrice -= _diff;
                }
                else
                {
                    price += _diff * 3;
                    secondPrice += _diff * 2;
                    newPrice += _diff;
>>>>>>> origin
                }
            }
            else
            {
                price = secondPrice = newPrice = Symbol.Bid;

                if (orderType == OrderType.Limit)
                {
<<<<<<< HEAD
                    price += Symbol.Point * 1000;
                    secondPrice += Symbol.Point * 500;
                    newPrice += Symbol.Point * 250;
                }
                else
                {
                    price -= Symbol.Point * 1000;
                    secondPrice -= Symbol.Point * 5000;
                    newPrice -= Symbol.Point * 2500;
=======
                    price += _diff * 3;
                    secondPrice += _diff * 2;
                    newPrice += _diff;
                }
                else
                {
                    price -= _diff * 3;
                    secondPrice -= _diff * 2;
                    newPrice -= _diff;
>>>>>>> origin
                }

            }
            double[] result = {price, secondPrice, newPrice};
            return result;
        }

        private async Task PerfomOrder(OrderType orderType, OrderSide orderSide, bool isAsync, OrderExecOptions options = OrderExecOptions.None, string tag = null)
        {
            
            var prices = GetPrices(orderType, orderSide);
            var title = (isAsync) ? "Async test: " : "Test: ";
            var postTitle = (tag == null) ? "" : " with tag";
            postTitle += (options == OrderExecOptions.ImmediateOrCancel) ? " with IoC" : "";
            double? price = null;
            double? secondPrice = null;
            var newPrice = prices[2];
            if (orderType != OrderType.Stop && orderType != OrderType.Market)
                price = prices[0];
            Order someOrder = null;

            if (orderType != OrderType.Limit && orderType != OrderType.Market)
                secondPrice = prices[1];
            if (options == OrderExecOptions.ImmediateOrCancel)
                    price = (orderSide == OrderSide.Sell) ? Symbol.Bid - _diff : Symbol.Ask + _diff;

<<<<<<< HEAD
            if (orderType != OrderType.Market)
=======
            try
>>>>>>> origin
            {
                _testCount++;
                Print(title + "open " + orderSide + " " + orderType + " order" + postTitle);
                someOrder = (isAsync)
                    ? ThrowOnError(await OpenOrderAsync(Symbol.Name, orderType, orderSide, Volume, null, price,
                        secondPrice, null, null, null, options, tag))
                    : ThrowOnError(OpenOrder(Symbol.Name, orderType, orderSide, Volume, null, price, secondPrice, null,
                        null, null, options, tag));

                var realOrderType = (Account.Type == AccountTypes.Cash && orderType == OrderType.Stop)
                    ? OrderType.StopLimit
                    : orderType;
                realOrderType = (orderType == OrderType.Market) ? OrderType.Position : realOrderType;
                VerifyOrder(someOrder.Id, realOrderType, orderSide, Volume, price, secondPrice, null, null, null, null,
                    tag);

                if (options == OrderExecOptions.ImmediateOrCancel && OrderType.Limit == orderType)
                {
                    someOrder = null;
                    return;
                }

                await TestAddModifyComment(someOrder.Id, isAsync);
                if (Account.Type == AccountTypes.Gross)
                {
                    await TestAddModifyStopLoss(someOrder.Id, isAsync, postTitle);
                    await TestAddModifyTakeProfit(someOrder.Id, isAsync, postTitle);
                }

                if (orderType != OrderType.Market)
                {
                    await TestAddModifyExpiration(someOrder.Id, isAsync);

                    if (orderType != OrderType.Stop)
                    {
                        await TestAddModifyMaxVisibleVolume(someOrder.Id, isAsync, postTitle);
                        await TestModifyLimitPrice(someOrder.Id, newPrice, secondPrice, isAsync, postTitle);
                    }

                    if (orderType != OrderType.Limit)
                        await TestModifyStopPrice(someOrder.Id, price, newPrice, isAsync, postTitle);
                }
            }
            catch (Exception e)
            {
                _errorCount++;
                _errorTextList.Add(e.Message + " in " + title + orderSide + " " + orderType + " order" + postTitle);
            }
            finally
            {
                if(someOrder != null)
                    await TestCancelOrder(someOrder.Id, orderSide, orderType, tag, false, isAsync);
            }
        }

        private async Task Test()
        {

            const string tag = "TAG";
            var sides = new List<OrderSide>();
            var types = new List<OrderType>();
            InitSidesAndTypes(sides, types);
            bool[] asyncModes = {false, true};
            string[] tags = {null, tag};
<<<<<<< HEAD
            _errors = new List<string>();
=======
            _errorTextList = new List<string>();
            _diff = Math.Ceiling((Symbol.Ask / Symbol.Point) / 100 ) * Symbol.Point * 2;
            
>>>>>>> origin

            foreach (var orderSide in sides)
            {
                foreach (var orderType in types)
                {
                    foreach (var asyncMode in asyncModes)
                    {
                        foreach (var someTag in tags)
                        {
<<<<<<< HEAD
                            await DoOrderTest(orderType, orderSide, asyncMode, false, someTag);
                            if (orderType == OrderType.Limit || orderType == OrderType.StopLimit)
                                await DoOrderTest(orderType, orderSide, asyncMode, true, someTag);
                        }
                    }
                }
            }
=======
                            await PerfomOrder(orderType, orderSide, asyncMode, OrderExecOptions.None, someTag);
                            if (orderType == OrderType.StopLimit || orderType == OrderType.Limit)
                                await PerfomOrder(orderType, orderSide, asyncMode, OrderExecOptions.ImmediateOrCancel, someTag);   
                        }

            await TestMarketOrder(Account.Type);
>>>>>>> origin

            PrintStatus();
        }

        private async Task DoOrderTest(OrderType orderType, OrderSide orderSide, bool asyncMode, bool isIoc, string tag)
        {
            string testCfgDescription = (asyncMode ? "[a] " : " ")
                    + orderSide + " " + orderType + " "
                    + (isIoc ? "(IoC)" : "")
                    + (tag != null ? " +Tag" : "");

            Print("ORDER TEST: " + testCfgDescription);

            try
            {
                await PerfomOrder(orderType, orderSide, asyncMode, isIoc ? OrderExecOptions.ImmediateOrCancel : OrderExecOptions.None, tag);
            }
<<<<<<< HEAD
            catch (AggregateException ae)
            {
                OnError(testCfgDescription, ae.InnerException.Message);
            }
            catch (Exception e)
            {
                OnError(testCfgDescription, e.Message);
=======
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
                _errorTextList.Add(e.Message + " in test market order");
                
>>>>>>> origin
            }
        }

        private void OnError(string testName, string message)
        {
            _errors.Add(testName + ": " + message);
        }

<<<<<<< HEAD
        private void PrintStatus()
        {
            var stringBuilder = new StringBuilder();
            foreach (var str in _errors)
                stringBuilder.AppendLine(str);

            if (_errors.Count == 0)
                Status.WriteLine("Test success: " + _testCount + " passed");
            else
            {
                Status.WriteLine("Test failed: " + _errors.Count + " errors in " + _testCount + " tests");
                Status.WriteLine(stringBuilder.ToString());
            }
        }

        private async Task TestAddModifyComment(string orderId, bool isAsync)
=======
        private async Task TestAddModifyComment(string orderId, bool isAsync, string postTitle = "")
>>>>>>> origin
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
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, comment);

            _testCount++;
            Print(title + "modify comment " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, newComment));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, newComment));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, newComment);

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
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, null, null, null, null, null, expiration);

            _testCount++;
            Print(title + "modify expiration " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, null, newExpiration));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, null, newExpiration));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, null, null, null, null, null, newExpiration);

        }

        private async Task TestAddModifyMaxVisibleVolume(string orderId, bool isAsync, string postTitle = "")
        {
            var order = Account.Orders[orderId];
            var maxVisibleVolume = Volume;
            var newMaxVisibleVolume = Volume * 0.1;
            var title = (isAsync) ? "Async test: " : "Test: ";

            _testCount++;
            Print(title + "add maxVisibleVolume " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, maxVisibleVolume, null, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, maxVisibleVolume, null, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, null, maxVisibleVolume);

            order = Account.Orders[orderId];

            _testCount++;
            Print(title + "modify maxVisibleVolume " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, newMaxVisibleVolume, null, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, newMaxVisibleVolume, null, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, null, newMaxVisibleVolume);

        }

        private async Task TestAddModifyTakeProfit(string orderId, bool isAsync, string postTitle = "")
        {
            var order = Account.Orders[orderId];
<<<<<<< HEAD
            var takeProfit = (order.Side == OrderSide.Buy) ? (Symbol.Ask + Symbol.Point * 10) : (Symbol.Bid - Symbol.Point * 10);
            var newTakeProfit = (order.Side == OrderSide.Buy) ? (Symbol.Ask + Symbol.Point * 15) : (Symbol.Bid - Symbol.Point * 15);
=======
            var takeProfit = (order.Side == OrderSide.Buy) ? (Symbol.Ask + _diff * 4) : (Symbol.Bid - _diff * 4);
            var newTakeProfit = (order.Side == OrderSide.Buy) ? (Symbol.Ask + _diff * 5) : (Symbol.Bid - _diff * 5);
>>>>>>> origin
            var title = (isAsync) ? "Async test: " : "Test: ";

            _testCount++;
            Print(title + "add takeProfit " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, takeProfit, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, takeProfit, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, null, null, takeProfit);

            _testCount++;
            Print(title + "modify takeProfit " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, newTakeProfit, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, null, newTakeProfit, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, null, null, newTakeProfit);

        }

        private async Task TestAddModifyStopLoss(string orderId, bool isAsync, string postTitle = "")
        {
            var order = Account.Orders[orderId];
<<<<<<< HEAD
            var stopLoss = (order.Side == OrderSide.Buy) ? (Symbol.Ask - Symbol.Point * 10) : (Symbol.Bid + Symbol.Point * 10);
            var newStopLoss = (order.Side == OrderSide.Buy) ? (Symbol.Ask - Symbol.Point * 15) : (Symbol.Bid + Symbol.Point * 15);
=======
            var stopLoss = (order.Side == OrderSide.Buy) ? (Symbol.Ask - _diff) : (Symbol.Bid + _diff);
            var newStopLoss = (order.Side == OrderSide.Buy) ? (Symbol.Ask - _diff) : (Symbol.Bid + _diff);
>>>>>>> origin
            var title = (isAsync) ? "Async test: " : "Test: ";

            _testCount++;
            Print(title + "add stopLoss " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, stopLoss, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, stopLoss, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, null, null, null, stopLoss);

            _testCount++;
            Print(title + "modify stopLoss " + order.Side + " " + order.Type + " order " + postTitle);
            if (isAsync)
                ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, newStopLoss, null, null));
            else
                ThrowOnError(ModifyOrder(order.Id, null, null, null, newStopLoss, null, null));
            VerifyOrder(order.Id, order.Type, order.Side, Volume, null, null, null, null, null, newStopLoss);

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
            VerifyOrder(order.Id, order.Type, order.Side, Volume, newPrice, stopPrice);
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
            VerifyOrder(order.Id, order.Type, order.Side, Volume, price, newStopPrice);
        }

        private async Task TestCancelOrder(string orderId, OrderSide orderSide, OrderType orderType, string tag,  bool isIoc, bool isAsync)
        {
            if (Account.Orders[orderId] == null)
                return;

            _testCount++;

            Print((isAsync ? "Async test: " : "Test: ")
                + "cancel " + orderSide + " " + orderType + " order"                      
                + (tag != null ? " with tag" : "")      
                + (isIoc ? " with IoC" : ""));

            if (Account.Orders[orderId].Type == OrderType.Position)
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
            double orderVolume,
            double? price = null,
            double? stopPrice = null,
            string comment = null,
            double? maxVisibleVolume = null,
            double? takeProfit = null,
            double? stopLoss = null,
            string tag = null,
            DateTime? expiration = null)

        {
            var order = Account.Orders[orderId];
            if (order.IsNull)
                throw new ApplicationException("Verification failed - order #" + orderId + " does not exis in order collection");

            if (order.Type != type)
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong order type: " + type);

            if (order.Side != side)
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong side: " + side);

            if (!order.RemainingVolume.E(orderVolume))
                throw new ApplicationException("Verification failed - order #" + orderId + " has wrong volume");

            if (price != null && !EqualPrices(order.Price, price.Value))
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

        private void VerifyOrderDeleted(string orderId)
        {
            var order = Account.Orders[orderId];
            if (!order.IsNull)
                throw new ApplicationException("Verification failed - order #" + orderId + " still exist in order collection!");
        }
    }
}
