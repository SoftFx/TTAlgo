using ActorSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using TickTrader.FDK.Common;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Common.Model
{
    public static class SfxTaskAdapter
    {
        #region Quote

        public static void InitTaskAdapter(this FDK.Client.QuoteFeed client)
        {
            client.ConnectResultEvent += (c, d) => SetCompleted(d);
            client.ConnectErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LoginResultEvent += (c, d) => SetCompleted(d);
            client.LoginErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LogoutResultEvent += (c, d, i) => SetCompleted(d, i);
            client.LogoutErrorEvent += (c, d, ex) => SetFailed<LogoutInfo>(d, ex);

            client.DisconnectResultEvent += (c, d, t) => SetCompleted(d, t);

            client.QuotesResultEvent += (c, d, r) => SetCompleted(d, SfxInterop.Convert(r));
            client.QuotesErrorEvent += (c, d, ex) => SetFailed<QuoteEntity[]>(d, ex);

            client.CurrencyListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.CurrencyListErrorEvent += (c, d, ex) => SetFailed<FDK2.CurrencyInfo[]>(d, ex);

            client.SymbolListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.SymbolListErrorEvent += (c, d, ex) => SetFailed<FDK2.SymbolInfo[]>(d, ex);

            client.SubscribeQuotesResultEvent += (c, d, r) => SetCompleted(d, SfxInterop.Convert(r));
            client.SubscribeQuotesErrorEvent += (c, d, ex) => SetFailed<QuoteEntity[]>(d, ex);

            client.QuotesResultEvent += (c, d, r) => SetCompleted(d, SfxInterop.Convert(r));
            client.QuotesErrorEvent += (c, d, ex) => SetFailed<QuoteEntity[]>(d, ex);
        }

        public static Task ConnectAsync(this FDK.Client.QuoteFeed client, string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public static Task LoginAsync(this FDK.Client.QuoteFeed client, string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public static Task LogoutAsync(this FDK.Client.QuoteFeed client, string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            client.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public static Task DisconnectAsync(this FDK.Client.QuoteFeed client, string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            client.DisconnectAsync(taskSrc, text);
            return taskSrc.Task;
        }

        public static Task<FDK2.CurrencyInfo[]> GetCurrencyListAsync(this FDK.Client.QuoteFeed client)
        {
            var taskSrc = new TaskCompletionSource<FDK2.CurrencyInfo[]>();
            client.GetCurrencyListAsync(taskSrc);
            return taskSrc.Task;
        }

        public static Task<FDK2.SymbolInfo[]> GetSymbolListAsync(this FDK.Client.QuoteFeed client)
        {
            var taskSrc = new TaskCompletionSource<FDK2.SymbolInfo[]>();
            client.GetSymbolListAsync(taskSrc);
            return taskSrc.Task;
        }

        public static Task<QuoteEntity[]> SubscribeQuotesAsync(this FDK.Client.QuoteFeed client, string[] symbolIds, int marketDepth)
        {
            var taskSrc = new TaskCompletionSource<QuoteEntity[]>();
            client.SubscribeQuotesAsync(taskSrc, GetSymbolEntries(symbolIds, marketDepth));
            return taskSrc.Task;
        }

        public static Task<Quote[]> GetQuotesAsync(this FDK.Client.QuoteFeed client, string[] symbolIds, int marketDepth)
        {
            var taskSrc = new TaskCompletionSource<Quote[]>();
            client.GetQuotesAsync(taskSrc, GetSymbolEntries(symbolIds, marketDepth));
            return taskSrc.Task;
        }

        #endregion

        #region Quote History

        public static void InitTaskAdapter(this FDK.Client.QuoteStore client)
        {
            client.ConnectResultEvent += (c, d) => SetCompleted(d);
            client.ConnectErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LoginResultEvent += (c, d) => SetCompleted(d);
            client.LoginErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LogoutResultEvent += (c, d, i) => SetCompleted(d, i);
            client.LogoutErrorEvent += (c, d, ex) => SetFailed<LogoutInfo>(d, ex);

            client.DisconnectResultEvent += (c, d, t) => SetCompleted(d, t);

            client.BarListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.BarListErrorEvent += (c, d, ex) => SetFailed<Bar[]>(d, ex);

            client.QuoteListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.QuoteListErrorEvent += (c, d, ex) => SetFailed<Bar[]>(d, ex);

            client.HistoryInfoResultEvent += (c, d, r) => SetCompleted(d, r);
            client.HistoryInfoErrorEvent += (c, d, ex) => SetFailed<HistoryInfo>(d, ex);

            //client.BarDownloadResultEvent += (c, d, r) => ((BlockingChannel<BarEntity>)d)?.Write(SfxInterop.Convert(r));
            //client.BarDownloadResultEndEvent += (c, d) => ((BlockingChannel<BarEntity>)d)?.Close();
            //client.BarDownloadErrorEvent += (c, d, ex) => ((BlockingChannel<BarEntity>)d)?.Close(ex);
        }

        public static Task ConnectAsync(this FDK.Client.QuoteStore client, string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public static Task LoginAsync(this FDK.Client.QuoteStore client, string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public static Task LogoutAsync(this FDK.Client.QuoteStore client, string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            client.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public static Task DisconnectAsync(this FDK.Client.QuoteStore client, string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            client.DisconnectAsync(taskSrc, text);
            return taskSrc.Task;
        }

        public static Task<Bar[]> GetBarListAsync(this FDK.Client.QuoteStore client, string symbol, PriceType priceType, BarPeriod barPeriod, DateTime from, int count)
        {
            var taskSrc = new TaskCompletionSource<Bar[]>();
            client.GetBarListAsync(taskSrc, symbol, priceType, barPeriod, from, count);
            return taskSrc.Task;
        }

        public static Task<Quote[]> GetQuoteListAsync(this FDK.Client.QuoteStore client, string symbol, QuoteDepth quoteDepth, DateTime from, int count)
        {
            var taskSrc = new TaskCompletionSource<Quote[]>();
            client.GetQuoteListAsync(taskSrc, symbol, quoteDepth, from, count);
            return taskSrc.Task;
        }

        public static Task<HistoryInfo> GetBarsHistoryInfoAsync(this FDK.Client.QuoteStore client, string symbol, BarPeriod period, PriceType priceType)
        {
            var taskSrc = new TaskCompletionSource<HistoryInfo>();
            client.GetBarsHistoryInfoAsync(taskSrc, symbol, period, priceType);
            return taskSrc.Task;
        }

        public static Task<HistoryInfo> GetQuotesHistoryInfoAsync(this FDK.Client.QuoteStore client, string symbol, bool level2)
        {
            var taskSrc = new TaskCompletionSource<HistoryInfo>();
            client.GetQuotesHistoryInfoAsync(taskSrc, symbol, level2);
            return taskSrc.Task;
        }


        //public static void DownloadBarsAsync(this FDK.QuoteStore.Client client, BlockingChannel<BarEntity> stream, string symbol, PriceType priceType, BarPeriod barPeriod, DateTime from, DateTime to)
        //{
        //    client.DownloadBarsAsync(stream, symbol, priceType, barPeriod, from, to);
        //}

        #endregion

        #region Trade

        public static void InitTaskAdapter(this FDK.Client.OrderEntry client)
        {
            client.ConnectResultEvent += (c, d) => SetCompleted(d);
            client.ConnectErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LoginResultEvent += (c, d) => SetCompleted(d);
            client.LoginErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LogoutResultEvent += (c, d, i) => SetCompleted(d, i);
            client.LogoutErrorEvent += (c, d, ex) => SetFailed<LogoutInfo>(d, ex);

            client.DisconnectResultEvent += (c, d, t) => SetCompleted(d, t);

            client.AccountInfoResultEvent += (c, d, r) => SetCompleted(d, r);
            client.AccountInfoErrorEvent += (c, d, ex) => SetFailed<AccountInfo>(d, ex);

            //client.OrdersBeginResultEvent += (c, d, c) =>
            client.OrdersResultEvent += (c, d, r) => ((BlockingChannel<OrderEntity>)d).Write(SfxInterop.Convert(r));
            client.OrdersEndResultEvent += (c, d) => ((BlockingChannel<OrderEntity>)d).Close();
            client.OrdersErrorEvent += (c, d, ex) => ((BlockingChannel<OrderEntity>)d).Close(ex);

            client.PositionsResultEvent += (c, d, r) => SetCompleted(d, r);
            client.PositionsErrorEvent += (c, d, ex) => SetFailed<Position[]>(d, ex);

            client.NewOrderResultEvent += (c, d, r) => SetOrderResult(d, r);
            client.NewOrderErrorEvent += (c, d, ex) => SetOrderFail(d, ex);

            client.CancelOrderResultEvent += (c, d, r) => SetOrderResult(d, r);
            client.CancelOrderErrorEvent += (c, d, ex) => SetOrderFail(d, ex);

            client.ReplaceOrderResultEvent += (c, d, r) => SetOrderResult(d, r);
            client.ReplaceOrderErrorEvent += (c, d, ex) => SetOrderFail(d, ex);

            client.ClosePositionResultEvent += (c, d, r) => SetOrderResult(d, r);
            client.ClosePositionErrorEvent += (c, d, ex) => SetOrderFail(d, ex);

            client.ClosePositionByResultEvent += (c, d, r) => SetOrderResult(d, r);
            client.ClosePositionByErrorEvent += (c, d, ex) => SetOrderFail(d, ex);
        }

        public static Task ConnectAsync(this FDK.Client.OrderEntry client, string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public static Task LoginAsync(this FDK.Client.OrderEntry client, string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public static Task LogoutAsync(this FDK.Client.OrderEntry client, string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            client.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public static Task DisconnectAsync(this FDK.Client.OrderEntry client, string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            client.DisconnectAsync(taskSrc, text);
            return taskSrc.Task;
        }

        public static Task<AccountInfo> GetAccountInfoAsync(this FDK.Client.OrderEntry client)
        {
            var taskSrc = new TaskCompletionSource<AccountInfo>();
            client.GetAccountInfoAsync(taskSrc);
            return taskSrc.Task;
        }

        public static void GetOrdersAsync(this FDK.Client.OrderEntry client, BlockingChannel<ExecutionReport> stream)
        {
            client.GetOrdersAsync(stream);
        }

        public static Task<Position[]> GetPositionsAsync(this FDK.Client.OrderEntry client)
        {
            var taskSrc = new TaskCompletionSource<Position[]>();
            client.GetPositionsAsync(taskSrc);
            return taskSrc.Task;
        }

        public static Task<List<FDK2.ExecutionReport>> NewOrderAsync(this FDK.Client.OrderEntry client, string clientOrderId, string symbol, OrderType type, OrderSide side,
            double qty, double? maxVisibleQty, double? price, double? stopPrice, OrderTimeInForce? timeInForce, DateTime? expireTime, double? stopLoss,
            double? takeProfit, string comment, string tag, int? magic, bool immediateOrCancel, double? slippage)
        {
            var taskSrc = new OrderResultSource();
            client.NewOrderAsync(taskSrc, clientOrderId, symbol, type, side, qty, maxVisibleQty, price, stopPrice, timeInForce, expireTime?.ToUniversalTime(), stopLoss, takeProfit, comment, tag, magic, immediateOrCancel, slippage);
            return taskSrc.Task;
        }

        public static Task<List<FDK2.ExecutionReport>> CancelOrderAsync(this FDK.Client.OrderEntry client, string clientOrderId, string origClientOrderId, string orderId)
        {
            var taskSrc = new OrderResultSource();
            client.CancelOrderAsync(taskSrc, clientOrderId, origClientOrderId, orderId);
            return taskSrc.Task;
        }

        public static Task<List<FDK2.ExecutionReport>> ReplaceOrderAsync(this FDK.Client.OrderEntry client, string clientOrderId, string origClientOrderId, string orderId, string symbol, OrderType type,
            OrderSide side, double newQty, double qty, double? maxVisibleQty, double? price, double? stopPrice, OrderTimeInForce? timeInForce, DateTime? expireTime, double? stopLoss,
            double? takeProfit, string comment, string tag, int? magic, bool? immediateOrCancel, double? slippage)
        {
            var taskSrc = new OrderResultSource();
            client.ReplaceOrderAsync(taskSrc, clientOrderId, origClientOrderId, orderId, symbol, type, side, newQty, maxVisibleQty, price, stopPrice, timeInForce, expireTime?.ToUniversalTime(), stopLoss, takeProfit, false, qty, comment, tag, magic, immediateOrCancel, slippage);
            return taskSrc.Task;
        }

        public static Task<List<FDK2.ExecutionReport>> ClosePositionAsync(this FDK.Client.OrderEntry client, string clientOrderId, string orderId, double? qty)
        {
            var taskSrc = new OrderResultSource();
            client.ClosePositionAsync(taskSrc, clientOrderId, orderId, qty);
            return taskSrc.Task;
        }

        public static Task<List<FDK2.ExecutionReport>> ClosePositionByAsync(this FDK.Client.OrderEntry client, string clientOrderId, string orderId, string byOrderId)
        {
            var taskSrc = new OrderResultSource();
            client.ClosePositionByAsync(taskSrc, clientOrderId, orderId, byOrderId);
            return taskSrc.Task;
        }

        #endregion

        #region Trade History

        public static void InitTaskAdapter(this FDK.Client.TradeCapture client)
        {
            client.ConnectResultEvent += (c, d) => SetCompleted(d);
            client.ConnectErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LoginResultEvent += (c, d) => SetCompleted(d);
            client.LoginErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LogoutResultEvent += (c, d, i) => SetCompleted(d, i);
            client.LogoutErrorEvent += (c, d, ex) => SetFailed<LogoutInfo>(d, ex);

            client.DisconnectResultEvent += (c, d, t) => SetCompleted(d, t);

            client.SubscribeTradesResultEvent += (c, d, r) => { }; // Required for SubscribeTradesResultEndEvent to work(should be fixed after 2.24.66). Just ignore trade reports we wil request them later with another request
            client.SubscribeTradesResultEndEvent += (c, d) => SetCompleted(d);
            client.SubscribeTradesErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.DownloadTradesResultEvent += (c, d, r) => ((BlockingChannel<TradeReportEntity>)d).Write(SfxInterop.Convert(r));
            client.DownloadTradesResultEndEvent += (c, d) => ((BlockingChannel<TradeReportEntity>)d).Close();
            client.DownloadTradesErrorEvent += (c, d, ex) => ((BlockingChannel<TradeReportEntity>)d).Close(ex);
        }

        public static Task ConnectAsync(this FDK.Client.TradeCapture client, string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public static Task LoginAsync(this FDK.Client.TradeCapture client, string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public static Task LogoutAsync(this FDK.Client.TradeCapture client, string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            client.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public static Task DisconnectAsync(this FDK.Client.TradeCapture client, string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            client.DisconnectAsync(taskSrc, text);
            return taskSrc.Task;
        }

        public static Task SubscribeTradesAsync(this FDK.Client.TradeCapture client, bool skipCancel)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.SubscribeTradesAsync(taskSrc, DateTime.UtcNow.AddMinutes(5), skipCancel); // Request 0 trade reports, we will download them later with separate request
            return taskSrc.WithTimeout();
        }

        public static void DownloadTradesAsync(this FDK.Client.TradeCapture client, TimeDirection timeDirection, DateTime? from, DateTime? to, bool skipCancel,
            BlockingChannel<TradeReportEntity> stream)
        {
            client.DownloadTradesAsync(stream, timeDirection, from, to, skipCancel);
        }

        #endregion

        #region Helpers

        private static void SetCompleted(object state)
        {
            SetCompleted<object>(state, null);
        }

        private static void SetOrderResult(object state, FDK2.ExecutionReport result)
        {
            ((OrderResultSource)state).OnReport(result);
        }

        private static void SetOrderFail(object state, Exception ex)
        {
            var eex = ex as ExecutionException;
            if (eex != null)
            {
                var report = eex.Report;
                if (report.OrderType == OrderType.Market && report.OrderStatus == FDK2.OrderStatus.Rejected && report.InitialVolume != report.LeavesVolume)
                {
                    ((OrderResultSource)state).OnReport(report);
                    return;
                }
            }
            ((OrderResultSource)state).SetException(Convert(ex));
        }

        private class OrderResultSource : TaskCompletionSource<List<FDK2.ExecutionReport>>
        {
            private List<FDK2.ExecutionReport> _reports = new List<FDK2.ExecutionReport>();

            public void OnReport(FDK2.ExecutionReport report)
            {
                _reports.Add(report);
                if (report.Last)
                    SetResult(_reports);
            }
        }

        private static void SetCompleted<T>(object state, T result)
        {
            if (state != null)
            {
                var src = (TaskCompletionSource<T>)state;
                src.SetResult(result);
            }
        }

        private static void SetFailed(object state, Exception ex)
        {
            SetFailed<object>(state, ex);
        }

        private static void SetFailed<T>(object state, Exception ex)
        {
            if (state != null)
            {
                var src = (TaskCompletionSource<T>)state;
                src.SetException(Convert(ex));
            }
        }

        private static Exception Convert(Exception ex)
        {
            if (ex is RejectException)
                return new InteropException(ex.Message, ConnectionErrorCodes.RejectedByServer);
            if (ex is FDK2.TimeoutException)
                return new InteropException(ex.Message, ConnectionErrorCodes.Timeout);
            return ex;
        }

        private static FDK2.SymbolEntry[] GetSymbolEntries(string[] symbolIds, int marketDepth)
        {
            return symbolIds.Select(id => new FDK2.SymbolEntry { Id = id, MarketDepth = (ushort)marketDepth }).ToArray();
        }

        private static Task<T> WithTimeout<T>(this TaskCompletionSource<T> taskSrc)
        {
            var timeoutCancelSrc = new CancellationTokenSource();
            Task.WhenAny(taskSrc.Task, Task.Delay(5 * 60 * 1000, timeoutCancelSrc.Token))
                .ContinueWith(t =>
                {
                    try
                    {
                        if (t.Result == taskSrc.Task)
                        {
                            timeoutCancelSrc.Cancel();
                        }
                        else
                        {
                            taskSrc.SetException(new InteropException("Request timed out.", ConnectionErrorCodes.Timeout));
                        }
                    }
                    finally
                    {
                        timeoutCancelSrc.Dispose();
                    }
                });
            return taskSrc.Task;
        }

        #endregion
    }
}
