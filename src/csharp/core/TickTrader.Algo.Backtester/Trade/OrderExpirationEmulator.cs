using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal class OrderExpirationEmulator
    {
        private SortedDictionary<UtcTicks, LinkedList<OrderAccessor>> innerMap;

        public int Count { get; private set; }

        public OrderExpirationEmulator()
        {
            innerMap = new SortedDictionary<UtcTicks, LinkedList<OrderAccessor>>();
        }

        public bool AddOrder(OrderAccessor order)
        {
            if (order.Info.Type.IsPending() && order.Info.Expiration != null)
            {
                GetOrAddDateList(order.Info.Expiration.Value).AddLast(order);
                Count++;
                return true;
            }
            return false;
        }

        public void RemoveOrder(OrderAccessor order)
        {
            if (order.Info.Type.IsPending() && order.Info.Expiration != null)
            {
                LinkedList<OrderAccessor> list;
                if (innerMap.TryGetValue(order.Info.Expiration.Value, out list))
                {
                    if (list.Remove(order))
                        Count--;
                }
            }
        }

        private LinkedList<OrderAccessor> GetOrAddDateList(UtcTicks dt)
        {
            LinkedList<OrderAccessor> list;
            if (!innerMap.TryGetValue(dt, out list))
            {
                list = new LinkedList<OrderAccessor>();
                innerMap.Add(dt, list);
            }

            return list;
        }

        public List<OrderAccessor> GetExpiredOrders(DateTime refTime)
        {
            var refTimestamp = refTime.ToUtcTicks();
            List<OrderAccessor> expiredOrders = new List<OrderAccessor>();

            while (innerMap.Count > 0)
            {
                KeyValuePair<UtcTicks, LinkedList<OrderAccessor>> list = innerMap.First();
                if (list.Key <= refTimestamp)
                {
                    expiredOrders.AddRange(list.Value);
                    if (innerMap.Remove(list.Key))
                        Count--;
                }
                else
                    break;
            }

            return expiredOrders;
        }

        public bool FindOrder(OrderAccessor order)
        {
            if (order.Info.Expiration != null)
            {
                LinkedList<OrderAccessor> list;
                if (innerMap.TryGetValue(order.Info.Expiration.Value, out list))
                {
                    return list.Contains(order);
                }
            }

            return false;
        }
    }
}
