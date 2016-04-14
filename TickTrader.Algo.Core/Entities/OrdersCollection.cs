using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class OrdersCollection
    {
        private OrdersFixture fixture = new OrdersFixture();

        internal OrderList OrderListImpl { get { return fixture; } }
        internal bool IsEnabled { get { return true; } }

        public void Add(OrderEntity entity)
        {
            fixture.Add(entity, IsEnabled);
        }

        public void Replace(OrderEntity entity)
        {
            fixture.Replace(entity, IsEnabled);
        }

        public void Remove(long orderId)
        {
            fixture.Remove(orderId, IsEnabled);
        }

        internal class OrdersFixture : OrderList
        {
            private Dictionary<long, OrderEntity> orders = new Dictionary<long, OrderEntity>();

            public Order this[long id]
            {
                get
                {
                    OrderEntity entity;
                    if (!orders.TryGetValue(id, out entity))
                        return null;
                    return entity;
                }
            }

            public void Add(OrderEntity entity, bool fireEvent)
            {
                orders.Add(entity.Id, entity);
                Opened(new OrderOpenedEventArgsImpl(entity));
            }

            public void Replace(OrderEntity entity, bool fireEvent)
            {
                var oldOrder = orders[entity.Id];
                orders[entity.Id] = entity;
                Modified(new OrderModifiedEventArgsImpl(entity, oldOrder));
            }

            public void Remove(long orderId, bool fireEvent)
            {
                var removedOrder = orders[orderId];
                Closed(new OrderClosedEventArgsImpl(removedOrder));
            }

            public event Action<OrderClosedEventArgs> Closed = delegate { };
            public event Action<OrderModifiedEventArgs> Modified = delegate { };
            public event Action<OrderOpenedEventArgs> Opened = delegate { };

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
