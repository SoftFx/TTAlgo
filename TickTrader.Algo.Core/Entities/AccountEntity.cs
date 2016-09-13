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
        private PluginBuilder builder;

        public AccountEntity(PluginBuilder builder)
        {
            this.builder = builder;

            Orders = new OrdersCollection(builder);
            Assets = new AssetsCollection(builder);
        }

        public OrdersCollection Orders { get; private set; }
        public AssetsCollection Assets { get; private set; }

        public string Id { get; set; }
        public double Balance { get; set; }
        public string BalanceCurrency { get; set; }
        public AccountTypes Type { get; set; }

        internal void FireBalanceUpdateEvent()
        {
            builder.InvokePluginMethod(() => BalanceUpdated());
        }

        OrderList AccountDataProvider.Orders { get { return Orders.OrderListImpl; } }
        AssetList AccountDataProvider.Assets { get { return Assets.AssetListImpl; } }

        public double Equity
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

        public event Action BalanceUpdated = delegate { };
    }
}
