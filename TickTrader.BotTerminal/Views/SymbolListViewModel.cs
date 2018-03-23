using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machinarium.Qnil;
using Machinarium.Var;

namespace TickTrader.BotTerminal
{
    internal class SymbolListViewModel : EntityBase
    {
        public static string ClassName { get { return "SymbolListViewModel"; } }

        private IDynamicListSource<SymbolViewModel> viewModelCollection;

        public SymbolListViewModel(SymbolCollectionModel symbolCollection, IShell shell)
        {
            viewModelCollection = symbolCollection.Select((k, v) => new SymbolViewModel((SymbolModel)v, shell)).OrderBy((k, v) => k);

            Symbols = viewModelCollection.AsObservable();
            SelectedSymbol = AddProperty<SymbolViewModel>();

            TriggerOnChange(SelectedSymbol.Var, a =>
            {
                if (a.Old != null) a.Old.IsSelected = false;
                if (a.New != null) a.New.IsSelected = true;
            });
        }

        public IObservableListSource<SymbolViewModel> Symbols { get; }
        public Property<SymbolViewModel> SelectedSymbol { get; }
    }
}
