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
        private SymbolCollectionModel model;

        public SymbolListViewModel(SymbolCollectionModel symbolCollection)
        {
            this.model = symbolCollection;
            this.Symbols = new ObservableCollection<SymbolViewModel>();

            foreach (var symbol in model)
            {
                Symbols.Add(new SymbolViewModel(symbol));
            }

            model.Added += m =>
                {
                    var symbolViewModel = new SymbolViewModel(m);
                    symbolViewModel.NewChartRequested += symbolViewModel_NewChartRequested;
                    Symbols.Add(symbolViewModel);
                };
            model.Removed += m =>
                {
                    var toRemove = Symbols.FirstOrDefault(s => s.Name == m.Name);
                    toRemove.NewChartRequested -= symbolViewModel_NewChartRequested;
                    if (toRemove != null)
                        Symbols.Remove(toRemove);
                };
        }

        void symbolViewModel_NewChartRequested(string symbol)
        {
            NewChartRequested(symbol);
        }

        public event Action<string> NewChartRequested = delegate { };

        public ObservableCollection<SymbolViewModel> Symbols { get; private set; }
    }
}
