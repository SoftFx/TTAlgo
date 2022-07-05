using System.Linq;
using Machinarium.Qnil;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Account;

namespace TickTrader.BotTerminal
{
    class GrossPositionListViewModel : AccountBasedViewModel
    {
        private readonly bool _isBacktester;
        private ProfileManager _profileManager;


        public IObservableList<OrderViewModel> Positions { get; private set; }
        public bool AutoSizeColumns { get; set; }
        public ViewModelStorageEntry StateProvider { get; private set; }


        public GrossPositionListViewModel(AccountModel model, IVarSet<string, SymbolInfo> symbols, IConnectionStatusInfo connection, ProfileManager profileManager, bool isBacktester)
            : base(model, connection)
        {
            _profileManager = profileManager;
            _isBacktester = isBacktester;

            Positions = model.Orders
                .Where((id, order) => order.Type == Algo.Domain.OrderInfo.Types.Type.Position)
                .OrderBy((id, order) => id)
                .Select(o => new OrderViewModel(o, symbols.GetOrDefault(o.Symbol), model.BalanceDigits))
                .DisposeItems()
                .AsObservable();

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


        private void UpdateProvider()
        {
            StateProvider = _profileManager.CurrentProfile.GetViewModelStorage(_isBacktester ? ViewModelStorageKeys.GrossPositionsBacktester : ViewModelStorageKeys.GrossPositions);
            NotifyOfPropertyChange(nameof(StateProvider));
        }
    }
}
