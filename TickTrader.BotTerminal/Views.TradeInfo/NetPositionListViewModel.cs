using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    class NetPositionListViewModel : AccountBasedViewModel
    {
        private SymbolCollectionModel _symbols;

        public NetPositionListViewModel(AccountModel model, SymbolCollectionModel symbols)
            : base(model)
        {
            _symbols = symbols;

            Positions = model.Positions
                .OrderBy((id, p) => id)
                .Select(p => new PositionViewModel(p, (SymbolModel)_symbols[p.Symbol]))
                .AsObservable();

            Positions.CollectionChanged += PositionsCollectionChanged;
        }

        protected override bool SupportsAccount(AccountType accType)
        {
            return accType == AccountType.Net;
        }

        public IObservableListSource<PositionViewModel> Positions { get; private set; }

        private void PositionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace
              || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                    ((PositionViewModel)item).Dispose();
            }
        }
    }
}
