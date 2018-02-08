using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class PluginTradeInfoProvider : CrossDomainObject, IAccountInfoProvider
    {
        private EntityCache _cache;
        private event Action<OrderExecReport> AlgoEvent_OrderUpdated = delegate { };
        private event Action<PositionExecReport> AlgoEvent_PositionUpdated = delegate { };
        private event Action<BalanceOperationReport> AlgoEvent_BalanceUpdated = delegate { };
        private ISyncContext _sync;

        public PluginTradeInfoProvider(EntityCache cache, ISyncContext sync)
        {
            _cache = cache;
            _sync = sync;

            _cache.Account.OrderUpdate += Account_OrderUpdate;
        }

        private void Account_OrderUpdate(OrderUpdateInfo update)
        {
            ExecReportToAlgo(update.ExecAction, update.EntityAction, update.Report, update.Order);
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

        public override void Dispose()
        {
            base.Dispose();

            _cache.Account.OrderUpdate -= Account_OrderUpdate;
        }

        #region IAccountInfoProvider

        void IAccountInfoProvider.SyncInvoke(Action syncAction)
        {
            _sync.Invoke(syncAction);
        }

        AccountEntity IAccountInfoProvider.AccountInfo => _cache.Account.GetAccountInfo();

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
