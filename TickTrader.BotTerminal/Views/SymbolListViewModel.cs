using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class SymbolListViewModel
    {
        public SymbolListViewModel()
        {
            Symbols = new ObservableCollection<SymbolViewModel>();
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURUSD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURCAD", "Forex"));
            Symbols.Add(new SymbolViewModel("EURJPY", "Metals"));
            Symbols.Add(new SymbolViewModel("EURJPY", "Metals"));
            Symbols.Add(new SymbolViewModel("EURJPY", "Metals"));
            Symbols.Add(new SymbolViewModel("EURJPY", "Metals"));
            Symbols.Add(new SymbolViewModel("EURJPY", "Metals"));
            Symbols.Add(new SymbolViewModel("EURJPY", "Metals"));
            Symbols.Add(new SymbolViewModel("EURJPY", "Metals"));
        }

        public ObservableCollection<SymbolViewModel> Symbols { get; private set; }
    }
}
