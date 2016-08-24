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

        public double BalanceCurrency
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public double Equity
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AccountTypes Type
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NetPositionList NetPositions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public AssetList Assets
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
