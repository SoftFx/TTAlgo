using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class TesterTradeTransaction
    {
        public OrderEntity OrderUpdate { get; private set; }
        public OrderEntityAction OrderEntityAction { get; private set; }
        public OrderExecAction OrderExecAction { get; private set; }

        public OrderEntity PositionUpdate { get; private set; }
        public OrderEntityAction PositionEntityAction { get; private set; }
        public OrderExecAction PositionExecAction { get; private set; }

        public PositionEntity NetPositionUpdate { get; private set; }
        public AssetEntity Asset1Update { get; }
        public AssetEntity Asset2Update { get; }
        public double? Balance { get; private set; }

        internal static TesterTradeTransaction OnOpenOrder(OrderAccessor order, bool isInstantOrder, FillInfo fillInfo, double balance)
        {
            var update = new TesterTradeTransaction();
            update.OrderExecAction = OrderExecAction.Opened;
            if (isInstantOrder)
                update.OnInstantOrderOpened(order);
            else if (order.RemainingAmount > 0)
                update.OnOrderAdded(order);
            else
                update.OrderUpdate = order.Entity;
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
            update.OnOrderRemoved(OrderExecAction.Canceled, order);
            return update;
        }

        internal static TesterTradeTransaction OnClosePosition(bool remove, OrderAccessor position, double balance)
        {
            var update = new TesterTradeTransaction();
            if (remove)
                update.OnPositionRemoved(OrderExecAction.Closed, position);
            else
                update.OnPositionReplaced(OrderExecAction.Closed, position);
            update.OnBalanceChanged(balance);
            return update;
        }

        internal static TesterTradeTransaction OnFill(OrderAccessor order, FillInfo fillInfo, double balance)
        {
            var update = new TesterTradeTransaction();

            if (fillInfo.Position != null && fillInfo.Position.OrderId == order.OrderId) // order was transformed into position
            {
                update.OnOrderReplaced(OrderExecAction.Filled, order);
            }
            else
            {
                if (order.RemainingAmount == 0)
                    update.OnOrderRemoved(OrderExecAction.Filled, order);
                else
                    update.OnOrderAdded(OrderExecAction.Filled, order);

                if (fillInfo.Position != null)
                    update.OnPositionAdded(OrderExecAction.Filled, fillInfo.Position);
            }

            if (fillInfo.NetPos != null)
            //if (fillInfo.WasNetPositionClosed)
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
            update.OnOrderReplaced(OrderExecAction.Modified, order);
            return update;
        }

        internal static TesterTradeTransaction OnActivateStopLimit(OrderAccessor order)
        {
            var update = new TesterTradeTransaction();
            update.OnOrderRemoved(OrderExecAction.Activated, order);
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
            OrderEntityAction = OrderEntityAction.None;
            OrderUpdate = order.Entity;
        }

        private void OnOrderAdded(OrderAccessor order)
        {
            OrderEntityAction = OrderEntityAction.Added;
            OrderUpdate = order.Entity;
        }

        private void OnOrderAdded(OrderExecAction execAction, OrderAccessor order)
        {
            OrderExecAction = execAction;
            OrderEntityAction = OrderEntityAction.Added;
            OrderUpdate = order.Entity;
        }

        private void OnOrderReplaced(OrderExecAction execAction, OrderAccessor order)
        {
            OrderExecAction = execAction;
            OrderEntityAction = OrderEntityAction.Updated;
            OrderUpdate = order.Entity;
        }

        private void OnOrderRemoved(OrderExecAction execAction, OrderAccessor order)
        {
            OrderExecAction = execAction;
            OrderEntityAction = OrderEntityAction.Removed;
            OrderUpdate = order.Entity;
        }

        private void OnPositionAdded(OrderExecAction execAction, OrderAccessor order)
        {
            PositionExecAction = execAction;
            PositionEntityAction = OrderEntityAction.Added;
            PositionUpdate = order.Entity;
        }

        private void OnPositionReplaced(OrderExecAction execAction, OrderAccessor order)
        {
            PositionExecAction = execAction;
            PositionEntityAction = OrderEntityAction.Updated;
            PositionUpdate = order.Entity;
        }

        private void OnPositionRemoved(OrderExecAction execAction, OrderAccessor order)
        {
            PositionExecAction = execAction;
            PositionEntityAction = OrderEntityAction.Removed;
            PositionUpdate = order.Entity;
        }

        private void OnPositionChanged(PositionAccessor pos)
        {
            NetPositionUpdate = pos.GetEntityCopy();
        }

        private void OnBalanceChanged(double balance)
        {
            Balance = balance;
        }
    }
}
