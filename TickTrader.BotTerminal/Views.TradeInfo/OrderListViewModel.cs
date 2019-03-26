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
        public OrderListViewModel(AccountModel model, IVarSet<string, SymbolModel> symbols, IConnectionStatusInfo connection)
            : base(model, connection)
        {
            Orders = model.Orders
                .Where((id, order) => order.OrderType != OrderType.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, symbols.GetOrDefault(o.Symbol)))
                .AsObservable();

            Orders.CollectionChanged += OrdersCollectionChanged;
            Account.AccountTypeChanged += () => NotifyOfPropertyChange(nameof(IsGrossAccount));
            Account.AccountTypeChanged += () => NotifyOfPropertyChange(nameof(IsNetAccount));
        }

        public IObservableList<OrderViewModel> Orders { get; private set; }
        public bool IsGrossAccount => Account.Type == AccountTypes.Gross;

        public bool IsNetAccount => Account.Type == AccountTypes.Net;

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
