using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    [TradeBot(DisplayName = "Composite Tarde API Test", Version = "1.1", Category = "Auto Tests", SetupMainSymbol = true,
        Description = "")]
    public class CompositeTradeApiTest : TradeBot
    {
        private const int pipsDelta = 1000;

        private List<OrderVerifier> _tradeRepVerifiers = new List<OrderVerifier>();

        private readonly TimeSpan PauseVal = TimeSpan.FromSeconds(1);

        [Parameter]
        public bool UseDealerCmdApi { get; set; }

        [Parameter(DefaultValue = 0.1)]
        public double BaseOrderVolume { get; set; }

        protected async override void OnStart()
        {
            try
            {
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

            await Delay(PauseVal);
        }

        #region Orders

        private async Task OpenFill(OrderType type, OrderSide side, double volumeFactor)
        {
            var volume = BaseOrderVolume * volumeFactor;
            var price = GetImmExecPrice(side, type);

            var resp = await OpenOrderAsync(Symbol.Name, type, side, volume, null, price, null, null, null, null, OrderExecOptions.None);

            ThrowIfOpenFailed(resp);

            var verifier = new OrderVerifier(resp.ResultingOrder.Id, Account.Type, type, side, volume, price, null);

            if (!IsImmidiateFill(type, OrderExecOptions.None))
                await Delay(PauseVal);

            var fillVerifier = verifier.Fill();

            if (Account.Type == AccountTypes.Gross)
            {
                var closeResp = await CloseOrderAsync(resp.ResultingOrder.Id);

                ThrowIfCloseFailed(closeResp);

                var closeVerifier = fillVerifier.Close(DateTime.MinValue);
                _tradeRepVerifiers.Add(closeVerifier);
            }
            else
                _tradeRepVerifiers.Add(fillVerifier);
        }

        private async Task OpenCancel(OrderType type, OrderSide side, double volumeFactor)
        {
            var volume = BaseOrderVolume * volumeFactor;
            var price = GetDoNotExecPrice(side, type);

            var resp = await OpenOrderAsync(Symbol.Name, type, side, volume, null, price, null, null, null);

            ThrowIfOpenFailed(resp);

            var verifier = new OrderVerifier(resp.ResultingOrder.Id, Account.Type, type, side, volume, price, null);

            var cancelResp = await CancelOrderAsync(resp.ResultingOrder.Id);

            ThrowIfCancelFailed(cancelResp);

            var cancelVerifier = verifier.Cancel();

            _tradeRepVerifiers.Add(cancelVerifier);
        }

        private async Task OpenExpire(OrderType type, OrderSide side, double volumeFactor)
        {
            var volume = BaseOrderVolume * volumeFactor;
            var price = GetDoNotExecPrice(side, OrderType.Limit);
            var expiration = Now;

            var resp = await OpenOrderAsync(Symbol.Name, type, side, volume, null, price,
                null, null, null, null, OrderExecOptions.None, null, expiration);

            ThrowIfOpenFailed(resp);
        }

        private double GetImmExecPrice(OrderSide side, OrderType type)
        {
            var delta = pipsDelta * Symbol.Point;

            if (type == OrderType.Market)
                return Symbol.Bid;

            if (type == OrderType.Limit)
            {
                if (side == OrderSide.Buy)
                    return Symbol.Bid + delta;
                else if (side == OrderSide.Sell)
                    return Symbol.Ask - delta;
            }

            throw new NotImplementedException();
        }

        private double GetDoNotExecPrice(OrderSide side, OrderType type)
        {
            var delta = pipsDelta * Symbol.Point;

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
            await QueryRangeAll_Near(async, options);
        }

        private void ReportsIteratorTest()
        {
            Print("Test simple iterator");

            var rangeStart = _tradeRepVerifiers.First().TradeReportTimestamp;
            var expected = Enumerable.Reverse(_tradeRepVerifiers).ToList();
            var actual = Account.TradeHistory.TakeWhile(r => r.ReportTime >= rangeStart).ToList();

            CheckReports(expected, actual);
        }

        private async Task QueryRangeAll_Exact(bool async, ThQueryOptions options)
        {
            var from = _tradeRepVerifiers.First().TradeReportTimestamp;
            var to = _tradeRepVerifiers.Last().TradeReportTimestamp;

            await QuerySegmentTest(from, to, async, options);
        }

        private async Task QueryRangeAll_Near(bool async, ThQueryOptions options)
        {
            var from = _tradeRepVerifiers.First().TradeReportTimestamp - TimeSpan.FromSeconds(1);
            var to = _tradeRepVerifiers.Last().TradeReportTimestamp + TimeSpan.FromSeconds(1);

            await QuerySegmentTest(from, to, async, options);
        }

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
            Print("Test {0} segment query from {1} to {2}, {3}", async ? "async" : "", from, to, options);

            var reversed = options.HasFlag(ThQueryOptions.Backwards);
            var noCancels = options.HasFlag(ThQueryOptions.SkipCanceled);

            var expected = TakeVerifiers(from, to, reversed, noCancels);
            var actual = await QuerySegmentToList(from, to, async, options);

            CheckReports(expected, actual);
        }

        private async Task QueryVectorTest(DateTime from, bool async, ThQueryOptions options)
        {
            Print("Test {0} vector query from {1}, {2}", async ? "async" : "", from, options);

            var reversed = options.HasFlag(ThQueryOptions.Backwards);
            var noCancels = options.HasFlag(ThQueryOptions.SkipCanceled);

            var expected = TakeVerifiers(from, reversed, noCancels);
            var actual = await QueryVectorToList(from, async, options);

            CheckReports(expected, actual);
        }

        private List<OrderVerifier> TakeVerifiers(DateTime from, DateTime to, bool reversed, bool noCancels)
        {
            var result = new List<OrderVerifier>();

            foreach (var v in _tradeRepVerifiers)
            {
                var skipCancel = noCancels && v.TradeReportAction == TradeExecActions.OrderCanceled;

                if ((v.TradeReportTimestamp >= from && v.TradeReportTimestamp <= to) && !skipCancel)
                    result.Add(v);
            }

            if (reversed)
                result.Reverse();

            return result;
        }

        private List<OrderVerifier> TakeVerifiers(DateTime from, bool reversed, bool noCancels)
        {
            var result = new List<OrderVerifier>();

            foreach (var v in _tradeRepVerifiers)
            {
                var skipCancel = noCancels && v.TradeReportAction == TradeExecActions.OrderCanceled;

                if (reversed && v.TradeReportTimestamp <= from || v.TradeReportTimestamp >= from)
                    result.Add(v);
            }

            if (reversed)
                result.Reverse();

            return result;
        }

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
            if (async)
            {
                var result = new List<TradeReport>();

                using (var e = Account.TradeHistory.GetRangeAsync(from, to, options))
                {
                    while (await e.Next())
                        result.Add(e.Current);
                }

                return result;
            }
            else
                return Account.TradeHistory.GetRange(from, to, options).ToList();
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
    }
}
