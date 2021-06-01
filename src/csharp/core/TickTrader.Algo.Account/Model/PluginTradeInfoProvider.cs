using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class PluginTradeInfoProvider : IAccountInfoProvider
    {
        private EntityCache _cache;
        private event Action<Domain.OrderExecReport> AlgoEvent_OrderUpdated = delegate { };
        private event Action<Domain.PositionExecReport> AlgoEvent_PositionUpdated = delegate { };
        private event Action<Domain.BalanceOperation> AlgoEvent_BalanceUpdated = delegate { };
        private ISyncContext _sync;

        public PluginTradeInfoProvider(EntityCache cache, ISyncContext sync)
        {
            _cache = cache;
            _sync = sync;

            _cache.Account.OrderUpdate += Account_OrderUpdate;
            _cache.Account.BalanceOperationUpdate += Account_BalanceUpdate;
            _cache.Account.PositionUpdate += Account_PositionUpdate;
        }

        private void Account_OrderUpdate(OrderUpdateInfo update)
        {
            ExecReportToAlgo(update.ExecAction, update.EntityAction, update.Report, update.Order, update.Position);
        }

        private void Account_BalanceUpdate(Domain.BalanceOperation rep)
        {
            AlgoEvent_BalanceUpdated?.Invoke(rep);
        }

        private void Account_PositionUpdate(PositionInfo position, Domain.OrderExecReport.Types.ExecAction action)
        {
            AlgoEvent_PositionUpdated(new Domain.PositionExecReport() { PositionCopy = position, ExecAction = action });
        }

        private void ExecReportToAlgo(Domain.OrderExecReport.Types.ExecAction action, Domain.OrderExecReport.Types.EntityAction entityAction, ExecutionReport report, OrderInfo newOrder, Domain.PositionInfo position)
        {
            var algoReport = new Domain.OrderExecReport();
            if (newOrder != null)
                algoReport.OrderCopy = newOrder;
            algoReport.OperationId = GetOperationId(report);
            algoReport.ExecAction = action;
            algoReport.EntityAction = entityAction;
            if (algoReport.ExecAction == Domain.OrderExecReport.Types.ExecAction.Rejected)
                algoReport.ResultCode = report.RejectReason;
            if (!double.IsNaN(report.Balance))
                algoReport.NewBalance = report.Balance;
            if (report.Assets != null)
                algoReport.Assets.AddRange(report.Assets);
            algoReport.NetPositionCopy = position;
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

        public void Dispose()
        {
            _cache.Account.OrderUpdate -= Account_OrderUpdate;
            _cache.Account.BalanceOperationUpdate -= Account_BalanceUpdate;
            _cache.Account.PositionUpdate -= Account_PositionUpdate;
        }

        #region IAccountInfoProvider

        void IAccountInfoProvider.SyncInvoke(Action syncAction)
        {
            _sync.Invoke(syncAction);
        }

        Domain.AccountInfo IAccountInfoProvider.GetAccountInfo()
        {
            return _cache.Account.GetAccountInfo();
        }

        List<Domain.OrderInfo> IAccountInfoProvider.GetOrders()
        {
            return _cache.Account.Orders.Snapshot.Select(pair => pair.Value).ToList();
        }

        List<Domain.PositionInfo> IAccountInfoProvider.GetPositions()
        {
            return _cache.Account.Positions.Snapshot.Values.ToList();
        }

        event Action<Domain.OrderExecReport> IAccountInfoProvider.OrderUpdated
        {
            add { AlgoEvent_OrderUpdated += value; }
            remove { AlgoEvent_OrderUpdated -= value; }
        }

        event Action<Domain.PositionExecReport> IAccountInfoProvider.PositionUpdated
        {
            add { AlgoEvent_PositionUpdated += value; }
            remove { AlgoEvent_PositionUpdated -= value; }
        }

        event Action<Domain.BalanceOperation> IAccountInfoProvider.BalanceUpdated
        {
            add { AlgoEvent_BalanceUpdated += value; }
            remove { AlgoEvent_BalanceUpdated -= value; }
        }

        #endregion
    }
}
