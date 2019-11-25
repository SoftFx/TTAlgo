using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    [TradeBot(DisplayName = "Composite Tarde API Test", Version = "1.2", Category = "Auto Tests", SetupMainSymbol = true,
        Description = "")]
    public class CompositeTradeApiTest : TradeBot
    {
        private List<OrderVerifier> _tradeRepVerifiers = new List<OrderVerifier>();
        private TaskCompletionSource<object> _eventWaiter;

        private readonly TimeSpan OpenEventTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan FillEventTimeout = TimeSpan.FromSeconds(10);
        private readonly TimeSpan CancelEventTimeout = TimeSpan.FromSeconds(5);
        private readonly TimeSpan ExpirationEventTimeout = TimeSpan.FromSeconds(25);
        private readonly TimeSpan PauseBetweenOpenOrders = TimeSpan.FromSeconds(1);
        private readonly TimeSpan PauseBetweenOrders = TimeSpan.FromSeconds(1);

        [Parameter]
        public bool UseDealerCmdApi { get; set; }

        [Parameter(DefaultValue = 0.1)]
        public double BaseOrderVolume { get; set; }

        [Parameter(DefaultValue = 1000)]
        public int PriceDelta { get; set; }

        protected async override void OnStart()
        {
            try
            {
                Account.Orders.Opened += a => OnEventFired(a);
                Account.Orders.Filled += a => OnEventFired(a);
                Account.Orders.Closed += a => OnEventFired(a);
                Account.Orders.Modified += a => OnEventFired(a);
                Account.Orders.Expired += a => OnEventFired(a);
                Account.Orders.Canceled += a => OnEventFired(a);

                await OpenOrders();

                ReportsIteratorTest();

                await DoQueryTests(false);
                await DoQueryTests(true);
            }
            catch (Exception ex)
            {
                PrintError(ex.Message);
            }

            Exit();
        }

        private async Task OpenOrders()
        {
            await OpenFill(OrderType.Market, OrderSide.Buy, 2);
            await OpenFill(OrderType.Market, OrderSide.Sell, 1);
            await OpenFill(OrderType.Market, OrderSide.Sell, 1);

            await OpenCancel(OrderType.Limit, OrderSide.Buy, 1);
            await OpenCancel(OrderType.Limit, OrderSide.Sell, 1);

            await OpenExpire(OrderType.Limit, OrderSide.Buy, 1);
            await OpenExpire(OrderType.Limit, OrderSide.Sell, 1);

            await OpenFill(OrderType.Limit, OrderSide.Buy, 2);
            await OpenFill(OrderType.Limit, OrderSide.Sell, 1);
            await OpenFill(OrderType.Limit, OrderSide.Sell, 1);

            // wait some time to allow trade reports reach DB
            await Delay(TimeSpan.FromSeconds(5));
        }

        #region Orders

        private async Task OpenFill(OrderType type, OrderSide side, double volumeFactor)
        {
            await Delay(PauseBetweenOpenOrders);

            var volume = BaseOrderVolume * volumeFactor;
            var price = GetImmExecPrice(side, type);

            // open order
            var openVer = await OpenAndCheck(type, side, volume, price, null);

            // check fill event
            DateTime fillTime = openVer.TradeReportTimestamp;
            var fillVer = openVer.Fill(fillTime);
            if (!IsImmidiateFill(type, OrderExecOptions.None))
            {
                var fillEventArgs = await WaitEvent<OrderFilledEventArgs>(FillEventTimeout);
                fillVer.VerifyEvent(fillEventArgs);
                fillTime = fillEventArgs.NewOrder.Modified;
            }

            if (Account.Type == AccountTypes.Gross)
            {
                // close order
                var closeResp = await CloseOrderAsync(openVer.OrderId);
                ThrowIfCloseFailed(closeResp);

                // check closed order
                var closeVer = fillVer.Close(closeResp.TransactionTime);
                closeVer.VerifyOrder(closeResp.ResultingOrder);

                // add trade report verification
                _tradeRepVerifiers.Add(closeVer);
            }
            else
            {
                // add trade report verification
                _tradeRepVerifiers.Add(fillVer);
            }
        }

        private async Task OpenCancel(OrderType type, OrderSide side, double volumeFactor)
        {
            await Delay(PauseBetweenOrders);

            var volume = BaseOrderVolume * volumeFactor;
            var price = GetDoNotExecPrice(side, type);

            // open order
            var openVer = await OpenAndCheck(type, side, volume, price, null);

            // cancel order
            var cancelResp = await CancelOrderAsync(openVer.OrderId);
            ThrowIfCancelFailed(cancelResp);
            var cancelVer = openVer.Cancel(cancelResp.TransactionTime);
            cancelVer.VerifyOrder(cancelResp.ResultingOrder);

            // check cancel event
            var cancelEventArgs = await WaitEvent<OrderCanceledEventArgs>(CancelEventTimeout);
            cancelVer.VerifyEvent(cancelEventArgs);

            // add trade report verification
            _tradeRepVerifiers.Add(cancelVer);
        }

        private async Task OpenExpire(OrderType type, OrderSide side, double volumeFactor)
        {
            await Delay(PauseBetweenOrders);

            var volume = BaseOrderVolume * volumeFactor;
            var price = GetDoNotExecPrice(side, OrderType.Limit);
            var liftime = TimeSpan.FromSeconds(5);

            // open order
            var openVer = await OpenAndCheck(type, side, volume, price, Now + liftime);

            // check expiration event
            var expirationEventArgs = await WaitEvent<OrderExpiredEventArgs>(ExpirationEventTimeout + liftime);
            var expVer = openVer.Expire(expirationEventArgs.Order.Modified);
            openVer.VerifyEvent(expirationEventArgs);

            // add trade report verification
            _tradeRepVerifiers.Add(expVer);
        }

        private async Task<OrderVerifier> OpenAndCheck(OrderType type, OrderSide side, double volume, double price, DateTime? expiration)
        {
            // open order
            var resp = await OpenOrderAsync(Symbol.Name, type, side, volume, null, price, null, null, null, null, OrderExecOptions.None, null, expiration);
            ThrowIfOpenFailed(resp);

            // check opened order
            var verifier = new OrderVerifier(resp.ResultingOrder.Id, Account.Type, type, side, volume, price, null, resp.TransactionTime);
            verifier.VerifyOrder(resp.ResultingOrder);

            // check open event
            var openedEventArgs = await WaitEvent<OrderOpenedEventArgs>(OpenEventTimeout);
            verifier.VerifyEvent(openedEventArgs);

            return verifier;
        }

        private double GetImmExecPrice(OrderSide side, OrderType type)
        {
            var delta = Symbol.Point * Math.Max(1, 10 - Symbol.Digits); //Math.Max it is necessary that orders are not executed on symbols with large price jumps

            if (type == OrderType.Market)
                return Symbol.Bid - delta;

            return side == OrderSide.Buy ? Symbol.Ask + delta : Symbol.Bid - delta;
        }

        private double GetDoNotExecPrice(OrderSide side, OrderType type)
        {
            var delta = PriceDelta * Symbol.Point * Math.Max(1, 10 - Symbol.Digits); //Math.Max it is necessary that orders are not executed on symbols with large price jumps

            if (type == OrderType.Limit)
            {
                if (side == OrderSide.Buy)
                    return Symbol.Ask - delta;
                else if (side == OrderSide.Sell)
                    return Symbol.Bid + delta;
            }

            throw new NotImplementedException();
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
            //await QueryRangeAll_Near(async, options);
        }

        private void ReportsIteratorTest()
        {
            Print("Query trade history: simple iterator");

            //var rangeStart = _tradeRepVerifiers.First().TradeReportTimestamp;
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

        //private async Task QueryRangeAll_Near(bool async, ThQueryOptions options)
        //{
        //    Print("Query trade history: range=near async=" + async + " options=" + options);

        //    var from = _tradeRepVerifiers.First().TradeReportTimestamp - TimeSpan.FromSeconds(1);
        //    var to = _tradeRepVerifiers.Last().TradeReportTimestamp + TimeSpan.FromSeconds(1);

        //    await QuerySegmentTest(from, to, async, options);
        //}

        private async Task QueryMiddleSegment(bool async)
        {
        }

        private async Task QuerySegmentBehind(bool async)
        {
        }

        private async Task QueryEmptySegmentBefore(bool async)
        {
        }

        private async Task QuerySegmentTest(DateTime from, DateTime to, bool async, ThQueryOptions options)
        {
            //Print("Test {0} segment query from {1} to {2}, {3}", async ? "async" : "", from, to, options);

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

        //private async Task QueryVectorTest(DateTime from, bool async, ThQueryOptions options)
        //{
        //    Print("Test {0} vector query from {1}, {2}", async ? "async" : "", from, options);

        //    var reversed = options.HasFlag(ThQueryOptions.Backwards);
        //    var noCancels = options.HasFlag(ThQueryOptions.SkipCanceled);

        //    var expected = TakeVerifiers(from, reversed, noCancels);
        //    var actual = await QueryVectorToList(from, async, options);

        //    CheckReports(expected, actual);
        //}

        private bool ShouldSkip(bool skipCancels, OrderVerifier v)
        {
            return skipCancels && (v.TradeReportAction == TradeExecActions.OrderCanceled
                || v.TradeReportAction == TradeExecActions.OrderExpired);
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

        //private List<OrderVerifier> TakeVerifiers(DateTime from, bool reversed, bool noCancels)
        //{
        //    var result = new List<OrderVerifier>();

        //    foreach (var v in _tradeRepVerifiers)
        //    {
        //        var skipCancel = noCancels && (v.TradeReportAction == TradeExecActions.OrderCanceled
        //            || v.TradeReportAction == TradeExecActions.OrderExpired);

        //        if (reversed && v.TradeReportTimestamp <= from || v.TradeReportTimestamp >= from)
        //            result.Add(v);
        //    }

        //    if (reversed)
        //        result.Reverse();

        //    return result;
        //}

        private void CheckReports(List<OrderVerifier> verifiers, List<TradeReport> reports)
        {
            if (reports.Count != verifiers.Count)
                throw new Exception("Report count does not match expected number!");

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

        private async Task<List<TradeReport>> QueryVectorToList(DateTime from, bool async, ThQueryOptions options)
        {
            if (async)
            {
                var result = new List<TradeReport>();

                using (var e = Account.TradeHistory.GetRangeAsync(from, options))
                {
                    while (await e.Next())
                        result.Add(e.Current);
                }

                return result;
            }
            else
                return Account.TradeHistory.GetRange(from, options).ToList();
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
            //Print("Event: " + args.GetType().Name + (_eventWaiter == null ? " (n)" : ""));

            if (_eventWaiter == null)
            {
                Print("Unexpected event: " + args.GetType().Name);
                return; //throw new Exception("Unexpected event: " + args.GetType().Name);
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

        #endregion
    }
}
