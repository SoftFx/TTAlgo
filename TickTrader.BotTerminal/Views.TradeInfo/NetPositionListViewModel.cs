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

        public NetPositionListViewModel(AccountModel model, IVarSet<string, SymbolModel> symbols, IConnectionStatusInfo connection, ProfileManager profile)
            : base(model, connection)
        {
            Positions = model.Positions
                .OrderBy((id, p) => id)
                .Select(p => new PositionViewModel(p))
                .AsObservable();

            _profileManager = profile;
            Positions.CollectionChanged += PositionsCollectionChanged;

            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }
        }

        public ProviderColumnsState StateProvider { get; private set; }

        protected override bool SupportsAccount(AccountTypes accType)
        {
            return accType == AccountTypes.Net;
        }

        public IObservableList<PositionViewModel> Positions { get; private set; }

        private void UpdateProvider()
        {
            if (_profileManager.CurrentProfile.ColumnsShow != null)
            {
                var postfix = nameof(NetPositionListViewModel);

                if (_profileManager.OpenBacktester)
                    postfix += "_backtester";

                StateProvider = new ProviderColumnsState(_profileManager.CurrentProfile.ColumnsShow, postfix);
                NotifyOfPropertyChange(nameof(StateProvider));
            }
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
