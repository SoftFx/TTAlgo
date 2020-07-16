using System;
using System.Collections.Generic;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class RuntimeInfoProvider : CrossDomainObject, IAccountInfoProvider
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
    }
}
