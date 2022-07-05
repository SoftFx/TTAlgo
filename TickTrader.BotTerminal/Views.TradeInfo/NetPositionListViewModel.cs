using System.Linq;
using Machinarium.Qnil;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Account;

namespace TickTrader.BotTerminal
{
    class NetPositionListViewModel : AccountBasedViewModel
    {
        private ProfileManager _profileManager;
        private bool _isBacktester;


        public IObservableList<PositionViewModel> Positions { get; private set; }
        public ViewModelStorageEntry StateProvider { get; private set; }
        public bool AutoSizeColumns { get; set; }


        public NetPositionListViewModel(AccountModel model, IConnectionStatusInfo connection, ProfileManager profile = null, bool isBacktester = false)
            : base(model, connection)
        {
            Positions = model.Positions
                .OrderBy((id, p) => id)
                .Select(p => new PositionViewModel(p, model))
                .DisposeItems()
                .AsObservable();

            _profileManager = profile;
            _isBacktester = isBacktester;

            if (_profileManager != null)
            {
                _profileManager.ProfileUpdated += UpdateProvider;
                UpdateProvider();
            }
        }


        protected override bool SupportsAccount(AccountInfo.Types.Type accType)
        {
            return accType == AccountInfo.Types.Type.Net;
        }


        private void UpdateProvider()
        {
            StateProvider = _profileManager.CurrentProfile.GetViewModelStorage(_isBacktester ? ViewModelStorageKeys.NetPositionsBacktester : ViewModelStorageKeys.NetPositions);
            NotifyOfPropertyChange(nameof(StateProvider));
        }
    }
}
