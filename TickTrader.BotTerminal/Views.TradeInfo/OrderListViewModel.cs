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
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    class OrderListViewModel : AccountBasedViewModel
    {
        private ProfileManager _profileManager;
        private bool _isBacktester;

        public OrderListViewModel(AccountModel model, IVarSet<string, SymbolModel> symbols, IConnectionStatusInfo connection, ProfileManager profile = null, bool isBacktester = false)
            : base(model, connection)
        {
            Orders = model.Orders
                .Where((id, order) => order.Type != Algo.Domain.OrderInfo.Types.Type.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, symbols.GetOrDefault(o.Symbol), model.BalanceDigits))
                .AsObservable();

            _profileManager = profile;
            _isBacktester = isBacktester;

            Orders.CollectionChanged += OrdersCollectionChanged;
            Account.AccountTypeChanged += () => NotifyOfPropertyChange(nameof(IsGrossAccount));

            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }
        }

        public ViewModelStorageEntry StateProvider { get; private set; }
        public IObservableList<OrderViewModel> Orders { get; private set; }
        public bool IsGrossAccount => Account.Type == AccountInfo.Types.Type.Gross;
        public bool AutoSizeColumns { get; set; }

        private void UpdateProvider()
        {
            StateProvider = _profileManager.CurrentProfile.GetViewModelStorage(_isBacktester ? ViewModelStorageKeys.OrdersBacktester : ViewModelStorageKeys.Orders);
            NotifyOfPropertyChange(nameof(StateProvider));
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
