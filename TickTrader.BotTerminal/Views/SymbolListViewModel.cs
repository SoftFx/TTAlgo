using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class SymbolListViewModel : PropertyChangedBase
    {
        private SymbolCollectionModel model;
        private SymbolViewModel selected;

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
                    var toRemove = Symbols.FirstOrDefault(s => s.SymbolName == m.Name);
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

        public SymbolViewModel SelectedSymbol
        {
            get { return selected; }
            set
            {
                if (selected != null)
                    selected.IsSelected = false;
                selected = value;
                NotifyOfPropertyChange("SelectedSymbols");
                if (selected != null)
                    selected.IsSelected = true;
            }
        }
    }
}
