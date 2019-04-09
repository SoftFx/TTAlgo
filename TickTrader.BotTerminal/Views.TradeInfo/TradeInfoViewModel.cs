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
        public TradeInfoViewModel(TraderClientModel clientModel, ConnectionManager cManager, ProfileManager _profileStorageModel = null)
            : this(clientModel.Account, clientModel.Symbols, clientModel.Currencies, clientModel, _profileStorageModel)
        {
        }

        public TradeInfoViewModel(AccountModel accModel, IVarSet<string, SymbolModel> symbols,
            IVarSet<string, CurrencyEntity> currencies, IConnectionStatusInfo connectionInfo, ProfileManager _profileStorageModel = null)
        {
            var netPositions = new NetPositionListViewModel(accModel, symbols, connectionInfo, _profileStorageModel);
            var grossPositions = new GrossPositionListViewModel(accModel, symbols, connectionInfo);
            Positions = new PositionListViewModel(netPositions, grossPositions);
            Orders = new OrderListViewModel(accModel, symbols, connectionInfo, _profileStorageModel);
            Assets = new AssetsViewModel(accModel, currencies.Snapshot, connectionInfo);
            AccountStats = new AccountStatsViewModel(accModel, connectionInfo);
        }

        public OrderListViewModel Orders { get; }
        public AssetsViewModel Assets { get; }
        public PositionListViewModel Positions { get; }
        public AccountStatsViewModel AccountStats { get; }
    }

    internal class ProviderColumnsState : ISettings
    {
        private List<ColumnStateStorageEntry> _source;
        private Dictionary<string, bool> _dict;
        private string _prefix;

        public object this[string key]
        {
            get => _dict[$"{key}{_prefix}"];
            set
            {
                var fullKey = $"{key}{_prefix}";
                _dict[fullKey] = (bool)value;
                var item = _source.FirstOrDefault(i => i.Key == fullKey);

                if (item == null)
                    _source.Add(new ColumnStateStorageEntry() { Key = fullKey, State = (bool)value, });
                else
                    item.State = (bool)value;
            }
        }

        public ProviderColumnsState(List<ColumnStateStorageEntry> source, string prefix = "")
        {
            _source = source;
            _prefix = prefix;

            _dict = _source?.ToDictionary(i => i.Key, i => i.State);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey($"{key}{_prefix}");
        }
    }
}
