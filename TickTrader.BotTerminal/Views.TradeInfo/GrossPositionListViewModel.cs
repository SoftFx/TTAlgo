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
    class GrossPositionListViewModel : AccountBasedViewModel
    {
        private SymbolCollectionModel _symbols;

        public GrossPositionListViewModel(AccountModel model, SymbolCollectionModel symbols) : base(model)
        {
            _symbols = symbols;

            Positions = model.Orders
                .Where((id, order) => order.OrderType == TradeRecordType.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, _symbols[o.Symbol]))
                .AsObservable();

            Positions.CollectionChanged += PositionsCollectionChanged;
        }

        protected override bool SupportsAccount(AccountType accType)
        {
            return accType == AccountType.Gross;
        }

        public IObservableListSource<OrderViewModel> Positions { get; private set; }
        private void PositionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace
              || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                    ((OrderViewModel)item).Dispose();
            }
        }
    }
}
