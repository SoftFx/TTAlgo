using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    class TradeInfoViewModel: PropertyChangedBase
    {
        public TradeInfoViewModel(TraderClientModel clientModel, ConnectionManager cManager, ProfileManager profile = null)
            : this(clientModel.Account, clientModel.Symbols, clientModel.Currencies, clientModel, true, profile)
        {
        }

        public TradeInfoViewModel(AccountModel accModel, IVarSet<string, SymbolInfo> symbols,
            IVarSet<string, CurrencyEntity> currencies, IConnectionStatusInfo connectionInfo, bool autoSizeColumns, ProfileManager profile = null, bool isBacktester = false)
        {
            var netPositions = new NetPositionListViewModel(accModel, connectionInfo, profile, isBacktester);
            var grossPositions = new GrossPositionListViewModel(accModel, symbols, connectionInfo, profile, isBacktester);
            Positions = new PositionListViewModel(netPositions, grossPositions);
            Orders = new OrderListViewModel(accModel, symbols, connectionInfo, profile, isBacktester);
            Assets = new AssetsViewModel(accModel, currencies.Snapshot, connectionInfo);
            AccountStats = new AccountStatsViewModel(accModel, connectionInfo);

            Orders.AutoSizeColumns = autoSizeColumns;
            Positions.Gross.AutoSizeColumns = autoSizeColumns;
            Positions.Net.AutoSizeColumns = autoSizeColumns;
        }

        public OrderListViewModel Orders { get; }
        public AssetsViewModel Assets { get; }
        public PositionListViewModel Positions { get; }
        public AccountStatsViewModel AccountStats { get; }
    }
}
