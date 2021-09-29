using ActorSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.FDK.Common;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Account.Fdk2
{
    public class Fdk2TradeAdapter
    {
        private readonly FDK.Client.OrderEntry _tradeProxy;
        private readonly Action<FDK2.ExecutionReport> _execReportHandler;


        public Fdk2TradeAdapter(FDK.Client.OrderEntry tradeProxy, Action<FDK2.ExecutionReport> execReportHandler)
        {
            _tradeProxy = tradeProxy;
            _execReportHandler = execReportHandler;

            _tradeProxy.ConnectResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _tradeProxy.ConnectErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _tradeProxy.LoginResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _tradeProxy.LoginErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _tradeProxy.LogoutResultEvent += (c, d, i) => SfxTaskAdapter.SetCompleted(d, i);
            _tradeProxy.LogoutErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<LogoutInfo>(d, ex);

            _tradeProxy.DisconnectResultEvent += (c, d, t) => SfxTaskAdapter.SetCompleted(d, t);

            _tradeProxy.AccountInfoResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _tradeProxy.AccountInfoErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<AccountInfo>(d, ex);

            //client.OrdersBeginResultEvent += (c, d, c) =>
            _tradeProxy.OrdersResultEvent += (c, d, r) => ((BlockingChannel<Domain.OrderInfo>)d).Write(SfxInterop.Convert(r));
            _tradeProxy.OrdersEndResultEvent += (c, d) => ((BlockingChannel<Domain.OrderInfo>)d).Close();
            _tradeProxy.OrdersErrorEvent += (c, d, ex) => ((BlockingChannel<Domain.OrderInfo>)d).Close(ex);

            _tradeProxy.PositionsResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _tradeProxy.PositionsErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<Position[]>(d, ex);

            _tradeProxy.NewOrderResultEvent += (c, d, r) => SetOrderResult(d, r);
            _tradeProxy.NewOrderErrorEvent += (c, d, ex) => SetOrderFail(d, ex);

            _tradeProxy.CancelOrderResultEvent += (c, d, r) => SetOrderResult(d, r);
            _tradeProxy.CancelOrderErrorEvent += (c, d, ex) => SetOrderFail(d, ex);

            _tradeProxy.ReplaceOrderResultEvent += (c, d, r) => SetOrderResult(d, r);
            _tradeProxy.ReplaceOrderErrorEvent += (c, d, ex) => SetOrderFail(d, ex);

            _tradeProxy.ClosePositionResultEvent += (c, d, r) => SetOrderResult(d, r);
            _tradeProxy.ClosePositionErrorEvent += (c, d, ex) => SetOrderFail(d, ex);

            _tradeProxy.ClosePositionByResultEvent += (c, d, r) => SetOrderResult(d, r);
            _tradeProxy.ClosePositionByErrorEvent += (c, d, ex) => SetOrderFail(d, ex);
        }


        public Task Deinit()
        {
            return Task.Factory.StartNew(() => _tradeProxy.Dispose());
        }


        public Task ConnectAsync(string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _tradeProxy.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public Task LoginAsync(string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _tradeProxy.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public Task LogoutAsync(string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            _tradeProxy.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public Task DisconnectAsync(string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            if (!_tradeProxy.DisconnectAsync(taskSrc, SoftFX.Net.Core.Reason.ClientRequest(text)))
                taskSrc.SetResult("Already disconnected!");
            return taskSrc.Task;
        }

        public Task<AccountInfo> GetAccountInfoAsync()
        {
            var taskSrc = new TaskCompletionSource<AccountInfo>();
            _tradeProxy.GetAccountInfoAsync(taskSrc);
            return taskSrc.Task;
        }

        public void GetOrdersAsync(BlockingChannel<Domain.OrderInfo> stream)
        {
            _tradeProxy.GetOrdersAsync(stream);
        }

        public Task<Position[]> GetPositionsAsync()
        {
            var taskSrc = new TaskCompletionSource<Position[]>();
            _tradeProxy.GetPositionsAsync(taskSrc);
            return taskSrc.Task;
        }

        public Task<List<FDK2.ExecutionReport>> NewOrderAsync(string clientOrderId, string symbol, OrderType type, OrderSide side,
            double qty, double? maxVisibleQty, double? price, double? stopPrice, OrderTimeInForce? timeInForce, DateTime? expireTime, double? stopLoss,
            double? takeProfit, string comment, string tag, int? magic, bool immediateOrCancel, double? slippage, bool oneCancelsTheOtherFlag, bool ocoEqualQty, long? relatedOrderId,
            ContingentOrderTriggerType? otoTriggerType, DateTime? otoTriggerTime, long? otoTriggereById)
        {
            var taskSrc = new OrderResultSource(_execReportHandler);
            _tradeProxy.NewOrderAsync(taskSrc, clientOrderId, symbol, type, side, qty, maxVisibleQty, price, stopPrice, timeInForce, expireTime?.ToUniversalTime(), stopLoss, takeProfit, comment, tag, magic, immediateOrCancel, slippage, oneCancelsTheOtherFlag, ocoEqualQty, relatedOrderId, otoTriggerType, otoTriggerTime, otoTriggereById);
            return taskSrc.Task;
        }

        public Task<List<FDK2.ExecutionReport>> NewOcoOrdersAsync(string symbol, int timeout,

            string clientOrderId, OrderType type, OrderSide side, double qty, double? maxVisibleQty, double? price, double? stopPrice,
            OrderTimeInForce? timeInForce, DateTime? expireTime, double? stopLoss, double? takeProfit, string comment, string tag, int? magic, double? slippage,

            string clientOrderId2, OrderType type2, OrderSide side2, double qty2, double? maxVisibleQty2, double? price2, double? stopPrice2,
            OrderTimeInForce? timeInForce2, DateTime? expireTime2, double? stopLoss2, double? takeProfit2, string comment2, string tag2, int? magic2, double? slippage2,

            ContingentOrderTriggerType? otoTriggerType, DateTime? otoTriggerTime, long? otoTriggereById)
        {
            var taskSrc = new OrderResultSource(_execReportHandler);

            var ttsReports = _tradeProxy.OpenOcoOrders(symbol, clientOrderId, type, side, qty, maxVisibleQty, price, stopPrice, timeInForce, expireTime, stopLoss,
                takeProfit, comment, tag, magic, slippage, clientOrderId2, type2, side2, qty2, maxVisibleQty2, price2, stopPrice2, timeInForce2, expireTime2, stopLoss2,
                takeProfit2, comment2, tag2, magic2, slippage2, timeout, otoTriggerType, otoTriggerTime, otoTriggereById);

            taskSrc.OnMultipleReports(ttsReports);

            return taskSrc.Task;
        }

        public Task<List<FDK2.ExecutionReport>> CancelOrderAsync(string clientOrderId, string origClientOrderId, string orderId)
        {
            var taskSrc = new OrderResultSource(_execReportHandler);
            _tradeProxy.CancelOrderAsync(taskSrc, clientOrderId, origClientOrderId, orderId);
            return taskSrc.Task;
        }

        public Task<List<FDK2.ExecutionReport>> ReplaceOrderAsync(string clientOrderId, string origClientOrderId, string orderId, string symbol, OrderType type,
            OrderSide side, double newQty, double qty, double? maxVisibleQty, double? price, double? stopPrice, OrderTimeInForce? timeInForce, DateTime? expireTime, double? stopLoss,
            double? takeProfit, string comment, string tag, int? magic, bool? immediateOrCancel, double? slippage)
        {
            var taskSrc = new OrderResultSource(_execReportHandler);
            _tradeProxy.ReplaceOrderAsync(taskSrc, clientOrderId, origClientOrderId, orderId, symbol, type, side, newQty, maxVisibleQty, price, stopPrice, timeInForce, expireTime?.ToUniversalTime(), stopLoss, takeProfit, false, qty, comment, tag, magic, immediateOrCancel, slippage);
            return taskSrc.Task;
        }

        public Task<List<FDK2.ExecutionReport>> ReplaceOrderAsync(string clientOrderId, string origClientOrderId, string orderId, string symbol, OrderType type,
            OrderSide side, double qtyChange, double? maxVisibleQty, double? price, double? stopPrice, OrderTimeInForce? timeInForce, DateTime? expireTime, double? stopLoss,
            double? takeProfit, string comment, string tag, int? magic, bool? immediateOrCancel, double? slippage, bool? isOneCancelsTheOther, bool? ocoEqualQty, long? relatedId,
            ContingentOrderTriggerType? otoTriggerType, DateTime? otoTriggerTime, long? otoTriggerById)
        {
            var taskSrc = new OrderResultSource(_execReportHandler);
            _tradeProxy.ReplaceOrderAsync(taskSrc, clientOrderId, origClientOrderId, orderId, symbol, type, side, qtyChange, maxVisibleQty, price, stopPrice, timeInForce, expireTime?.ToUniversalTime(), stopLoss, takeProfit, comment, tag, magic, immediateOrCancel, slippage, isOneCancelsTheOther, ocoEqualQty, relatedId, otoTriggerType, otoTriggerTime, otoTriggerById);
            return taskSrc.Task;
        }

        public Task<List<FDK2.ExecutionReport>> ClosePositionAsync(string clientOrderId, string orderId, double? qty, double? slippage)
        {
            var taskSrc = new OrderResultSource(_execReportHandler);
            _tradeProxy.ClosePositionAsync(taskSrc, clientOrderId, orderId, qty, slippage);
            return taskSrc.Task;
        }

        public Task<List<FDK2.ExecutionReport>> ClosePositionByAsync(string clientOrderId, string orderId, string byOrderId)
        {
            var taskSrc = new OrderResultSource(_execReportHandler);
            _tradeProxy.ClosePositionByAsync(taskSrc, clientOrderId, orderId, byOrderId);
            return taskSrc.Task;
        }

        #region Helpers

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
            ((OrderResultSource)state).SetException(SfxTaskAdapter.Convert(ex));
        }

        private class OrderResultSource : TaskCompletionSource<List<FDK2.ExecutionReport>>
        {
            private readonly Action<FDK2.ExecutionReport> _execReportHandler;

            private List<FDK2.ExecutionReport> _reports = new List<FDK2.ExecutionReport>();


            public OrderResultSource(Action<FDK2.ExecutionReport> execReportHandler)
            {
                _execReportHandler = execReportHandler;
            }


            public void OnReport(FDK2.ExecutionReport report)
            {
                _execReportHandler?.Invoke(report);

                if (report.Last)
                    SetResult(_reports);
            }

            public void OnMultipleReports(FDK2.ExecutionReport[] reports)
            {
                _reports = reports.ToList();

                foreach (var report in _reports)
                    _execReportHandler?.Invoke(report);

                SetResult(_reports);
            }
        }

        #endregion Helpers
    }
}
