using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal class SymbolCatalog
    {
        private CustomFeedStorage.Handler _customStorage;

        public SymbolCatalog(CustomFeedStorage.Handler customStorage, TraderClientModel client)
        {
            _customStorage = customStorage;

            OnlineSymbols = client.Symbols.Select<string, SymbolModel, SymbolData>(
                (k, s) => new OnlineSymbolData(s, client));

            CustomSymbols = customStorage.Symbols.Select<string, CustomSymbol, SymbolData>(
                (k, s) => new CustomSymbolData(s, customStorage));

            var sortedOnlineList = OnlineSymbols.OrderBy((k, v) => v.Name);
            var sortedCustomList = CustomSymbols.OrderBy((k, v) => v.Name);

            ObservableOnlineSymbols = sortedOnlineList.AsObservable();
            ObservableCustomSymbols = sortedCustomList.AsObservable();

            AllSymbols = VarCollection.Combine(sortedOnlineList, sortedCustomList);
            ObservableSymbols = AllSymbols.AsObservable();

            AllSymbols.Updated += a =>
            {
                if (a.OldItem != null)
                    a.OldItem.OnRemoved();
            };
        }

        public IVarList<SymbolData> AllSymbols { get; }
        public IVarSet<string, SymbolData> OnlineSymbols { get; }
        public IVarSet<string, SymbolData> CustomSymbols { get; }

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
    }
}
