using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class RuntimeInfoProvider : CrossDomainObject, IAccountInfoProvider, ITradeExecutor, ITradeHistoryProvider
    {
        private readonly UnitRuntimeV1Handler _handler;
        private readonly IAccountInfoProvider _remotingAccInfo;


        public RuntimeInfoProvider(UnitRuntimeV1Handler handler, IAccountInfoProvider remotingAccInfo)
        {
            _handler = handler;
            _remotingAccInfo = remotingAccInfo;

            _handler.OrderUpdated += o => OrderUpdated?.Invoke(o);
            _handler.PositionUpdated += p => PositionUpdated?.Invoke(p);
            _handler.BalanceUpdated += b => BalanceUpdated?.Invoke(b);
        }


        #region IAccountInfoProvider

        public event Action<Domain.OrderExecReport> OrderUpdated;
        public event Action<Domain.PositionExecReport> PositionUpdated;
        public event Action<BalanceOperation> BalanceUpdated;

        public AccountInfo GetAccountInfo()
        {
            return _handler.GetAccountInfo();
        }

        public List<OrderInfo> GetOrders()
        {
            return _handler.GetOrderList();
        }

        public List<PositionInfo> GetPositions()
        {
            return _handler.GetPositionList();
        }

        public void SyncInvoke(Action action)
        {
            _remotingAccInfo.SyncInvoke(action);
        }

        #endregion IAccountInfoProvider

        #region ITradeExecutor

        public void SendOpenOrder(Domain.OpenOrderRequest request)
        {
            _handler.SendOpenOrder(request);
        }

        public void SendModifyOrder(Domain.ModifyOrderRequest request)
        {
            _handler.SendModifyOrder(request);
        }

        public void SendCloseOrder(Domain.CloseOrderRequest request)
        {
            _handler.SendCloseOrder(request);
        }

        public void SendCancelOrder(Domain.CancelOrderRequest request)
        {
            _handler.SendCancelOrder(request);
        }

        #endregion ITradeExecutor

        #region ITradeHistoryProvider

        public IAsyncPagedEnumerator<Domain.TradeReportInfo> GetTradeHistory(DateTime? from, DateTime? to, TradeHistoryRequestOptions options)
        {
            return _handler.GetTradeHistory(new TradeHistoryRequest { From = from?.ToUniversalTime().ToTimestamp(), To = to?.ToUniversalTime().ToTimestamp(), Options = options });
        }

        #endregion ITradeHistoryProvider
    }
}
