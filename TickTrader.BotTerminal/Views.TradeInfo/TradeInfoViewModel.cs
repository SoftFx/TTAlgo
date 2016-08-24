using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class TradeInfoViewModel: PropertyChangedBase
    {
        private AccountModel _accountModel;

        public TradeInfoViewModel(OrderListViewModel ordersVM, PositionListViewModel positionsVM, AssetsViewModel assetsVM)
        {
            Orders = ordersVM;
            Positions = positionsVM;
            Assets = assetsVM;
        }

        public OrderListViewModel Orders { get; }
        public AssetsViewModel Assets { get; }
        public PositionListViewModel Positions { get; }
    }
}
