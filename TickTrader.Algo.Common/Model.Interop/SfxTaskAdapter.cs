using ActorSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.FDK.Common;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Common.Model
{
    public static class SfxTaskAdapter
    {
        #region Quote

        public static void InitTaskAdapter(this FDK.QuoteFeed.Client client)
        {
            client.ConnectResultEvent += (c, d) => SetCompleted(d);
            client.ConnectErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LoginResultEvent += (c, d) => SetCompleted(d);
            client.LoginErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.QuotesResultEvent += (c, d, r) => SetCompleted(d, SfxInterop.Convert(r));
            client.QuotesErrorEvent += (c, d, ex) => SetFailed<QuoteEntity[]>(d, ex);

            client.CurrencyListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.CurrencyListErrorEvent += (c, d, ex) => SetFailed<CurrencyInfo[]>(d, ex);

            client.SymbolListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.SymbolListErrorEvent += (c, d, ex) => SetFailed<SymbolInfo[]>(d, ex);

            client.SubscribeQuotesResultEvent += (c, d, r) => SetCompleted(d, SfxInterop.Convert(r));
            client.SubscribeQuotesErrorEvent += (c, d, ex) => SetFailed<QuoteEntity[]>(d, ex);

            client.QuotesResultEvent += (c, d, r) => SetCompleted(d, SfxInterop.Convert(r));
            client.QuotesErrorEvent += (c, d, ex) => SetFailed<QuoteEntity[]>(d, ex);
        }

        public static Task ConnectAsync(this FDK.QuoteFeed.Client client, string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public static Task LoginAsync(this FDK.QuoteFeed.Client client, string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public static Task<CurrencyInfo[]> GetCurrencyListAsync(this FDK.QuoteFeed.Client client)
        {
            var taskSrc = new TaskCompletionSource<CurrencyInfo[]>();
            client.GetCurrencyListAsync(taskSrc);
            return taskSrc.Task;
        }

        public static Task<SymbolInfo[]> GetSymbolListAsync(this FDK.QuoteFeed.Client client)
        {
            var taskSrc = new TaskCompletionSource<SymbolInfo[]>();
            client.GetSymbolListAsync(taskSrc);
            return taskSrc.Task;
        }

        public static Task<QuoteEntity[]> SubscribeQuotesAsync(this FDK.QuoteFeed.Client client, string[] symbolIds, int marketDepth)
        {
            var taskSrc = new TaskCompletionSource<QuoteEntity[]>();
            client.SubscribeQuotesAsync(taskSrc, symbolIds, marketDepth);
            return taskSrc.Task;
        }

        public static Task<Quote[]> GetQuotesAsync(this FDK.QuoteFeed.Client client, string[] symbolIds, int marketDepth)
        {
            var taskSrc = new TaskCompletionSource<Quote[]>();
            client.GetQuotesAsync(taskSrc, symbolIds, marketDepth);
            return taskSrc.Task;
        }

        #endregion

        #region Quote History

        public static void InitTaskAdapter(this FDK.QuoteStore.Client client)
        {
            client.ConnectResultEvent += (c, d) => SetCompleted(d);
            client.ConnectErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LoginResultEvent += (c, d) => SetCompleted(d);
            client.LoginErrorEvent += (c, d, ex) => SetFailed(d, ex);
            
            client.BarListResultEvent += (c, d, r) => SetCompleted(d, r);
            client.BarListErrorEvent += (c, d, ex) => SetFailed<Bar[]>(d, ex);

            //client.BarDownloadResultEvent += (c, d, r) => ((BlockingChannel<BarEntity>)d)?.Write(SfxInterop.Convert(r));
            //client.BarDownloadResultEndEvent += (c, d) => ((BlockingChannel<BarEntity>)d)?.Close();
            //client.BarDownloadErrorEvent += (c, d, ex) => ((BlockingChannel<BarEntity>)d)?.Close(ex);
        }

        public static Task ConnectAsync(this FDK.QuoteStore.Client client, string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public static Task LoginAsync(this FDK.QuoteStore.Client client, string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public static Task<Bar[]> GetBarListAsync(this FDK.QuoteStore.Client client, string symbol, PriceType priceType, BarPeriod barPeriod, DateTime from, int count)
        {
            var taskSrc = new TaskCompletionSource<Bar[]>();
            client.GetBarListAsync(taskSrc, symbol, priceType, barPeriod, from, count);
            return taskSrc.Task;
        }

        //public static void DownloadBarsAsync(this FDK.QuoteStore.Client client, BlockingChannel<BarEntity> stream, string symbol, PriceType priceType, BarPeriod barPeriod, DateTime from, DateTime to)
        //{
        //    client.DownloadBarsAsync(stream, symbol, priceType, barPeriod, from, to);
        //}

        #endregion

        #region Trade

        public static void InitTaskAdapter(this FDK.OrderEntry.Client client)
        {
            client.ConnectResultEvent += (c, d) => SetCompleted(d);
            client.ConnectErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LoginResultEvent += (c, d) => SetCompleted(d);
            client.LoginErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.AccountInfoResultEvent += (c, d, r) => SetCompleted(d, r);
            client.AccountInfoErrorEvent += (c, d, ex) => SetFailed<AccountInfo>(d, ex);

            //client.OrdersBeginResultEvent += (c, d, c) =>
            client.OrdersResultEvent += (c, d, r) => ((BlockingChannel<OrderEntity>)d).Write(SfxInterop.Convert(r));
            client.OrdersEndResultEvent += (c, d) => ((BlockingChannel<OrderEntity>)d).Close();
            client.OrdersErrorEvent += (c, d, ex) => ((BlockingChannel<OrderEntity>)d).Close(ex);

            client.PositionsResultEvent += (c, d, r) => SetCompleted(d, r);
            client.PositionsErrorEvent += (c, d, ex) => SetFailed<Position[]>(d, ex);

            client.NewOrderResultEvent += (c, d, r) => SetCompleted(d, r);
            client.NewOrderErrorEvent += (c, d, ex) => SetFailed<FDK2.ExecutionReport>(d, ex);

            client.CancelOrderResultEvent += (c, d, r) => SetCompleted(d, r);
            client.CancelOrderErrorEvent += (c, d, ex) => SetFailed<FDK2.ExecutionReport>(d, ex);

            client.ReplaceOrderResultEvent += (c, d, r) => SetCompleted(d, r);
            client.ReplaceOrderErrorEvent += (c, d, ex) => SetFailed<FDK2.ExecutionReport>(d, ex);

            client.ClosePositionResultEvent += (c, d, r) => SetCompleted(d, r);
            client.ClosePositionErrorEvent += (c, d, ex) => SetFailed<FDK2.ExecutionReport>(d, ex);

            client.ClosePositionByResultEvent += (c, d, r) => SetCompleted(d, r);
            client.ClosePositionByErrorEvent += (c, d, ex) => SetFailed<FDK2.ExecutionReport>(d, ex);
        }

        public static Task ConnectAsync(this FDK.OrderEntry.Client client, string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public static Task LoginAsync(this FDK.OrderEntry.Client client, string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public static Task<AccountInfo> GetAccountInfoAsync(this FDK.OrderEntry.Client client)
        {
            var taskSrc = new TaskCompletionSource<AccountInfo>();
            client.GetAccountInfoAsync(taskSrc);
            return taskSrc.Task;
        }

        public static void GetOrdersAsync(this FDK.OrderEntry.Client client, BlockingChannel<ExecutionReport> stream)
        {
            client.GetOrdersAsync(stream);
        }

        public static Task<Position[]> GetPositionsAsync(this FDK.OrderEntry.Client client)
        {
            var taskSrc = new TaskCompletionSource<Position[]>();
            client.GetPositionsAsync(taskSrc);
            return taskSrc.Task;
        }

        public static Task<FDK2.ExecutionReport> NewOrderAsync(this FDK.OrderEntry.Client client, string clientOrderId, string symbol, OrderType type, OrderSide side,
            double qty, double? maxVisibleQty, double? price, double? stopPrice, OrderTimeInForce? timeInForce, DateTime? expireTime, double? stopLoss,
            double? takeProfit, string comment, string tag, int? magic)
        {
            var taskSrc = new TaskCompletionSource<FDK2.ExecutionReport>();
            client.NewOrderAsync(taskSrc, clientOrderId, symbol, type, side, qty, maxVisibleQty, price, stopPrice, timeInForce, expireTime?.ToUniversalTime(), stopLoss, takeProfit, comment, tag, magic);
            return taskSrc.Task;
        }

        public static Task<FDK2.ExecutionReport> CancelOrderAsync(this FDK.OrderEntry.Client client, string clientOrderId, string origClientOrderId, string orderId)
        {
            var taskSrc = new TaskCompletionSource<FDK2.ExecutionReport>();
            client.CancelOrderAsync(taskSrc, clientOrderId, origClientOrderId, orderId);
            return taskSrc.Task;
        }

        public static Task<FDK2.ExecutionReport> ReplaceOrderAsync(this FDK.OrderEntry.Client client, string clientOrderId, string origClientOrderId, string orderId, string symbol, OrderType type,
            OrderSide side, double newQty, double qty, double? maxVisibleQty, double? price, double? stopPrice, OrderTimeInForce? timeInForce, DateTime? expireTime, double? stopLoss,
            double? takeProfit, string comment, string tag, int? magic)
        {
            var taskSrc = new TaskCompletionSource<FDK2.ExecutionReport>();
            client.ReplaceOrderAsync(taskSrc, clientOrderId, origClientOrderId, orderId, symbol, type, side, newQty, maxVisibleQty, price, stopPrice, timeInForce, expireTime?.ToUniversalTime(), stopLoss, takeProfit, false, qty, comment, tag, magic);
            return taskSrc.Task;
        }

        public static Task<FDK2.ExecutionReport> ClosePositionAsync(this FDK.OrderEntry.Client client, string clientOrderId, string orderId, double? qty)
        {
            var taskSrc = new TaskCompletionSource<FDK2.ExecutionReport>();
            client.ClosePositionAsync(taskSrc, clientOrderId, orderId, qty);
            return taskSrc.Task;
        }

        public static Task<FDK2.ExecutionReport> ClosePositionByAsync(this FDK.OrderEntry.Client client, string clientOrderId, string orderId, string byOrderId)
        {
            var taskSrc = new TaskCompletionSource<FDK2.ExecutionReport>();
            client.ClosePositionByAsync(taskSrc, clientOrderId, orderId, byOrderId);
            return taskSrc.Task;
        }

        #endregion

        #region Trade History

        public static void InitTaskAdapter(this FDK.TradeCapture.Client client)
        {
            client.ConnectResultEvent += (c, d) => SetCompleted(d);
            client.ConnectErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.LoginResultEvent += (c, d) => SetCompleted(d);
            client.LoginErrorEvent += (c, d, ex) => SetFailed(d, ex);
            
            client.SubscribeTradesResultEvent += (c, d) => SetCompleted(d);
            client.SubscribeTradesErrorEvent += (c, d, ex) => SetFailed(d, ex);

            client.TradeDownloadResultEvent += (c, d, r) => ((BlockingChannel<TradeReportEntity>)d).Write(SfxInterop.Convert(r));
            client.TradeDownloadResultEndEvent += (c, d) => ((BlockingChannel<TradeReportEntity>)d).Close();
            client.TradeDownloadErrorEvent += (c, d, ex) => ((BlockingChannel<TradeReportEntity>)d).Close(ex);
        }

        public static Task ConnectAsync(this FDK.TradeCapture.Client client, string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public static Task LoginAsync(this FDK.TradeCapture.Client client, string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public static Task SubscribeTradesAsync(this FDK.TradeCapture.Client client, bool skipCancel)
        {
            var taskSrc = new TaskCompletionSource<object>();
            client.SubscribeTradesAsync(taskSrc, skipCancel);
            return taskSrc.Task;
        }

        public static void DownloadTradesAsync(this FDK.TradeCapture.Client client, TimeDirection timeDirection, DateTime? from, DateTime? to, bool skipCancel,
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

        private static void SetCompleted(object state, FDK2.ExecutionReport result)
        {
            if (result.Last)
                SetCompleted<FDK2.ExecutionReport>(state, result);
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
                src.SetException(ex);
            }
        }

        #endregion
    }
}
