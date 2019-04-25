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
        private ProfileManager _profileManager;

        public OrderListViewModel(AccountModel model, IVarSet<string, SymbolModel> symbols, IConnectionStatusInfo connection, ProfileManager profile)
            : base(model, connection)
        {
            Orders = model.Orders
                .Where((id, order) => order.OrderType != OrderType.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, symbols.GetOrDefault(o.Symbol)))
                .AsObservable();

            _profileManager = profile;

            Orders.CollectionChanged += OrdersCollectionChanged;
            Account.AccountTypeChanged += () => NotifyOfPropertyChange(nameof(IsGrossAccount));

            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }
        }

        public ProviderColumnsState StateProvider { get; private set; }
        public IObservableList<OrderViewModel> Orders { get; private set; }
        public bool IsGrossAccount => Account.Type == AccountTypes.Gross;
        public bool AutoSizeColumns { get; set; }

        private void UpdateProvider()
        {
            if (_profileManager.CurrentProfile.ColumnsShow != null)
            {
                var prefix = nameof(OrderListViewModel);

                if (_profileManager.OpenBacktester)
                    prefix += "_backtester";

                StateProvider = new ProviderColumnsState(_profileManager.CurrentProfile.ColumnsShow, prefix);
                NotifyOfPropertyChange(nameof(StateProvider));
            }
        }

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
