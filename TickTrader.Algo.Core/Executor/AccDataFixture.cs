using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class AccDataFixture : NoTimeoutByRefObject
    {
        private IFixtureContext context;

        public AccDataFixture(IFixtureContext context)
        {
            this.context = context;
        }

        public IAccountInfoProvider DataProvider { get; set; }

        public void Start()
        {
            if (DataProvider != null)
                DataProvider.SyncInvoke(InitOrders);
        }

        private void InitOrders()
        {
            DataProvider.OrderUpdated += DataProvider_OrderUpdated;
            var builder = context.Builder;
            foreach (var order in DataProvider.GetOrders())
                builder.Account.Orders.Add(order);
        }

        public void Stop()
        {
            DataProvider.SyncInvoke(Deinit);
        }

        private void Deinit()
        {
            DataProvider.OrderUpdated -= DataProvider_OrderUpdated;
        }

        private static void ApplyOrderEntity(OrderExecReport eReport, OrdersCollection collection)
        {
            if (eReport.Action == OrderEntityAction.Added)
                collection.Add(eReport.OrderCopy);
            else if (eReport.Action == OrderEntityAction.Removed)
                collection.Remove(eReport.OrderId);
            else if (eReport.Action == OrderEntityAction.Updated)
                collection.Replace(eReport.OrderCopy);
        }

        private void DataProvider_OrderUpdated(OrderExecReport eReport)
        {
            context.PluginInvoke(b =>
            {
                var orderCollection = b.Account.Orders;

                if (eReport.ExecAction == OrderExecAction.Opened)
                {
                    ApplyOrderEntity(eReport, orderCollection);
                    orderCollection.FireOrderOpened(new OrderOpenedEventArgsImpl(null));
                }
                else if (eReport.ExecAction == OrderExecAction.Closed)
                {
                    var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                    if (oldOrder != null)
                    {
                        ApplyOrderEntity(eReport, orderCollection);
                        orderCollection.FireOrderClosed(new OrderClosedEventArgsImpl(oldOrder));
                    }
                }
                else if (eReport.ExecAction == OrderExecAction.Canceled)
                {
                    var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                    if (oldOrder != null)
                    {
                        ApplyOrderEntity(eReport, orderCollection);
                        orderCollection.FireOrderCanceled(new OrderCanceledEventArgsImpl(oldOrder));
                    }
                }
                else if (eReport.ExecAction == OrderExecAction.Expired)
                {
                    var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                    if (oldOrder != null)
                    {
                        ApplyOrderEntity(eReport, orderCollection);
                        orderCollection.FireOrderExpired(new OrderCanceledEventArgsImpl(oldOrder));
                    }
                }
                else if (eReport.ExecAction == OrderExecAction.Modified)
                {
                    var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                    if (oldOrder != null && eReport.OrderCopy != null)
                    {
                        ApplyOrderEntity(eReport, orderCollection);
                        orderCollection.FireOrderModified(new OrderModifiedEventArgsImpl(oldOrder, eReport.OrderCopy));
                    }
                }
                else if (eReport.ExecAction == OrderExecAction.Filled)
                {
                    var oldOrder = orderCollection.GetOrderOrNull(eReport.OrderId);
                    if (oldOrder != null && eReport.OrderCopy != null)
                    {
                        ApplyOrderEntity(eReport, orderCollection);
                        orderCollection.FireOrderFilled(new OrderFilledEventArgsImpl(oldOrder, eReport.OrderCopy));
                    }
                }
            });
        }
    }
}
