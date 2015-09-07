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

            model.Added += m => Symbols.Add(new SymbolViewModel(m));
            model.Removed += m =>
                {
                    var toRemove = Symbols.FirstOrDefault(s => s.Name == m.Name);
                    if (toRemove != null)
                        Symbols.Remove(toRemove);
                };
        }

        public ObservableCollection<SymbolViewModel> Symbols { get; private set; }
    }
}
