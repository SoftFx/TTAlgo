using System;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class TesterTradeTransaction
    {
        public Domain.OrderInfo OrderUpdate { get; private set; }
        public Domain.OrderExecReport.Types.EntityAction OrderEntityAction { get; private set; }
        public Domain.OrderExecReport.Types.ExecAction OrderExecAction { get; private set; }

        public Domain.OrderInfo PositionUpdate { get; private set; }
        public Domain.OrderExecReport.Types.EntityAction PositionEntityAction { get; private set; }
        public Domain.OrderExecReport.Types.ExecAction PositionExecAction { get; private set; }

        public Domain.PositionExecReport NetPositionUpdate { get; private set; }
        public AssetEntity Asset1Update { get; }
        public AssetEntity Asset2Update { get; }
        public double? Balance { get; private set; }

        internal static TesterTradeTransaction OnOpenOrder(OrderAccessor order, bool isInstantOrder, FillInfo fillInfo, double balance)
        {
            var update = new TesterTradeTransaction();
            update.OrderExecAction = Domain.OrderExecReport.Types.ExecAction.Opened;
            if (isInstantOrder)
                update.OnInstantOrderOpened(order);
            else if (order.Info.RemainingAmount > 0)
                update.OnOrderAdded(order);
            else
                update.OrderUpdate = order.Entity.GetInfo();
            if (fillInfo.NetPos != null)
            {
                update.OnPositionChanged(fillInfo.NetPos.ResultingPosition);
                if (fillInfo.WasNetPositionClosed)
                    update.OnBalanceChanged(balance);
            }
            return update;
        }

        internal static TesterTradeTransaction OnCancelOrder(OrderAccessor order)
        {
            var update = new TesterTradeTransaction();
            update.OnOrderRemoved(Domain.OrderExecReport.Types.ExecAction.Canceled, order);
            return update;
        }

        internal static TesterTradeTransaction OnClosePosition(bool remove, OrderAccessor position, double balance)
        {
            var update = new TesterTradeTransaction();
            if (remove)
                update.OnPositionRemoved(Domain.OrderExecReport.Types.ExecAction.Closed, position);
            else
                update.OnPositionReplaced(Domain.OrderExecReport.Types.ExecAction.Closed, position);
            update.OnBalanceChanged(balance);
            return update;
        }

        internal static TesterTradeTransaction OnFill(OrderAccessor order, FillInfo fillInfo, double balance)
        {
            var update = new TesterTradeTransaction();

            if (fillInfo.Position != null && fillInfo.Position.IsSameOrderId(order)) // order was transformed into position
            {
                update.OnOrderReplaced(Domain.OrderExecReport.Types.ExecAction.Filled, order);
            }
            else
            {
                if (order.Info.RemainingAmount == 0)
                    update.OnOrderRemoved(Domain.OrderExecReport.Types.ExecAction.Filled, order);
                else
                    update.OnOrderAdded(Domain.OrderExecReport.Types.ExecAction.Filled, order);

                if (fillInfo.Position != null)
                    update.OnPositionAdded(Domain.OrderExecReport.Types.ExecAction.Filled, fillInfo.Position);
            }

            if (fillInfo.WasNetPositionClosed)
            {
                update.OnPositionChanged(fillInfo.NetPos.ResultingPosition);
                if (fillInfo.WasNetPositionClosed)
                    update.OnBalanceChanged(balance);
            }

            return update;
        }

        internal static TesterTradeTransaction OnReplaceOrder(OrderAccessor order)
        {
            var update = new TesterTradeTransaction();
            update.OnOrderReplaced(Domain.OrderExecReport.Types.ExecAction.Modified, order);
            return update;
        }

        internal static TesterTradeTransaction OnActivateStopLimit(OrderAccessor order)
        {
            var update = new TesterTradeTransaction();
            update.OnOrderRemoved(Domain.OrderExecReport.Types.ExecAction.Activated, order);
            return update;
        }

        internal static TesterTradeTransaction OnRolloverUpdate(OrderAccessor order)
        {
            return OnReplaceOrder(order);
        }

        internal static TesterTradeTransaction OnRolloverUpdate(PositionAccessor pos)
        {
            var update = new TesterTradeTransaction();
            update.OnPositionChanged(pos);
            return update;
        }

        private void OnInstantOrderOpened(OrderAccessor order)
        {
            OrderEntityAction = Domain.OrderExecReport.Types.EntityAction.NoAction;
            OrderUpdate = order.Entity.GetInfo();
        }

        private void OnOrderAdded(OrderAccessor order)
        {
            OrderEntityAction = Domain.OrderExecReport.Types.EntityAction.Added;
            OrderUpdate = order.Entity.GetInfo();
        }

        private void OnOrderAdded(Domain.OrderExecReport.Types.ExecAction execAction, OrderAccessor order)
        {
            OrderExecAction = execAction;
            OrderEntityAction = Domain.OrderExecReport.Types.EntityAction.Added;
            OrderUpdate = order.Entity.GetInfo();
        }

        private void OnOrderReplaced(Domain.OrderExecReport.Types.ExecAction execAction, OrderAccessor order)
        {
            OrderExecAction = execAction;
            OrderEntityAction = Domain.OrderExecReport.Types.EntityAction.Updated;
            OrderUpdate = order.Entity.GetInfo();
        }

        private void OnOrderRemoved(Domain.OrderExecReport.Types.ExecAction execAction, OrderAccessor order)
        {
            OrderExecAction = execAction;
            OrderEntityAction = Domain.OrderExecReport.Types.EntityAction.Removed;
            OrderUpdate = order.Entity.GetInfo();
        }

        private void OnPositionAdded(Domain.OrderExecReport.Types.ExecAction execAction, OrderAccessor order)
        {
            PositionExecAction = execAction;
            PositionEntityAction = Domain.OrderExecReport.Types.EntityAction.Added;
            PositionUpdate = order.Entity.GetInfo();
        }

        private void OnPositionReplaced(Domain.OrderExecReport.Types.ExecAction execAction, OrderAccessor order)
        {
            PositionExecAction = execAction;
            PositionEntityAction = Domain.OrderExecReport.Types.EntityAction.Updated;
            PositionUpdate = order.Entity.GetInfo();
        }

        private void OnPositionRemoved(Domain.OrderExecReport.Types.ExecAction execAction, OrderAccessor order)
        {
            PositionExecAction = execAction;
            PositionEntityAction = Domain.OrderExecReport.Types.EntityAction.Removed;
            PositionUpdate = order.Entity.GetInfo();
        }

        private void OnPositionChanged(PositionAccessor pos)
        {
            NetPositionUpdate = new Domain.PositionExecReport
            {
                PositionCopy = pos.Info.Clone(),
                ExecAction = Domain.OrderExecReport.Types.ExecAction.Modified,
            };
        }

        private void OnBalanceChanged(double balance)
        {
            Balance = balance;
        }
    }
}
