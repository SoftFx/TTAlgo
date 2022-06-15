using Machinarium.Qnil;
using Machinarium.Var;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class SymbolListViewModel : EntityBase
    {
        private readonly IShell _shell;


        public Property<SymbolViewModel> SelectedSymbol { get; }

        public IObservableList<SymbolViewModel> Symbols { get; }


        public SymbolListViewModel(IVarSet<string, SymbolInfo> symbolCollection, QuoteDistributor2 distributor, IShell shell)
        {
            _shell = shell;

            SelectedSymbol = AddProperty<SymbolViewModel>();
            Symbols = symbolCollection.Select((k, v) => new SymbolViewModel(v, distributor)).OrderBy((k, v) => k).AsObservable();
        }

        public void OpenNewChart() //using on UI
        {
            if (SelectedSymbol.HasValue)
                _shell?.OpenChart(SelectedSymbol?.Value?.SymbolName);
        }
    }
}
