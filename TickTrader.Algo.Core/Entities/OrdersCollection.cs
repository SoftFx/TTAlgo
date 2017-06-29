using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class OrdersCollection
    {
        private PluginBuilder builder;
        private OrdersFixture fixture = new OrdersFixture();

        internal OrderList OrderListImpl { get { return fixture; } }
        internal bool IsEnabled { get { return true; } }

        public OrdersCollection(PluginBuilder builder)
        {
            this.builder = builder;
        }

        public void Add(OrderEntity entity)
        {
            fixture.Add(entity);
        }

        public void Replace(OrderEntity entity)
        {
            fixture.Replace(entity, IsEnabled);
        }

        public OrderEntity GetOrderOrNull(string id)
        {
            return fixture.GetOrNull(id);
        }

        public void Remove(string orderId)
        {
            fixture.Remove(orderId);
        }

        public void FireOrderOpened(OrderOpenedEventArgs args)
        {
            builder.InvokePluginMethod(() => fixture.FireOrderOpened(args));
        }

        public void FireOrderModified(OrderModifiedEventArgs args)
        {
            builder.InvokePluginMethod(() => fixture.FireOrderModified(args));
        }

        public void FireOrderClosed(OrderClosedEventArgs args)
        {
            builder.InvokePluginMethod(() => fixture.FireOrderClosed(args));
        }

        public void FireOrderCanceled(OrderCanceledEventArgs args)
        {
            builder.InvokePluginMethod(() => fixture.FireOrderCanceled(args));
        }

        public void FireOrderExpired(OrderExpiredEventArgs args)
        {
            builder.InvokePluginMethod(() => fixture.FireOrderExpired(args));
        }

        public void FireOrderFilled(OrderFilledEventArgs args)
        {
            builder.InvokePluginMethod(() => fixture.FireOrderFilled(args));
        }

        internal class OrdersFixture : OrderList
        {
            private ConcurrentDictionary<string, OrderEntity> orders = new ConcurrentDictionary<string, OrderEntity>();

            public int Count { get { return orders.Count; } }

            public Order this[string id]
            {
                get
                {
                    OrderEntity entity;
                    if (!orders.TryGetValue(id, out entity))
                        return Null.Order;
                    return entity;
                }
            }

            public OrderEntity GetOrNull(string id)
            {
                OrderEntity entity;
                orders.TryGetValue(id, out entity);
                return entity;
            }

            public void Add(OrderEntity entity)
            {
                orders.TryAdd(entity.Id, entity);
            }

            public void Replace(OrderEntity entity, bool fireEvent)
            {
                var oldValue = this[entity.Id];
                if (oldValue != null && oldValue.Modified <= entity.Modified)
                    orders[entity.Id] = entity;
            }

            public void Remove(string orderId)
            {
                OrderEntity removed;
                orders.TryRemove(orderId, out removed);
            }

            public void FireOrderOpened(OrderOpenedEventArgs args)
            {
                Opened(args);
            }

            public void FireOrderModified(OrderModifiedEventArgs args)
            {
                Modified(args);
            }

            public void FireOrderClosed(OrderClosedEventArgs args)
            {
                Closed(args);
            }

            public void FireOrderCanceled(OrderCanceledEventArgs args)
            {
                Canceled(args);
            }

            public void FireOrderExpired(OrderExpiredEventArgs args)
            {
                Expired(args);
            }

            public void FireOrderFilled(OrderFilledEventArgs args)
            {
                Filled(args);
            }

            public event Action<OrderClosedEventArgs> Closed = delegate { };
            public event Action<OrderModifiedEventArgs> Modified = delegate { };
            public event Action<OrderOpenedEventArgs> Opened = delegate { };
            public event Action<OrderCanceledEventArgs> Canceled = delegate { };
            public event Action<OrderExpiredEventArgs> Expired = delegate { };
            public event Action<OrderFilledEventArgs> Filled = delegate { };

            public IEnumerator<Order> GetEnumerator()
            {
                return this.orders.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.orders.Values.GetEnumerator();
            }
        }
    }
}
