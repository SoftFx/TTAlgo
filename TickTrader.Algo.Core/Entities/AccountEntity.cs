using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class AccountEntity : AccountDataProvider
    {
        public AccountEntity()
        {
            Orders = new OrdersCollection();
        }

        public OrdersCollection Orders { get; private set; }
        public double Balance { get; set; }

        OrderList AccountDataProvider.Orders { get { return Orders.OrderListImpl; } }
        PositionList AccountDataProvider.Positions { get { throw new NotImplementedException(); } }
    }
}
