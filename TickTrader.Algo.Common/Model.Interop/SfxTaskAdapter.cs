using ActorSharp;
using System;
using System.Linq;
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
            client.QuotesErrorEvent += (c, d, ex) => SetFailed<Domain.QuoteInfo[]>(d, ex);

            client.CurrencyListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.CurrencyListErrorEvent += (c, d, ex) => SetFailed<FDK2.CurrencyInfo[]>(d, ex);

            client.SymbolListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.SymbolListErrorEvent += (c, d, ex) => SetFailed<FDK2.SymbolInfo[]>(d, ex);

            client.SubscribeQuotesResultEvent += (c, d, r) => SetCompleted(d, SfxInterop.Convert(r));
            client.SubscribeQuotesErrorEvent += (c, d, ex) => SetFailed<Domain.QuoteInfo[]>(d, ex);

            client.QuotesResultEvent += (c, d, r) => SetCompleted(d, SfxInterop.Convert(r));
            client.QuotesErrorEvent += (c, d, ex) => SetFailed<Domain.QuoteInfo[]>(d, ex);
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

        public static Task<Domain.QuoteInfo[]> SubscribeQuotesAsync(this FDK.Client.QuoteFeed client, string[] symbolIds, int marketDepth)
        {
            var taskSrc = new TaskCompletionSource<Domain.QuoteInfo[]>();
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

            client.DownloadTradesResultEvent += (c, d, r) => ((BlockingChannel<Domain.TradeReportInfo>)d).Write(SfxInterop.Convert(r));
            client.DownloadTradesResultEndEvent += (c, d) => ((BlockingChannel<Domain.TradeReportInfo>)d).Close();
            client.DownloadTradesErrorEvent += (c, d, ex) => ((BlockingChannel<Domain.TradeReportInfo>)d).Close(ex);
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
            BlockingChannel<Domain.TradeReportInfo> stream)
        {
            client.DownloadTradesAsync(stream, timeDirection, from, to, skipCancel);
        }

        #endregion

        #region Helpers

        internal static void SetCompleted(object state)
        {
            SetCompleted<object>(state, null);
        }

        internal static void SetCompleted<T>(object state, T result)
        {
            if (state != null)
            {
                var src = (TaskCompletionSource<T>)state;
                src.SetResult(result);
            }
        }

        internal static void SetFailed(object state, Exception ex)
        {
            SetFailed<object>(state, ex);
        }

        internal static void SetFailed<T>(object state, Exception ex)
        {
            if (state != null)
            {
                var src = (TaskCompletionSource<T>)state;
                src.SetException(Convert(ex));
            }
        }

        internal static Exception Convert(Exception ex)
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
