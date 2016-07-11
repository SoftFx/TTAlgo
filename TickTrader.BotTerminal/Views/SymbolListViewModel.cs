using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class SymbolListViewModel : PropertyChangedBase
    {
        private SymbolViewModel selected;
        private IDynamicListSource<SymbolViewModel> viewModelCollection;

        public SymbolListViewModel(SymbolCollectionModel symbolCollection, OrderUi orderUi)
        {
            viewModelCollection = symbolCollection.Select((k, v) => new SymbolViewModel(v, orderUi)).OrderBy((k, v) => k);

            Symbols = viewModelCollection.AsObservable();

            viewModelCollection.Updated += args =>
            {
                if(args.Action == DLinqAction.Remove || args.Action == DLinqAction.Replace)
                    args.OldItem.NewChartRequested -= symbolViewModel_NewChartRequested;

                if (args.Action == DLinqAction.Insert || args.Action == DLinqAction.Replace)
                    args.NewItem.NewChartRequested += symbolViewModel_NewChartRequested;
            };
        }

        void symbolViewModel_NewChartRequested(string symbol)
        {
            NewChartRequested(symbol);
        }

        public event Action<string> NewChartRequested = delegate { };

        public IObservableListSource<SymbolViewModel> Symbols { get; private set; }

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
