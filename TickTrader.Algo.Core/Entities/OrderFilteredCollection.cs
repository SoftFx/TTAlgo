using System;
using System.Collections;
using System.Collections.Generic;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class OrderFilteredCollection : OrderList
    {
        private Dictionary<string, Order> filteredOrders = new Dictionary<string, Order>();
        private OrderList originalList;
        private Predicate<Order> predicate;

        public OrderFilteredCollection(OrderList originalList, Predicate<Order> predicate)
        {
            this.originalList = originalList;
            this.predicate = predicate;

            foreach (var order in originalList)
            {
                if (predicate(order))
                    filteredOrders.Add(order.Id, order);
            }

            originalList.Canceled += OriginalList_Canceled;
            originalList.Closed += OriginalList_Closed;
            originalList.Expired += OriginalList_Expired;
            originalList.Filled += OriginalList_Filled;
            originalList.Modified += OriginalList_Modified;
            originalList.Opened += OriginalList_Opened;
        }

        public Order this[string id]
        {
            get
            {
                Order entity;
                if (!filteredOrders.TryGetValue(id, out entity))
                    return Null.Order;
                return entity;
            }
        }

        public int Count { get { return filteredOrders.Count; } }

        public event Action<OrderCanceledEventArgs> Canceled;
        public event Action<OrderClosedEventArgs> Closed;
        public event Action<OrderExpiredEventArgs> Expired;
        public event Action<OrderFilledEventArgs> Filled;
        public event Action<OrderModifiedEventArgs> Modified;
        public event Action<OrderOpenedEventArgs> Opened;

        private void OriginalList_Canceled(OrderCanceledEventArgs args)
        {
            if (filteredOrders.ContainsKey(args.Order.Id))
                Canceled?.Invoke(args);
        }

        private void OriginalList_Opened(OrderOpenedEventArgs args)
        {
            if (predicate(args.Order))
                Opened?.Invoke(args);
        }

        private void OriginalList_Modified(OrderModifiedEventArgs args)
        {
            if (filteredOrders.ContainsKey(args.OldOrder.Id))
                Modified?.Invoke(args);
        }

        private void OriginalList_Closed(OrderClosedEventArgs args)
        {
            if (filteredOrders.ContainsKey(args.Order.Id))
                Closed?.Invoke(args);
        }

        private void OriginalList_Expired(OrderExpiredEventArgs args)
        {
            if (filteredOrders.ContainsKey(args.Order.Id))
                Expired?.Invoke(args);
        }

        private void OriginalList_Filled(OrderFilledEventArgs args)
        {
            if (filteredOrders.ContainsKey(args.OldOrder.Id))
                Filled?.Invoke(args);
        }

        public IEnumerator<Order> GetEnumerator()
        {
            return filteredOrders.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
