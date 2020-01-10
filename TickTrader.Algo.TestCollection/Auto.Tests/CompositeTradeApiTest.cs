using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    [TradeBot(DisplayName = "Composite Trade API Test", Version = "1.3", Category = "Auto Tests", SetupMainSymbol = true,
        Description = "")]
    public class CompositeTradeApiTest : TradeBot
    {
        private List<OrderVerifier> _tradeRepVerifiers = new List<OrderVerifier>();
        private TaskCompletionSource<object> _eventWaiter;
        private int _testCount;
        private int _errorCount;

        private List<OrderSide> _orderSides;
        private List<OrderType> _orderTypes;

        private readonly TimeSpan OpenEventTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan ActivateEventTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan FillEventTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan ModifyEventTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan TPSLEventTimeout = TimeSpan.FromSeconds(20);
        private readonly TimeSpan CancelEventTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan CloseEventTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan ExpirationEventTimeout = TimeSpan.FromSeconds(25);
        private readonly TimeSpan PauseBetweenOpenOrders = TimeSpan.FromSeconds(1);
        private readonly TimeSpan PauseBetweenOrders = TimeSpan.FromSeconds(1);

        private double CurrentVolume;

        [Parameter]
        public bool UseDealerCmdApi { get; set; }

        [Parameter]
        public bool TestADComments { get; set; }

        [Parameter(DefaultValue = 0.1)]
        public double BaseOrderVolume { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int PriceDelta { get; set; }

        protected async override void OnStart()
        {
            try
            {
                Init();

                const string tag = "TAG";
                bool[] asyncModes = { false, true };
                string[] tags = { null, tag };

                foreach (var orderSide in _orderSides)
                    foreach (var orderType in _orderTypes)
                        foreach (var asyncMode in asyncModes)
                            foreach (var someTag in tags)
                            {
                                try
                                {
                                    if (orderType != OrderType.Market)
                                        await PerfomAddModifyTests(orderType, orderSide, asyncMode, OrderExecOptions.None, someTag);

                                    await PerformExecutionTests(orderType, orderSide, asyncMode, someTag, OrderExecOptions.None);
                                    if (orderType == OrderType.StopLimit || orderType == OrderType.Limit)
                                        await PerformExecutionTests(orderType, orderSide, asyncMode, someTag, OrderExecOptions.ImmediateOrCancel);

                                    if (TestADComments && orderType != OrderType.Stop && orderType != OrderType.StopLimit)
                                    {
                                        await PerformCommentsTest(orderType, orderSide, asyncMode, someTag, OrderExecOptions.None);
                                        if (orderType == OrderType.Limit)
                                            await PerformCommentsTest(orderType, orderSide, asyncMode, someTag, OrderExecOptions.ImmediateOrCancel);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ++_errorCount;
                                    PrintError(ex.Message);
                                }

                                PrintStatus();
                            }

                // History test
                ReportsIteratorTest();
                await DoQueryTests(false);
                await DoQueryTests(true);

            }
            catch (Exception ex)
            {
                ++_errorCount;
                PrintError(ex.Message);
            }

            PrintStatus();

            Exit();
        }

        private void Init()
        {
            _testCount = 0;
            _errorCount = 0;

            Account.Orders.Opened += a => OnEventFired(a);
            Account.Orders.Filled += a => OnEventFired(a);
            Account.Orders.Closed += a => OnEventFired(a);
            Account.Orders.Expired += a => OnEventFired(a);
            Account.Orders.Canceled += a => OnEventFired(a);
            Account.Orders.Activated += a => OnEventFired(a);
            Account.Orders.Modified += a => OnEventFired(a);

            _orderSides = new List<OrderSide>();
            _orderTypes = new List<OrderType>();
            InitSidesAndTypes(_orderSides, _orderTypes);
        }

        private void PrintStatus()
        {
            Status.WriteLine($"Tests: {_testCount}, Errors: {_errorCount}");
            Status.WriteLine($"See logs for error details");
            Status.Flush();
        }

        #region Execution test

        private async Task PerformExecutionTests(OrderType orderType, OrderSide orderSide, bool asyncMode, string tag, OrderExecOptions options)
        {
            await TryPerformTest(() => TestFill(orderType, orderSide, asyncMode, tag, options));

            if (Account.Type == AccountTypes.Gross)
            {
                await TryPerformTest(() => TestTP(orderType, orderSide, asyncMode, tag, options));

                await TryPerformTest(() => TestSL(orderType, orderSide, asyncMode, tag, options));
            }

            if (!IsImmidiateFill(orderType, options))
            {
                await TryPerformTest(() => TestCancel(orderType, orderSide, asyncMode, tag, options));

                await TryPerformTest(() => TestExpire(orderType, orderSide, asyncMode, tag, options));
            }
        }

        private async Task TestFill(OrderType type, OrderSide side, bool asyncMode, string tag, OrderExecOptions options)
        {
            await Delay(PauseBetweenOpenOrders);

            var volume = BaseOrderVolume;
            var price = GetImmExecPrice(side, type);
            var stopPrice = GetImmExecStopPrice(side, type);

            Print($"Test: Fill with options {type}, {side}, {(asyncMode ? "async" : "non-async")}, tag = {tag}, options: {options}");

            var openVer = await OpenAndCheck(type, side, volume, price, asyncMode, tag, stopPrice: stopPrice, options: options);

            DateTime fillTime = openVer.TradeReportTimestamp;

            if (type == OrderType.StopLimit)
            {
                var activationArgs = await WaitEvent<OrderActivatedEventArgs>(ActivateEventTimeout);
                var actVer = openVer.Activate(activationArgs.Order.Modified);
                _tradeRepVerifiers.Add(actVer);

                actVer.VerifyEvent(activationArgs);

                var openArgs = await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);

                openVer = new OrderVerifier(openArgs.Order.Id, Account.Type, type, side, volume, price, null, openArgs.Order.Created);
            }

            OrderVerifier fillVer = null;

            if (Account.Type == AccountTypes.Gross)
            {
                if (!((type == OrderType.Limit || type == OrderType.StopLimit) && options == OrderExecOptions.ImmediateOrCancel))
                    fillTime = (await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout)).Order.Modified;
            }
            else
            {
                var fillArgs = await WaitEvent<OrderFilledEventArgs>(FillEventTimeout);
                fillTime = fillArgs.OldOrder.Modified;
                fillVer = openVer.Fill(fillTime);
                if (type != OrderType.StopLimit)
                    fillVer.VerifyEvent(fillArgs);
            }

            if (Account.Type == AccountTypes.Gross)
            {
                var closeResp = await CloseOrderAsync(openVer.OrderId);
                var args = await WaitEvent<OrderClosedEventArgs>(CloseEventTimeout);
                ThrowIfCloseFailed(closeResp);

                var closeVer = openVer.Fill(closeResp.TransactionTime).Close(closeResp.TransactionTime);
                closeVer.VerifyEvent(args);

                _tradeRepVerifiers.Add(closeVer);
            }
            else
            {
                _tradeRepVerifiers.Add(fillVer);
            }
        }

        private async Task TestTP(OrderType type, OrderSide side, bool asyncMode, string tag, OrderExecOptions options)
        {
            await Delay(PauseBetweenOpenOrders);

            var volume = BaseOrderVolume;
            var price = GetImmExecPrice(side, type);
            var stopPrice = GetImmExecStopPrice(side, type);
            var tp = GetTPPrice(side);

            Print($"Test: TakeProfit with options {type}, {side}, {(asyncMode ? "async" : "non-async")}, tag = {tag}, options: {options}");

            var openVer = await OpenAndCheck(type, side, volume, price, asyncMode, tag, stopPrice: stopPrice, tp: tp, options: options);

            DateTime fillTime = openVer.TradeReportTimestamp;

            if (type == OrderType.StopLimit)
            {
                var activationArgs = await WaitEvent<OrderActivatedEventArgs>(ActivateEventTimeout);
                _tradeRepVerifiers.Add(openVer.Activate(activationArgs.Order.Modified));

                var openArgs = await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);

                openVer = new OrderVerifier(openArgs.Order.Id, Account.Type, type, side, volume, price, null, openArgs.Order.Created);
            }

            if (!((type == OrderType.Limit || type == OrderType.StopLimit) && options == OrderExecOptions.ImmediateOrCancel))
                await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);

            await WaitEvent<OrderClosedEventArgs>(TPSLEventTimeout);
            _tradeRepVerifiers.Add(openVer.Close(openVer.TradeReportTimestamp));
        }

        private async Task TestSL(OrderType type, OrderSide side, bool asyncMode, string tag, OrderExecOptions options)
        {
            await Delay(PauseBetweenOpenOrders);

            var volume = BaseOrderVolume;
            var price = GetImmExecPrice(side, type);
            var stopPrice = GetImmExecStopPrice(side, type);
            var sl = GetSLPrice(side);

            Print($"Test: StopLoss with options {type}, {side}, {(asyncMode ? "async" : "non-async")}, tag = {tag}, options: {options}");

            var openVer = await OpenAndCheck(type, side, volume, price, asyncMode, tag, stopPrice: stopPrice, sl: sl, options: options);

            DateTime fillTime = openVer.TradeReportTimestamp;

            if (type == OrderType.StopLimit)
            {
                var activationArgs = await WaitEvent<OrderActivatedEventArgs>(ActivateEventTimeout);
                _tradeRepVerifiers.Add(openVer.Activate(activationArgs.Order.Modified));

                var openArgs = await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);

                openVer = new OrderVerifier(openArgs.Order.Id, Account.Type, type, side, volume, price, null, openArgs.Order.Created);
            }

            if (!((type == OrderType.Limit || type == OrderType.StopLimit) && options == OrderExecOptions.ImmediateOrCancel))
                await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);

            await WaitEvent<OrderClosedEventArgs>(TPSLEventTimeout);
            _tradeRepVerifiers.Add(openVer.Close(openVer.TradeReportTimestamp));
        }

        private async Task TestCancel(OrderType type, OrderSide side, bool asyncMode, string tag, OrderExecOptions options)
        {
            await Delay(PauseBetweenOrders);

            var volume = BaseOrderVolume;
            var price = GetDoNotExecPrice(side, type);
            var stopPrice = GetDoNotExecStopPrice(side, type);

            Print($"Test: Cancel with options {type}, {side}, {(asyncMode ? "async" : "non-async")}, tag = {tag}, options: {options}");

            var openVer = await OpenAndCheck(type, side, volume, price, asyncMode, tag, stopPrice: stopPrice, options: options);

            var cancelResp = await CancelOrderAsync(openVer.OrderId);
            ThrowIfCancelFailed(cancelResp);
            var cancelVer = openVer.Cancel(cancelResp.TransactionTime);

            var args = await WaitEvent<OrderCanceledEventArgs>(CancelEventTimeout);
            cancelVer.VerifyEvent(args);

            _tradeRepVerifiers.Add(cancelVer);
        }

        private async Task TestExpire(OrderType type, OrderSide side, bool asyncMode, string tag, OrderExecOptions options)
        {
            await Delay(PauseBetweenOrders);

            var volume = BaseOrderVolume;
            var price = GetDoNotExecPrice(side, type);
            var liftime = TimeSpan.FromSeconds(5);
            var stopPrice = GetDoNotExecStopPrice(side, type);

            Print($"Test: Expire with options {type}, {side}, {(asyncMode ? "async" : "non-async")}, tag = {tag}, options: {options}");

            var openVer = await OpenAndCheck(type, side, volume, price, asyncMode, tag, Now + liftime, stopPrice: stopPrice, options: options);

            var expirationEventArgs = await WaitEvent<OrderExpiredEventArgs>(ExpirationEventTimeout + liftime);
            var expVer = openVer.Expire(expirationEventArgs.Order.Modified);

            _tradeRepVerifiers.Add(expVer);
        }

        private async Task<OrderVerifier> OpenAndCheck(OrderType type, OrderSide side, double volume, double? price, bool asyncMode, string tag, DateTime? expiration = null, double? maxVisible = null, double? stopPrice = null, double? sl = null, double? tp = null, OrderExecOptions options = OrderExecOptions.None, string comment = null)
        {
            OrderCmdResult resp;
            if (asyncMode)
                resp = await OpenOrderAsync(Symbol.Name, type, side, volume, maxVisible, price, stopPrice, sl, tp, comment, options, tag, expiration);
            else
                resp = OpenOrder(Symbol.Name, type, side, volume, maxVisible, price, stopPrice, sl, tp, comment, options, tag, expiration);
            ThrowIfOpenFailed(resp);

            var verifier = new OrderVerifier(resp.ResultingOrder.Id, Account.Type, type, side, volume, price, stopPrice, resp.TransactionTime);

            var openedEventArgs = await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);

            return verifier;
        }

        private double? GetSLPrice(OrderSide side)
        {
            var delta = 2 * Symbol.Point * Math.Max(1, 10 - Symbol.Digits); //Math.Max it is necessary that orders are not executed on symbols with large price jumps
            return side == OrderSide.Buy ? Symbol.Ask + delta : Symbol.Bid - delta;
        }

        private double GetTPPrice(OrderSide side)
        {
            var delta = 2 * Symbol.Point * Math.Max(1, 10 - Symbol.Digits); //Math.Max it is necessary that orders are not executed on symbols with large price jumps
            return side == OrderSide.Buy ? Symbol.Ask - delta : Symbol.Bid + delta;
        }

        private double? GetImmExecPrice(OrderSide side, OrderType type)
        {
            var delta = Symbol.Point * Math.Max(1, 10 - Symbol.Digits); //Math.Max it is necessary that orders are not executed on symbols with large price jumps

            if (type == OrderType.Market)
                return Symbol.Bid - delta;

            if (type == OrderType.Stop)
                return null;
            
            return side == OrderSide.Buy ? Symbol.Ask + delta : Symbol.Bid - delta;
        }

        private double? GetImmExecStopPrice(OrderSide side, OrderType type)
        {
            var delta = Symbol.Point * Math.Max(1, 10 - Symbol.Digits); //Math.Max it is necessary that orders are not executed on symbols with large price jumps

            if (type != OrderType.Stop && type != OrderType.StopLimit)
                return null;

            return side == OrderSide.Buy ? Symbol.Ask - delta : Symbol.Bid + delta;
        }

        private double? GetDoNotExecPrice(OrderSide side, OrderType type)
        {
            var delta = PriceDelta * Symbol.Point * Math.Max(1, 10 - Symbol.Digits); //Math.Max it is necessary that orders are not executed on symbols with large price jumps

            if (type == OrderType.Limit || type == OrderType.StopLimit)
            {
                if (side == OrderSide.Buy)
                    return Symbol.Ask - delta;
                else if (side == OrderSide.Sell)
                    return Symbol.Bid + delta;
            }

            return null;
        }

        private double? GetDoNotExecStopPrice(OrderSide side, OrderType type)
        {
            var delta = PriceDelta * Symbol.Point * Math.Max(1, 10 - Symbol.Digits); //Math.Max it is necessary that orders are not executed on symbols with large price jumps

            if (type == OrderType.Stop || type == OrderType.StopLimit)
            {
                if (side == OrderSide.Buy)
                    return Symbol.Bid + delta;
                else if (side == OrderSide.Sell)
                    return Symbol.Ask - delta;
            }

            return null;
        }

        private void ThrowIfOpenFailed(OrderCmdResult resp)
        {
            if (resp.IsFaulted)
                throw new Exception("Failed to open order - " + resp.ResultCode);
        }

        private void ThrowIfCloseFailed(OrderCmdResult resp)
        {
            if (resp.IsFaulted)
                throw new Exception("Failed to close order - " + resp.ResultCode);
        }

        private void ThrowIfCancelFailed(OrderCmdResult resp)
        {
            if (resp.IsFaulted)
                throw new Exception("Failed to cancel order - " + resp.ResultCode);
        }

        private bool IsImmidiateFill(OrderType type, OrderExecOptions options)
        {
            return type == OrderType.Market || (type == OrderType.Limit && options.HasFlag(OrderExecOptions.ImmediateOrCancel));
        }

        #endregion

        #region History test

        private async Task DoQueryTests(bool async)
        {
            await DoQueryTests(async, ThQueryOptions.None);
            await DoQueryTests(async, ThQueryOptions.Backwards);
            await DoQueryTests(async, ThQueryOptions.SkipCanceled);
            await DoQueryTests(async, ThQueryOptions.SkipCanceled | ThQueryOptions.Backwards);
        }

        private async Task DoQueryTests(bool async, ThQueryOptions options)
        {
            await QueryRangeAll_Exact(async, options);
        }

        private void ReportsIteratorTest()
        {
            Print("Query trade history: simple iterator");

            var expected = Enumerable.Reverse(_tradeRepVerifiers).ToList();
            var actual = Account.TradeHistory.Take(expected.Count).ToList();

            CheckReports(expected, actual);
        }

        private async Task QueryRangeAll_Exact(bool async, ThQueryOptions options)
        {
            Print("Query trade history: async=" + async + " options=" + options);

            var from = _tradeRepVerifiers.First().TradeReportTimestamp;
            var to = _tradeRepVerifiers.Last().TradeReportTimestamp;

            await QuerySegmentTest(from, to, async, options);
        }

        private async Task QuerySegmentTest(DateTime from, DateTime to, bool async, ThQueryOptions options)
        {
            try
            {
                var reversed = options.HasFlag(ThQueryOptions.Backwards);
                var noCancels = options.HasFlag(ThQueryOptions.SkipCanceled);

                var expected = TakeVerifiers(from, to, reversed, noCancels);
                var actual = await QuerySegmentToList(from, to, async, options);

                CheckReports(expected, actual);
            }
            catch (Exception ex)
            {
                PrintError("Test failed: " + ex.Message);
            }
        }

        private bool ShouldSkip(bool skipCancels, OrderVerifier v)
        {
            return skipCancels && (v.TradeReportAction == TradeExecActions.OrderCanceled
                || v.TradeReportAction == TradeExecActions.OrderExpired
                || v.TradeReportAction == TradeExecActions.OrderActivated);
        }

        private List<OrderVerifier> TakeVerifiers(DateTime from, DateTime to, bool reversed, bool noCancels)
        {
            var result = new List<OrderVerifier>();

            foreach (var v in _tradeRepVerifiers)
            {
                if (ShouldSkip(noCancels, v))
                    continue;

                if ((v.TradeReportTimestamp >= from && v.TradeReportTimestamp <= to))
                    result.Add(v);
            }

            if (reversed)
                result.Reverse();

            return result;
        }

        private void CheckReports(List<OrderVerifier> verifiers, List<TradeReport> reports)
        {
            if (reports.Count != verifiers.Count)
                throw new Exception($"Report count does not match expected number! {reports.Count} vs {verifiers.Count}");

            for (int i = 0; i < reports.Count; i++)
            {
                verifiers[i].VerifyTradeReport(reports[i]);
            }
        }

        private async Task<List<TradeReport>> QuerySegmentToList(DateTime from, DateTime to, bool async, ThQueryOptions options)
        {
            var nFrom = FloorToSec(from);
            var nTo = CeilToSec(to);

            if (async)
            {
                var result = new List<TradeReport>();

                using (var e = Account.TradeHistory.GetRangeAsync(nFrom, nTo, options))
                {
                    while (await e.Next())
                        result.Add(e.Current);
                }

                return result;
            }
            else
                return Account.TradeHistory.GetRange(nFrom, nTo, options).ToList();
        }

        #endregion

        #region Event Verification

        private async Task<TArgs> WaitEvent<TArgs>(TimeSpan waitTimeout)
        {
            _eventWaiter = new TaskCompletionSource<object>();

            var delayTask = Task.Delay(waitTimeout);
            var eventTask = _eventWaiter.Task;

            var completedFirst = await Task.WhenAny(delayTask, eventTask);

            if (completedFirst == delayTask)
                throw new Exception("Timeout reached while wating for event " + typeof(TArgs).Name);

            var argsObj = await eventTask;

            if (argsObj is TArgs)
                return (TArgs)argsObj;

            throw new Exception("Unexpected event: Received " + argsObj.GetType().Name + " while expecting " + typeof(TArgs).Name);
        }

        private void OnEventFired<TArgs>(TArgs args)
        {
            if (_eventWaiter == null)
            {
                Print("Unexpected event: " + args.GetType().Name);
                return;
            }

            // note: function may start wating for new event inside SetResult(), so _eventWaiter = null should be before SetResult()
            var waiterCopy = _eventWaiter;
            _eventWaiter = null;
            waiterCopy.SetResult(args);
        }

        #endregion

        #region Misc methods

        private DateTime FloorToSec(DateTime src)
        {
            return src.Floor(TimeSpan.FromSeconds(1));
        }

        private DateTime CeilToSec(DateTime src)
        {
            return src.Ceil(TimeSpan.FromSeconds(1));
        }

        private void InitSidesAndTypes(ICollection<OrderSide> sides, ICollection<OrderType> types)
        {
            sides.Add(OrderSide.Buy);
            sides.Add(OrderSide.Sell);

            types.Add(OrderType.Market);
            if (Account.Type != AccountTypes.Cash)
                types.Add(OrderType.Stop);
            types.Add(OrderType.Limit);
            types.Add(OrderType.StopLimit);
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

        private static Order ThrowOnError(OrderCmdResult cmdResult)
        {
            if (cmdResult.ResultCode != OrderCmdResultCodes.Ok)
                throw new ApplicationException("Operation failed! Code=" + cmdResult.ResultCode);

            return cmdResult.ResultingOrder;
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

        private async Task TryPerformTest(Func<Task> func)
        {
            ++_testCount;
            try
            {
                await func();
            }
            catch (Exception ex)
            {
                ++_errorCount;
                PrintError(ex.Message);
            }
        }

        #endregion

        #region Add/Modify test

        private async Task PerfomAddModifyTests(OrderType orderType, OrderSide orderSide, bool isAsync, OrderExecOptions options = OrderExecOptions.None, string tag = null)
        {
            CurrentVolume = BaseOrderVolume;

            bool isInstantOrder = IsImmidiateFill(orderType, options);

            GetAddModifyPrices(orderType, orderSide, options, out var price, out var stopPrice, out var newPrice, out var newStopPrice);

            var title = isAsync ? "Async test: " : "Test: ";
            var postTitle = tag == null ? "" : " with tag";
            postTitle += options == OrderExecOptions.ImmediateOrCancel ? " with IoC" : "";
            Order accOrder = null;

            try
            {
                ++_testCount;
                Print(title + "open " + orderSide + " " + orderType + " order" + postTitle);
                var openVerifier = await OpenAndCheck(orderType, orderSide, CurrentVolume, price, isAsync, tag, stopPrice: stopPrice, options: options);
                if (IsImmidiateFill(orderType, options))
                {
                    var fillArgs = await WaitEvent<OrderFilledEventArgs>(FillEventTimeout);
                    OrderVerifier fillVerifier = openVerifier.Fill(fillArgs.OldOrder.Modified);
                    _tradeRepVerifiers.Add(fillVerifier);
                }
                accOrder = Account.Orders[openVerifier.OrderId];

                var realOrderType = orderType;
                if (Account.Type == AccountTypes.Cash && orderType == OrderType.Stop)
                    realOrderType = OrderType.StopLimit;
                else if (Account.Type == AccountTypes.Gross && isInstantOrder)
                    realOrderType = OrderType.Position;

                if (!isInstantOrder || Account.Type == AccountTypes.Gross)
                    VerifyOrder(accOrder.Id, realOrderType, orderSide, CurrentVolume, price, isInstantOrder, stopPrice, null, null, null, null, tag);

                if (!isInstantOrder)
                {
                    CurrentVolume *= 2;
                    openVerifier = new OrderVerifier(openVerifier.OrderId, Account.Type, orderType, orderSide, CurrentVolume, price, stopPrice, openVerifier.TradeReportTimestamp);
                    await TestModifyVolume(accOrder.Id, isAsync, CurrentVolume, postTitle);
                }

                if (!isInstantOrder || Account.Type == AccountTypes.Gross)
                {
                    await TestAddModifyComment(accOrder.Id, isAsync, postTitle);
                }

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
                    {
                        await TestModifyStopPrice(accOrder.Id, price, newStopPrice.Value, isAsync, postTitle);
                    }

                    if (accOrder != null)
                    {
                        await TestCancelOrder(accOrder.Id, orderSide, orderType, tag, false, isAsync, openVerifier);
                    }
                }
            }
            catch (Exception e)
            {
                _errorCount++;
                PrintError(e.Message);
            }
        }

        private void GetAddModifyPrices(OrderType orderType, OrderSide orderSide, OrderExecOptions options, out double? price, out double? stopPrice,
            out double? modifyPrice, out double? modifyStopPrice)
        {
            var diff = PriceDelta * Symbol.Point;
            bool isIoc = orderType == OrderType.Limit && options == OrderExecOptions.ImmediateOrCancel;
            bool isInstantOrder = orderType == OrderType.Market || isIoc;

            price = stopPrice = modifyPrice = modifyStopPrice = null;

            if (orderSide == OrderSide.Buy)
            {
                var basePrice = Symbol.Ask;

                if (isInstantOrder)
                    price = Symbol.Ask + diff;
                else
                {
                    if (orderType == OrderType.Limit)
                    {
                        price = Symbol.Ask - diff * 3;
                        modifyPrice = Symbol.Ask - diff;
                    }
                    if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                    {
                        stopPrice = Symbol.Ask + diff * 2;
                        modifyStopPrice = Symbol.Ask + diff;
                    }
                    if (orderType == OrderType.StopLimit)
                    {
                        price = Symbol.Ask + diff * 3;
                        modifyPrice = Symbol.Ask + diff * 4;
                    }
                }
            }
            else
            {
                if (isInstantOrder)
                    price = Symbol.Bid - diff;
                else
                {
                    if (orderType == OrderType.Limit)
                    {
                        price = Symbol.Bid + diff * 3;
                        modifyPrice = Symbol.Bid + diff;
                    }
                    if (orderType == OrderType.Stop || orderType == OrderType.StopLimit)
                    {
                        stopPrice = Symbol.Bid - diff * 2;
                        modifyStopPrice = Symbol.Bid - diff;
                    }
                    if (orderType == OrderType.StopLimit)
                    {
                        price = Symbol.Bid - diff * 3;
                        modifyPrice = Symbol.Bid - diff * 4;
                    }
                }
            }
        }

        private async Task TestAddModifyComment(string orderId, bool isAsync, string postTitle = "")
        {
            try
            {
                const string comment = "Comment";
                const string newComment = "New comment";
                var title = (isAsync) ? "Async test: " : "Test: ";
                var order = Account.Orders[orderId];

                ++_testCount;
                Print(title + "add comment " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, comment));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, comment));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, comment);

                ++_testCount;
                Print(title + "modify comment " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, newComment));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, newComment));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, newComment);
            }
            catch
            {

            }
        }

        private async Task TestAddModifyExpiration(string orderId, bool isAsync, string postTitle = "")
        {
            try
            {
                var year = DateTime.Today.Year;
                var month = DateTime.Today.Month;
                var day = DateTime.Today.Day;
                var expiration = new DateTime(year + 1, month, day);
                var newExpiration = new DateTime(year + 2, month, day);

                var title = (isAsync) ? "Async test: " : "Test: ";
                var order = Account.Orders[orderId];

                ++_testCount;
                Print(title + "add expiration " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, null, expiration));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, null, expiration));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, null, null, null, null, null, expiration);

                ++_testCount;
                Print(title + "modify expiration " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, null, newExpiration));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, null, newExpiration));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, null, null, null, null, null, newExpiration);
            }
            catch
            {

            }
        }

        private async Task TestAddModifyMaxVisibleVolume(string orderId, bool isAsync, string postTitle = "")
        {
            try
            {
                var order = Account.Orders[orderId];
                var maxVisibleVolume = BaseOrderVolume;
                var newMaxVisibleVolume = Symbol.MinTradeVolume;
                var title = (isAsync) ? "Async test: " : "Test: ";

                ++_testCount;
                Print(title + "add maxVisibleVolume " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, maxVisibleVolume, null, null, null));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, maxVisibleVolume, null, null, null));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, null, maxVisibleVolume);

                order = Account.Orders[orderId];

                ++_testCount;
                Print(title + "modify maxVisibleVolume " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, newMaxVisibleVolume, null, null, null));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, newMaxVisibleVolume, null, null, null));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, null, newMaxVisibleVolume);
            }
            catch
            {

            }
        }

        private async Task TestAddModifyTakeProfit(string orderId, bool isAsync, string postTitle = "")
        {
            try
            {
                double diff = PriceDelta * Symbol.Point;
                var order = Account.Orders[orderId];
                var takeProfit = (order.Side == OrderSide.Buy) ? (Symbol.Ask + diff * 4) : (Symbol.Bid - diff * 4);
                var newTakeProfit = (order.Side == OrderSide.Buy) ? (Symbol.Ask + diff * 5) : (Symbol.Bid - diff * 5);
                var title = (isAsync) ? "Async test: " : "Test: ";

                ++_testCount;
                Print(title + "add takeProfit " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, takeProfit, null));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, null, takeProfit, null));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, null, null, takeProfit);

                ++_testCount;
                Print(title + "modify takeProfit " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, newTakeProfit, null));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, null, newTakeProfit, null));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, null, null, newTakeProfit);
            }
            catch
            {

            }
        }

        private async Task TestAddModifyStopLoss(string orderId, bool isAsync, string postTitle = "")
        {
            try
            {
                double diff = PriceDelta * Symbol.Point;
                var order = Account.Orders[orderId];
                var stopLoss = (order.Side == OrderSide.Buy) ? (Symbol.Bid - diff * 4) : (Symbol.Ask + diff * 4);
                var newStopLoss = (order.Side == OrderSide.Buy) ? (Symbol.Bid - diff * 5) : (Symbol.Ask + diff * 5);
                var title = (isAsync) ? "Async test: " : "Test: ";

                ++_testCount;
                Print(title + "add stopLoss " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, stopLoss, null, null));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, stopLoss, null, null));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, null, null, null, stopLoss);

                ++_testCount;
                Print(title + "modify stopLoss " + order.Side + " " + order.Type + " order " + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, newStopLoss, null, null));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, newStopLoss, null, null));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, null, false, null, null, null, null, newStopLoss);
            }
            catch
            {

            }
        }

        private async Task TestModifyVolume(string orderId, bool isAsync, double newVolume, string postTitle = "")
        {
            try
            {
                var title = (isAsync) ? "Async test: " : "Test: ";
                var order = Account.Orders[orderId];

                ++_testCount;
                Print(title + "modify volume " + order.Side + " " + order.Type + " order" + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, null, null, null, null, null, null, null, newVolume));
                else
                    ThrowOnError(ModifyOrder(order.Id, null, null, null, null, null, null, null, newVolume));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, newVolume, null, false, null);
            }
            catch
            {

            }
        }

        private async Task TestModifyLimitPrice(string orderId, double newPrice, double? stopPrice, bool isAsync, string postTitle = "")
        {
            try
            {
                var title = (isAsync) ? "Async test: " : "Test: ";
                var order = Account.Orders[orderId];

                ++_testCount;
                Print(title + "modify limit price " + order.Side + " " + order.Type + " order" + postTitle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, newPrice, stopPrice, null, null, null, null));
                else
                    ThrowOnError(ModifyOrder(order.Id, newPrice, stopPrice, null, null, null, null));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, newPrice, false, stopPrice);
            }
            catch
            {

            }
        }

        private async Task TestModifyStopPrice(string orderId, double? price, double newStopPrice, bool isAsync, string postTittle = "")
        {
            try
            {
                var title = (isAsync) ? "Async test: " : "Test: ";
                var order = Account.Orders[orderId];

                ++_testCount;
                Print(title + "modify stopPrice " + order.Side + " " + order.Type + " order" + postTittle);
                if (isAsync)
                    ThrowOnError(await ModifyOrderAsync(order.Id, price, newStopPrice, null, null, null, null));
                else
                    ThrowOnError(ModifyOrder(order.Id, price, newStopPrice, null, null, null, null));
                await WaitEvent<OrderModifiedEventArgs>(ModifyEventTimeout);
                VerifyOrder(order.Id, order.Type, order.Side, CurrentVolume, price, false, newStopPrice);
            }
            catch
            {

            }
        }

        private async Task TestCancelOrder(string orderId, OrderSide orderSide, OrderType orderType, string tag, bool isIoc, bool isAsync, OrderVerifier openVer)
        {
            if (Account.Orders[orderId] == null)
                return;

            ++_testCount;

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
                var cancelEventArgs = await WaitEvent<OrderCanceledEventArgs>(CancelEventTimeout);
                _tradeRepVerifiers.Add(openVer.Cancel(cancelEventArgs.Order.Modified));
                VerifyOrderDeleted(orderId);
            }
            else
            {
                if (isAsync)
                    ThrowOnError(await CancelOrderAsync(orderId));
                else
                    ThrowOnError(CancelOrder(orderId));
                var cancelEventArgs = await WaitEvent<OrderCanceledEventArgs>(CancelEventTimeout);
                _tradeRepVerifiers.Add(openVer.Cancel(cancelEventArgs.Order.Modified));
                VerifyOrderDeleted(orderId);
            }
        }

        private void VerifyOrderDeleted(string orderId)
        {
            var order = Account.Orders[orderId];
            if (!order.IsNull)
                throw new ApplicationException("Verification failed - order #" + orderId + " still exist in order collection!");
        }

        #endregion

        #region AD Comments test

        private async Task PerformCommentsTest(OrderType orderType, OrderSide orderSide, bool asyncMode, string tag, OrderExecOptions options)
        {
            await TryPerformTest(() => TestCommentReject(orderType, orderSide, asyncMode, tag, options));

            if (Account.Type == AccountTypes.Gross)
            {
                await TryPerformTest(() => TestCommentCustomActivate(orderType, orderSide, asyncMode, tag, options));
            }
        }

        private async Task TestCommentReject(OrderType type, OrderSide side, bool asyncMode, string tag, OrderExecOptions options)
        {
            await Delay(PauseBetweenOpenOrders);

            var volume = BaseOrderVolume;
            var price = GetImmExecPrice(side, type);
            var stopPrice = GetImmExecStopPrice(side, type);

            CommentModel commentModel = new CommentModel();
            commentModel.Add(new RejectCommentModel());
            var comment = $"json:{commentModel.Serialize()}";

            Print($"Reject comment test with options {type}, {side}, {(asyncMode ? "async" : "non-async")}, tag = {tag}, options: {options}");

            try
            {
                await OpenAndCheck(type, side, volume, price, asyncMode, tag, stopPrice: stopPrice, comment: comment, options: options);
            }
            catch
            {
                return;
            }

            throw new Exception("Order not rejected!");
        }

        private async Task TestCommentCustomActivate(OrderType type, OrderSide side, bool asyncMode, string tag, OrderExecOptions options)
        {
            await Delay(PauseBetweenOpenOrders);

            var volume = BaseOrderVolume;
            var price = GetDoNotExecPrice(side, OrderType.Limit);
            var stopPrice = GetDoNotExecStopPrice(side, type);
            var customPrice = price / 2;
            var customVolume = 0.2 * BaseOrderVolume;

            CommentModel commentModel = new CommentModel();
            commentModel.Add(new ConfirmCommentModel());
            commentModel.Add(new ActivateCommentModel(customPrice, customVolume * Symbol.ContractSize));
            var comment = $"json:{commentModel.Serialize()}";

            Print($"Custom activate comment test with options {type}, {side}, {(asyncMode ? "async" : "non-async")}, tag = {tag}, options: {options}");

            var openVer = await OpenAndCheck(type, side, volume, price, asyncMode, tag, stopPrice: stopPrice, comment: comment, options: options);
            OrderVerifier newOpenVer = null;

            if (options != OrderExecOptions.ImmediateOrCancel)
            {
                var openArgs = await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);
                newOpenVer = new OrderVerifier(openArgs.Order.Id, Account.Type, OrderType.Position, side, volume - customVolume, customPrice, null, openArgs.Order.Created);

                await WaitEvent<OrderFilledEventArgs>(FillEventTimeout);
            }

            if (!IsImmidiateFill(type, options))
            {
                var cancelResp = await CancelOrderAsync(openVer.OrderId);
                await WaitEvent<OrderCanceledEventArgs>(CancelEventTimeout);
                ThrowIfCloseFailed(cancelResp);

                openVer.Clone(openVer.CurrentType, volume - customVolume, openVer.TradeReportAction, openVer.TradeReportTimestamp);
                var cancelVer = openVer.Cancel(cancelResp.TransactionTime);

                _tradeRepVerifiers.Add(cancelVer);
            }

            var closeResp = await CloseOrderAsync(newOpenVer == null ? openVer.OrderId : newOpenVer.OrderId);
            await WaitEvent<OrderClosedEventArgs>(CloseEventTimeout);
            ThrowIfCloseFailed(closeResp);

            var closeVer = openVer.Close(closeResp.TransactionTime);

            _tradeRepVerifiers.Add(closeVer);
        }

        #endregion
    }
}
