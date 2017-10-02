using System;
using System.Collections;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class OrderFilteredCollection : OrderList
    {
        private Dictionary<string, Order> _filteredOrders = new Dictionary<string, Order>();
        private OrderList _originalList;
        private Predicate<Order> _predicate;


        public Order this[string id]
        {
            get
            {
                Order entity;
                if (!_filteredOrders.TryGetValue(id, out entity))
                    return Null.Order;
                return entity;
            }
        }

        public int Count => _filteredOrders.Count;


        public event Action<OrderCanceledEventArgs> Canceled;
        public event Action<OrderClosedEventArgs> Closed;
        public event Action<OrderExpiredEventArgs> Expired;
        public event Action<OrderFilledEventArgs> Filled;
        public event Action<OrderModifiedEventArgs> Modified;
        public event Action<OrderOpenedEventArgs> Opened;
        public event Action<Order> Added;
        public event Action<Order> Removed;
        public event Action<Order> Replaced;
        public event Action<OrderActivatedEventArgs> Activated;


        public OrderFilteredCollection(OrderList originalList, Predicate<Order> predicate)
        {
            _originalList = originalList;
            _predicate = predicate;

            foreach (var order in originalList)
            {
                if (predicate(order))
                    _filteredOrders.Add(order.Id, order);
            }

            _originalList.Added += OriginalList_Added;
            _originalList.Removed += OriginalList_Removed;
            _originalList.Canceled += OriginalList_Canceled;
            _originalList.Closed += OriginalList_Closed;
            _originalList.Expired += OriginalList_Expired;
            _originalList.Filled += OriginalList_Filled;
            _originalList.Modified += OriginalList_Modified;
            _originalList.Opened += OriginalList_Opened;
            _originalList.Activated += OriginalList_Activated;
        }


        public IEnumerator<Order> GetEnumerator()
        {
            return _filteredOrders.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private void OriginalList_Added(Order order)
        {
            if (_predicate(order))
            {
                if (!_filteredOrders.ContainsKey(order.Id))
                    _filteredOrders.Add(order.Id, order);
            }
        }

        private void OriginalList_Removed(Order order)
        {
            if (_predicate(order))
            {
                _filteredOrders.Remove(order.Id);
            }
        }

        private void OriginalList_Canceled(OrderCanceledEventArgs args)
        {
            if (_predicate(args.Order))
                Canceled?.Invoke(args);
        }

        private void OriginalList_Opened(OrderOpenedEventArgs args)
        {
            if (_predicate(args.Order))
                Opened?.Invoke(args);
        }

        private void OriginalList_Modified(OrderModifiedEventArgs args)
        {
            if (_predicate(args.OldOrder))
                Modified?.Invoke(args);
        }

        private void OriginalList_Closed(OrderClosedEventArgs args)
        {
            if (_predicate(args.Order))
                Closed?.Invoke(args);
        }

        private void OriginalList_Expired(OrderExpiredEventArgs args)
        {
            if (_predicate(args.Order))
                Expired?.Invoke(args);
        }

        private void OriginalList_Filled(OrderFilledEventArgs args)
        {
            if (_predicate(args.OldOrder))
                Filled?.Invoke(args);
        }

        private void OriginalList_Activated(OrderActivatedEventArgs args)
        {
            if (_predicate(args.Order))
                Activated?.Invoke(args);
        }
    }
}
