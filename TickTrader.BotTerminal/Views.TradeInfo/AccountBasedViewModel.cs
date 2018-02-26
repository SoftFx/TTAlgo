using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal abstract class AccountBasedViewModel : PropertyChangedBase
    {
        private ConnectionModel.Handler _connection;

        public AccountBasedViewModel(AccountModel model, ConnectionModel.Handler connection)
        {
            Account = model;
            _connection = connection;

            connection.StateChanged += StateChanged;
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

        protected virtual bool SupportsAccount(AccountTypes accType)
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

        private void StateChanged(ConnectionModel.States from, ConnectionModel.States to)
        {
            UpdateState();
        }
    }
}
