using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    internal class OrderExpirationEmulator
    {
        private SortedDictionary<DateTime, LinkedList<OrderAccessor>> innerMap;

        public int Count { get; private set; }

        public OrderExpirationEmulator()
        {
            innerMap = new SortedDictionary<DateTime, LinkedList<OrderAccessor>>();
        }

        public bool AddOrder(OrderAccessor order)
        {
            if (order.IsPending && order.Entity.Expiration != null)
            {
                GetOrAddDateList(order.Entity.Expiration.Value).AddLast(order);
                Count++;
                return true;
            }
            return false;
        }

        public void RemoveOrder(OrderAccessor order)
        {
            if (order.IsPending && order.Entity.Expiration != null)
            {
                LinkedList<OrderAccessor> list;
                if (innerMap.TryGetValue(order.Entity.Expiration.Value, out list))
                {
                    if (list.Remove(order))
                        Count--;
                }
            }
        }

        private LinkedList<OrderAccessor> GetOrAddDateList(DateTime dt)
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
            List<OrderAccessor> expiredOrders = new List<OrderAccessor>();

            while (innerMap.Count > 0)
            {
                KeyValuePair<DateTime, LinkedList<OrderAccessor>> list = innerMap.First();
                if (list.Key <= refTime)
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
            if (order.Entity.Expiration != null)
            {
                LinkedList<OrderAccessor> list;
                if (innerMap.TryGetValue(order.Entity.Expiration.Value, out list))
                {
                    return list.Contains(order);
                }
            }

            return false;
        }
    }
}
