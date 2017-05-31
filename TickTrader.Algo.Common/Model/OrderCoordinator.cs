using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Core
{
    public class OrderCoordinator
    {
        private IDictionary<string, OrderEntity> _orders;

        public OrderCoordinator(IDictionary<string, OrderEntity> orderCollection,
            IDictionary<string, OrderEntity> positionCollection,
            IDictionary<string, AssetEntity> assetCollection)
        {
            _orders = orderCollection;
        }

        public void OnReport(OrderExecReport report)
        {

        }

        event Action<OrderExecReport> ExecReportReceived;
        event Action<PositionExecReport> PositionReportReceived;
        event Action<BalanceOperationReport> BalanceReportReceived;
    }
}
