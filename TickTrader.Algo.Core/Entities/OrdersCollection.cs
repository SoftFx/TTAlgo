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
    public class OrdersCollection : IEnumerable<OrderAccessor>
    {
        private PluginBuilder builder;
        private OrdersFixture fixture;

        internal OrderList OrderListImpl { get { return fixture; } }
        internal bool IsEnabled { get { return true; } }

        internal OrdersCollection(PluginBuilder builder)
        {
            this.builder = builder;
            fixture = new OrdersFixture(builder.Symbols);
        }

        internal void Add(OrderAccessor order)
        {
            fixture.Add(order);
            Added?.Invoke(order);
        }

        public OrderAccessor Add(OrderEntity entity)
        {
            var result = fixture.Add(entity);
            Added?.Invoke(result);
            return result;
        }

        public OrderAccessor Replace(OrderEntity entity)
        {
            var order = fixture.Replace(entity, IsEnabled);
            if (order != null)
                Replaced?.Invoke(order);
            return order;
        }

        public OrderAccessor GetOrderOrThrow(string id)
        {
            var order = fixture.GetOrNull(id);
            if (order == null)
                throw new OrderValidationError("Order Not Found " + id, OrderCmdResultCodes.OrderNotFound);
            return order;
        }

        public OrderAccessor GetOrderOrNull(string id)
        {
            return fixture.GetOrNull(id);
        }

        public OrderAccessor Remove(string orderId)
        {
            var order = fixture.Remove(orderId);
            if (order != null)
                Removed?.Invoke(order);
            return order;
        }

        public OrderAccessor UpdateAndRemove(OrderEntity entity)
        {
            var order = fixture.Remove(entity.Id);
            order?.Update(entity);
            if (order != null)
                Removed?.Invoke(order);
            return order;
        }

        public void Clear()
        {
            fixture.Clear();
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

        public void FireOrderActivated(OrderActivatedEventArgs args)
        {
            builder.InvokePluginMethod(() => fixture.FireOrderActivated(args));
        }

        public IEnumerator<OrderAccessor> GetEnumerator()
        {
            return ((IEnumerable<OrderAccessor>)OrderListImpl).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public event Action<OrderAccessor> Added;
        public event Action<OrderAccessor> Removed;
        public event Action<OrderAccessor> Replaced;

        internal class OrdersFixture : OrderList, IEnumerable<OrderAccessor>
        {
            private ConcurrentDictionary<string, OrderAccessor> orders = new ConcurrentDictionary<string, OrderAccessor>();
            private SymbolsCollection _symbols;

            internal OrdersFixture(SymbolsCollection symbols)
            {
                _symbols = symbols;
            }

            public int Count { get { return orders.Count; } }

            public Order this[string id]
            {
                get
                {
                    OrderAccessor entity;
                    if (!orders.TryGetValue(id, out entity))
                        return Null.Order;
                    return entity;
                }
            }

            public void Add(OrderAccessor order)
            {
                if (!orders.TryAdd(order.Id, order))
                    throw new ArgumentException("Order #" + order.Id + " already exist!");

                Added?.Invoke(order);
            }

            public OrderAccessor Add(OrderEntity entity)
            {
                var accessor = new OrderAccessor(entity, _symbols.GetOrDefault);
                if (!orders.TryAdd(entity.Id, accessor))
                    throw new ArgumentException("Order #" + entity.Id + " already exist!");

                Added?.Invoke(accessor);

                return accessor;
            }

            public OrderAccessor Replace(OrderEntity entity, bool fireEvent)
            {
                OrderAccessor order;

                if (this.orders.TryGetValue(entity.Id, out order))
                {
                    if (order.Modified <= entity.Modified)
                    {
                        order.Update(entity);
                        Replaced?.Invoke(order);
                    }
                }

                return order;
            }

            public OrderAccessor Remove(string orderId)
            {
                OrderAccessor removed;

                if (orders.TryRemove(orderId, out removed))
                    Removed?.Invoke(removed);

                return removed;
            }

            public OrderAccessor GetOrNull(string orderId)
            {
                OrderAccessor entity;
                orders.TryGetValue(orderId, out entity);
                return entity;
            }

            public void Clear()
            {
                orders.Clear();
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

            public void FireOrderActivated(OrderActivatedEventArgs args)
            {
                Activated(args);
            }

            public event Action<OrderClosedEventArgs> Closed = delegate { };
            public event Action<OrderModifiedEventArgs> Modified = delegate { };
            public event Action<OrderOpenedEventArgs> Opened = delegate { };
            public event Action<OrderCanceledEventArgs> Canceled = delegate { };
            public event Action<OrderExpiredEventArgs> Expired = delegate { };
            public event Action<OrderFilledEventArgs> Filled = delegate { };
            public event Action<OrderActivatedEventArgs> Activated = delegate { };
            public event Action<Order> Added;
            public event Action<Order> Removed;
            public event Action<Order> Replaced;

            public IEnumerator<OrderAccessor> GetTypedEnumerator()
            {
                return orders.Values.GetEnumerator();
            }

            public IEnumerator<Order> GetEnumerator()
            {
                return orders.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return orders.Values.GetEnumerator();
            }

            IEnumerator<OrderAccessor> IEnumerable<OrderAccessor>.GetEnumerator()
            {
                return orders.Values.GetEnumerator();
            }
        }      
    }
}
