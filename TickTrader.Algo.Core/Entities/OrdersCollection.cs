using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class OrdersCollection : OrderList
    {
        private readonly ConcurrentDictionary<string, OrderAccessor> _orders = new ConcurrentDictionary<string, OrderAccessor>();
        private readonly PluginBuilder _builder;

        public int Count => _orders.Count;

        public Order this[string id] => !_orders.TryGetValue(id, out OrderAccessor entity) ? entity : Null.Order;

        public IEnumerable<OrderAccessor> Values => _orders.Values;

        internal OrdersCollection(PluginBuilder builder)
        {
            _builder = builder;
        }

        internal OrderAccessor Add(OrderAccessor order)
        {
            if (!_orders.TryAdd(order.Info.Id, order))
                throw new ArgumentException("Order #" + order.Info.Id + " already exist!");

            Added?.Invoke(order);
            AddedInfo?.Invoke(order.Info);

            return order;
        }

        public OrderAccessor Add(OrderInfo info) => Add(new OrderAccessor(_builder.Symbols.GetOrDefault(info.Symbol), info));

        public OrderAccessor Update(OrderInfo info)
        {
            if (_orders.TryGetValue(info.Id, out OrderAccessor order))
            {
                if (order.Info.Modified <= info.Modified)
                {
                    order.Info.Update(info);
                    Replaced?.Invoke(order);
                }
            }

            return order;
        }

        public OrderAccessor GetOrderOrNull(string id) => _orders.GetOrDefault(id);

        public OrderAccessor Remove(OrderInfo info, bool update = false)
        {
            _orders.TryRemove(info.Id, out var order);

            if (!update)
                order?.Info.Update(info); //temporary convert

            if (order != null)
            {
                Removed?.Invoke(order);
                RemovedInfo?.Invoke(info);
            }

            return order;
        }

        public void Clear() => _orders.Clear();

        public void FireOrderOpened(OrderOpenedEventArgs args) => _builder.InvokePluginMethod((b, p) => Opened?.Invoke(p), args);

        public void FireOrderModified(OrderModifiedEventArgs args) => _builder.InvokePluginMethod((b, p) => Modified?.Invoke(p), args);

        public void FireOrderSplitted(OrderSplittedEventArgs args) => _builder.InvokePluginMethod((b, p) => Splitted?.Invoke(p), args);

        public void FireOrderClosed(OrderClosedEventArgs args) => _builder.InvokePluginMethod((b, p) => Closed?.Invoke(p), args);

        public void FireOrderCanceled(OrderCanceledEventArgs args) => _builder.InvokePluginMethod((b, p) => Canceled?.Invoke(p), args);

        public void FireOrderExpired(OrderExpiredEventArgs args) => _builder.InvokePluginMethod((b, p) => Expired?.Invoke(p), args);

        public void FireOrderFilled(OrderFilledEventArgs args) => _builder.InvokePluginMethod((b, p) => Filled?.Invoke(p), args);

        public void FireOrderActivated(OrderActivatedEventArgs args) => _builder.InvokePluginMethod((b, p) => Activated?.Invoke(p), args);

        IEnumerator IEnumerable.GetEnumerator() => _orders.Values.GetEnumerator();

        IEnumerator<Order> IEnumerable<Order>.GetEnumerator() => _orders.Values.GetEnumerator();

        public event Action<Order> Added;
        public event Action<Order> Removed;
        public event Action<IOrderCalcInfo> AddedInfo;
        public event Action<IOrderCalcInfo> RemovedInfo;
        public event Action<Order> Replaced;
        public event Action<OrderOpenedEventArgs> Opened;
        public event Action<OrderCanceledEventArgs> Canceled;
        public event Action<OrderClosedEventArgs> Closed;
        public event Action<OrderModifiedEventArgs> Modified;
        public event Action<OrderFilledEventArgs> Filled;
        public event Action<OrderExpiredEventArgs> Expired;
        public event Action<OrderActivatedEventArgs> Activated;
        public event Action<OrderSplittedEventArgs> Splitted;
    }
}
