using System;
using System.Collections;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal sealed class OrderFilteredCollection : OrderList
    {
        private readonly Dictionary<string, Order> _filteredOrders = new Dictionary<string, Order>();
        private readonly OrdersCollection _originalList;
        private readonly Predicate<Order> _filter;

        public Order this[string id] => _filteredOrders.TryGetValue(id, out Order entity) && ApplyFilter(entity) ? entity : Null.Order;

        public int Count => _filteredOrders.Count;

        public event Action<OrderCanceledEventArgs> Canceled;
        public event Action<OrderClosedEventArgs> Closed;
        public event Action<OrderExpiredEventArgs> Expired;
        public event Action<OrderFilledEventArgs> Filled;
        public event Action<OrderModifiedEventArgs> Modified;
        public event Action<OrderOpenedEventArgs> Opened;
        public event Action<OrderSplittedEventArgs> Splitted;
        public event Action<Order> Added { add { } remove { } }
        public event Action<Order> Removed { add { } remove { } }
        public event Action<Order> Replaced { add { } remove { } }
        public event Action<OrderActivatedEventArgs> Activated;

        public OrderFilteredCollection(OrdersCollection originalList, Predicate<Order> predicate)
        {
            _originalList = originalList;
            _filter = predicate;

            foreach (var order in originalList)
            {
                if (predicate(order))
                    _filteredOrders.Add(order.Id, order);
            }

            _originalList.Added += (args) => FilterOrderEvent(args, AddOrder);
            _originalList.Removed += (args) => FilterOrderEvent(args, RemoveOrder);

            _originalList.Opened += (args) => FilterSingleOrderEvent(args, Opened);
            _originalList.Closed += (args) => FilterSingleOrderEvent(args, Closed);
            _originalList.Expired += (args) => FilterSingleOrderEvent(args, Expired);
            _originalList.Activated += (args) => FilterSingleOrderEvent(args, Activated);
            _originalList.Canceled += (args) => FilterSingleOrderEvent(args, Canceled);

            _originalList.Filled += (args) => FilterDoubleOrderEvent(args, Filled);
            _originalList.Modified += (args) => FilterDoubleOrderEvent(args, Modified);
            _originalList.Splitted += (args) => FilterDoubleOrderEvent(args, Splitted);
        }

        public IEnumerator<Order> GetEnumerator() =>_filteredOrders.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private bool ApplyFilter(Order order) => _filter == null || _filter(order);

        private void FilterOrderEvent<T>(T order, Action<T> action) where T : Order
        {
            if (ApplyFilter(order))
                action?.Invoke(order);
        }

        private void FilterSingleOrderEvent<T>(T args, Action<T> action) where T : SingleOrderEventArgs
        {
            if (ApplyFilter(args.Order))
                action?.Invoke(args);
        }

        private void FilterDoubleOrderEvent<T>(T args, Action<T> action) where T : DoubleOrderEventArgs
        {
            if (ApplyFilter(args.OldOrder))
                action?.Invoke(args);
        }

        private void AddOrder(Order order)
        {
            if (!_filteredOrders.ContainsKey(order.Id))
                _filteredOrders.Add(order.Id, order);
        }

        private void RemoveOrder(Order order) => _filteredOrders.Remove(order.Id);
    }
}
