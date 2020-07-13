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
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    class GrossPositionListViewModel : AccountBasedViewModel
    {
        private readonly bool _isBacktester;
        private ProfileManager _profileManager;

        public GrossPositionListViewModel(AccountModel model, IVarSet<string, SymbolModel> symbols, IConnectionStatusInfo connection, ProfileManager profileManager, bool isBacktester)
            : base(model, connection)
        {
            _profileManager = profileManager;
            _isBacktester = isBacktester;

            Positions = model.Orders
                .Where((id, order) => order.OrderType == OrderType.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, symbols.GetOrDefault(o.Symbol), model))
                .AsObservable();

            Positions.CollectionChanged += PositionsCollectionChanged;

            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }
        }

        protected override bool SupportsAccount(AccountInfo.Types.Type accType)
        {
            return accType == AccountInfo.Types.Type.Gross;
        }

        public IObservableList<OrderViewModel> Positions { get; private set; }
        public bool AutoSizeColumns { get; set; }
        public ViewModelStorageEntry StateProvider { get; private set; }

        private void PositionsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace
              || e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                    ((OrderViewModel)item).Dispose();
            }
        }

        private void UpdateProvider()
        {
            StateProvider = _profileManager.CurrentProfile.GetViewModelStorage(_isBacktester ? ViewModelStorageKeys.GrossPositionsBacktester : ViewModelStorageKeys.GrossPositions);
            NotifyOfPropertyChange(nameof(StateProvider));
        }
    }
}
