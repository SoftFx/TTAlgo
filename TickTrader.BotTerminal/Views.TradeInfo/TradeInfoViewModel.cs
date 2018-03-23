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
        public static string ClassName { get { return "TradeInfoViewModel"; } }

        public TradeInfoViewModel(TraderClientModel clientModel)
        {
            var netPositions = new NetPositionListViewModel(clientModel.Account, clientModel.Symbols, clientModel.Connection);
            var grossPositions = new GrossPositionListViewModel(clientModel.Account, clientModel.Symbols, clientModel.Connection);
            Positions = new PositionListViewModel(netPositions, grossPositions);
            Orders = new OrderListViewModel(clientModel.Account, clientModel.Symbols, clientModel.Connection);
            Assets = new AssetsViewModel(clientModel.Account, clientModel.Currencies.Snapshot, clientModel.Connection);
            AccountStats = new AccountStatsViewModel(clientModel);
        }

        public OrderListViewModel Orders { get; }
        public AssetsViewModel Assets { get; }
        public PositionListViewModel Positions { get; }
        public AccountStatsViewModel AccountStats { get; }
    }
}
