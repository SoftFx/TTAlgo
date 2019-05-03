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
    class NetPositionListViewModel : AccountBasedViewModel
    {
        private ProfileManager _profileManager;
        private bool _isBacktester;

        public NetPositionListViewModel(AccountModel model, IVarSet<string, SymbolModel> symbols, IConnectionStatusInfo connection, ProfileManager profile = null, bool isBacktester = false)
            : base(model, connection)
        {
            Positions = model.Positions
                .OrderBy((id, p) => id)
                .Select(p => new PositionViewModel(p))
                .AsObservable();

            _profileManager = profile;
            _isBacktester = isBacktester;
            Positions.CollectionChanged += PositionsCollectionChanged;

            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }
        }

        public ViewModelPropertiesStorageEntry StateProvider { get; private set; }

        protected override bool SupportsAccount(AccountTypes accType)
        {
            return accType == AccountTypes.Net;
        }

        public IObservableList<PositionViewModel> Positions { get; private set; }

        private void UpdateProvider()
        {
            StateProvider = _isBacktester ? _profileManager.CurrentProfile.NetPositionsBacktesterStorage : _profileManager.CurrentProfile.NetPositionsStorage;
            NotifyOfPropertyChange(nameof(StateProvider));
        }

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
