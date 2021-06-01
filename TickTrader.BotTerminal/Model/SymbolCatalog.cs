using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Account.FeedStorage;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class SymbolCatalog
    {
        private CustomFeedStorage.Handler _customStorage;

        public SymbolCatalog(CustomFeedStorage.Handler customStorage, TraderClientModel client)
        {
            _customStorage = customStorage;

            OnlineSymbols = client.Symbols.Select((k, s) => CreateSymbolData(s, client));

            CustomSymbols = customStorage.Symbols.Select<SymbolKey, CustomSymbol, SymbolData>(
                (k, s) => new CustomSymbolData(s, customStorage));

            var sortedOnlineList = OnlineSymbols.OrderBy((k, v) => v.Name);
            var sortedCustomList = CustomSymbols.OrderBy((k, v) => v.Name);

            ObservableOnlineSymbols = sortedOnlineList.AsObservable();
            ObservableCustomSymbols = sortedCustomList.AsObservable();

            AllSymbols = VarCollection.Combine(OnlineSymbols, CustomSymbols);

            var sortedList = AllSymbols.OrderBy((k, v) => k, new SymbolKeyComparer());

            ObservableSymbols = sortedList.AsObservable();

            AllSymbols.Updated += a =>
            {
                if (a.OldItem != null)
                    a.OldItem.OnRemoved();
            };
        }

        public IVarSet<SymbolKey, SymbolData> AllSymbols { get; }
        public IVarSet<SymbolKey, SymbolData> OnlineSymbols { get; }
        public IVarSet<SymbolKey, SymbolData> CustomSymbols { get; }

        public IObservableList<SymbolData> ObservableSymbols { get; }
        public IObservableList<SymbolData> ObservableOnlineSymbols { get; }
        public IObservableList<SymbolData> ObservableCustomSymbols { get; }

        public Task AddCustomSymbol(CustomSymbol customSymbol)
        {
            return _customStorage.Add(customSymbol);
        }

        internal Task Update(CustomSymbol customSymbol)
        {
            return _customStorage.Update(customSymbol);
        }

        public SymbolData GetSymbol(ISetupSymbolInfo info)
        {
            var key = new SymbolKey(info.Name, info.Origin);

            if (info.Origin == SymbolConfig.Types.SymbolOrigin.Online)
                return OnlineSymbols.Snapshot[key];
            else if (info.Origin == SymbolConfig.Types.SymbolOrigin.Custom)
                return CustomSymbols.Snapshot[key];

            throw new Exception("Unsupported symbol origin: " + info.Origin);
        }

        private KeyValuePair<SymbolKey, SymbolData> CreateSymbolData(SymbolInfo smb, TraderClientModel client)
        {
            var key = new SymbolKey(smb.Name, SymbolConfig.Types.SymbolOrigin.Online);
            var data = new OnlineSymbolData(smb, client);
            return new KeyValuePair<SymbolKey, SymbolData>(key, data);

        }
    }
}
