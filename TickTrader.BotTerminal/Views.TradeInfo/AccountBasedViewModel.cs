using Caliburn.Micro;
using TickTrader.Algo.Account;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal abstract class AccountBasedViewModel : PropertyChangedBase
    {
        private IConnectionStatusInfo _connection;

        public AccountBasedViewModel(AccountModel model, IConnectionStatusInfo connection)
        {
            Account = model;
            _connection = connection;

            connection.IsConnectingChanged += UpdateState;
            model.AccountTypeChanged += AccountTypeChanged;
        }

        private void AccountTypeChanged()
        {
            var oldEnabled = IsEnabled;
            if (Account.Type != null)
                IsEnabled = SupportsAccount(Account.Type.Value);
            else
                IsEnabled = false;
            if (oldEnabled != IsEnabled)
                NotifyOfPropertyChange(nameof(IsEnabled));
        }

        public AccountModel Account { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsEnabled { get; private set; }

        protected virtual bool SupportsAccount(AccountInfo.Types.Type accType)
        {
            return true;
        }

        private void UpdateState()
        {
            var oldIsBusy = IsBusy;
            IsBusy = _connection.IsConnecting;
            if (oldIsBusy != IsBusy)
                NotifyOfPropertyChange(nameof(IsBusy));
        }
    }
}
