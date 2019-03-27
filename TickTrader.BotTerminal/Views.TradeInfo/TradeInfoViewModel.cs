using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    class TradeInfoViewModel: PropertyChangedBase
    {
        public TradeInfoViewModel(TraderClientModel clientModel, ConnectionManager cManager)
            : this(clientModel.Account, clientModel.Symbols, clientModel.Currencies, clientModel, true)
        {
        }

        public TradeInfoViewModel(AccountModel accModel, IVarSet<string, SymbolModel> symbols,
            IVarSet<string, CurrencyEntity> currencies, IConnectionStatusInfo connectionInfo, bool autoSizeColumns)
        {
            var netPositions = new NetPositionListViewModel(accModel, symbols, connectionInfo);
            var grossPositions = new GrossPositionListViewModel(accModel, symbols, connectionInfo);
            Positions = new PositionListViewModel(netPositions, grossPositions);
            Orders = new OrderListViewModel(accModel, symbols, connectionInfo);
            Assets = new AssetsViewModel(accModel, currencies.Snapshot, connectionInfo);
            AccountStats = new AccountStatsViewModel(accModel, connectionInfo);

            Orders.AutoSizeColumns = autoSizeColumns;
        }

        public OrderListViewModel Orders { get; }
        public AssetsViewModel Assets { get; }
        public PositionListViewModel Positions { get; }
        public AccountStatsViewModel AccountStats { get; }
    }
}
