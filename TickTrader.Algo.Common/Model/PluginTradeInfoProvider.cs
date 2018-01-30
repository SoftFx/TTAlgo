using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class PluginTradeInfoProvider : IAccountInfoProvider
    {
        private EntityCache _cache;
        private event Action<OrderExecReport> AlgoEvent_OrderUpdated = delegate { };
        private event Action<PositionExecReport> AlgoEvent_PositionUpdated = delegate { };
        private event Action<BalanceOperationReport> AlgoEvent_BalanceUpdated = delegate { };
        private Action<Action> _sync;

        public PluginTradeInfoProvider(EntityCache cache, Action<Action> syncInvokeAction)
        {
            _cache = cache;
            _sync = syncInvokeAction;
        }

        private void ExecReportToAlgo(OrderExecAction action, OrderEntityAction entityAction, ExecutionReport report, OrderModel newOrder = null)
        {
            OrderExecReport algoReport = new OrderExecReport();
            if (newOrder != null)
                algoReport.OrderCopy = newOrder.GetEntity();
            algoReport.OperationId = GetOperationId(report);
            algoReport.OrderId = report.OrderId;
            algoReport.ExecAction = action;
            algoReport.Action = entityAction;
            if (algoReport.ExecAction == OrderExecAction.Rejected)
                algoReport.ResultCode = report.RejectReason;
            if (!double.IsNaN(report.Balance))
                algoReport.NewBalance = report.Balance;
            if (report.Assets != null)
                algoReport.Assets = report.Assets.Select(assetInfo => new AssetModel(assetInfo, _cache.Currencies.Snapshot).GetEntity()).ToList();
            AlgoEvent_OrderUpdated(algoReport);
        }

        private string GetOperationId(ExecutionReport report)
        {
            if (!string.IsNullOrEmpty(report.ClosePositionRequestId))
                return report.ClosePositionRequestId;
            if (!string.IsNullOrEmpty(report.TradeRequestId))
                return report.TradeRequestId;
            return report.ClientOrderId;
        }


        #region IAccountInfoProvider

        void IAccountInfoProvider.SyncInvoke(Action syncAction)
        {
            _sync(syncAction);
        }

        AccountEntity IAccountInfoProvider.AccountInfo => throw new NotImplementedException();

        List<OrderEntity> IAccountInfoProvider.GetOrders()
        {
            return _cache.Account.Orders.Snapshot.Select(pair => pair.Value.GetEntity()).ToList();
        }

        IEnumerable<PositionExecReport> IAccountInfoProvider.GetPositions()
        {
            return _cache.Account.Positions.Snapshot.Select(pair => pair.Value.ToReport(OrderExecAction.Opened)).ToList();
        }

        event Action<OrderExecReport> IAccountInfoProvider.OrderUpdated
        {
            add { AlgoEvent_OrderUpdated += value; }
            remove { AlgoEvent_OrderUpdated -= value; }
        }

        event Action<PositionExecReport> IAccountInfoProvider.PositionUpdated
        {
            add { AlgoEvent_PositionUpdated += value; }
            remove { AlgoEvent_PositionUpdated -= value; }
        }

        event Action<BalanceOperationReport> IAccountInfoProvider.BalanceUpdated
        {
            add { AlgoEvent_BalanceUpdated += value; }
            remove { AlgoEvent_BalanceUpdated -= value; }
        }

        #endregion
    }
}
