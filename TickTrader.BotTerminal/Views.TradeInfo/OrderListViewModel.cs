using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Machinarium.Qnil;
using Caliburn.Micro;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    class OrderListViewModel : AccountBasedViewModel
    {
        private SymbolCollectionModel _symbols;

        public OrderListViewModel(AccountModel model, SymbolCollectionModel symbols, ConnectionModel connection)
            : base(model, connection)
        {
            _symbols = symbols;

            Orders = model.Orders
                .Where((id, order) => order.OrderType != OrderType.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, (SymbolModel)symbols.GetOrDefault(o.Symbol)))
                .AsObservable();

            Orders.CollectionChanged += OrdersCollectionChanged;
            Account.AccountTypeChanged += () => NotifyOfPropertyChange(nameof(IsGrossAccount));
        }

        public IObservableListSource<OrderViewModel> Orders { get; private set; }
        public bool IsGrossAccount => Account.Type == AccountTypes.Gross;

        private void OrdersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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
