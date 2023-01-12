using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    internal sealed class OrderExpirationEmulator
    {
        private readonly SortedDictionary<UtcTicks, LinkedList<OrderAccessor>> _innerMap;

        public int Count => _innerMap.Count;


        public OrderExpirationEmulator()
        {
            _innerMap = new SortedDictionary<UtcTicks, LinkedList<OrderAccessor>>();
        }


        public bool AddOrder(OrderAccessor order)
        {
            if (order.Info.Type.IsPending() && order.Info.Expiration != null)
            {
                GetOrAddDateList(order.Info.Expiration.Value).AddLast(order);
                return true;
            }

            return false;
        }


        public void RemoveOrder(OrderAccessor order)
        {
            if (order.Info.Type.IsPending() && order.Info.Expiration != null)
            {
                var time = order.Info.Expiration.Value;

                if (_innerMap.TryGetValue(time, out LinkedList<OrderAccessor> list))
                {
                    if (list.Remove(order) && list.Count == 0)
                        _innerMap.Remove(time);
                }
            }
        }

        public List<OrderAccessor> GetExpiredOrders(DateTime refTime)
        {
            var refTimestamp = refTime.ToUtcTicks();
            var expiredOrders = new List<OrderAccessor>();

            while (_innerMap.Count > 0)
            {
                var list = _innerMap.First();

                if (list.Key <= refTimestamp)
                {
                    expiredOrders.AddRange(list.Value);

                    _innerMap.Remove(list.Key);
                }
                else
                    break;
            }

            return expiredOrders;
        }

        private LinkedList<OrderAccessor> GetOrAddDateList(UtcTicks dt)
        {
            if (!_innerMap.TryGetValue(dt, out LinkedList<OrderAccessor> list))
            {
                list = new LinkedList<OrderAccessor>();
                _innerMap.Add(dt, list);
            }

            return list;
        }
    }
}
