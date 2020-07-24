using Machinarium.Qnil;
using Machinarium.Var;
using System;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Infrastructure;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class SymbolListViewModel : EntityBase
    {
        private readonly IShell _shell;

        public SymbolListViewModel(IVarSet<string, SymbolInfo> symbolCollection, QuoteDistributor distributor, IShell shell)
        {
            _shell = shell;

            Symbols = symbolCollection.Select((k, v) => new SymbolViewModel(v, distributor)).OrderBy((k, v) => k).AsObservable();
        }

        public IObservableList<SymbolViewModel> Symbols { get; }

        public void OpenChart(object o)
        {
            if (o is SymbolViewModel model)
                _shell?.OpenChart(model?.SymbolName);
        }
    }
}
