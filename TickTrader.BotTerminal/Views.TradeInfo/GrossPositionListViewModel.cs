using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    class GrossPositionListViewModel : AccountBasedViewModel
    {
        public GrossPositionListViewModel(AccountModel model, IVarSet<string, SymbolModel> symbols, ConnectionModel.Handler connection)
            : base(model, connection)
        {
            Positions = model.Orders
                .Where((id, order) => order.OrderType == OrderType.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, symbols.GetOrDefault(o.Symbol)))
                .AsObservable();

            Positions.CollectionChanged += PositionsCollectionChanged;
        }

        protected override bool SupportsAccount(AccountTypes accType)
        {
            return accType == AccountTypes.Gross;
        }

        public IObservableList<OrderViewModel> Positions { get; private set; }

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
