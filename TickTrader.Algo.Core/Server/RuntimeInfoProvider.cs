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

            _remotingAccInfo.OrderUpdated += o => OrderUpdated?.Invoke(o);
            _remotingAccInfo.PositionUpdated += p => PositionUpdated?.Invoke(p);
            _remotingAccInfo.BalanceUpdated += b => BalanceUpdated?.Invoke(b);
        }


        #region IAccountInfoProvider

        public AccountInfo AccountInfo => _handler.GetAccountInfo();

        public event Action<OrderExecReport> OrderUpdated;
        public event Action<PositionExecReport> PositionUpdated;
        public event Action<BalanceOperationReport> BalanceUpdated;

        public List<OrderEntity> GetOrders()
        {
            return _remotingAccInfo.GetOrders();
        }

        public IEnumerable<PositionExecReport> GetPositions()
        {
            return _remotingAccInfo.GetPositions();
        }

        public void SyncInvoke(Action action)
        {
            _remotingAccInfo.SyncInvoke(action);
        }

        #endregion IAccountInfoProvider
    }
}
