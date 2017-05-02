using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal abstract class AccountBasedViewModel : PropertyChangedBase
    {
        public AccountBasedViewModel(AccountModel model)
        {
            this.Account = model;

            model.Connection.State.StateChanged += StateChanged;
            model.AccountTypeChanged += AccountTypeChanged;
        }

        private void AccountTypeChanged()
        {
            var oldEnabled = IsEnabled;
            IsEnabled = SupportsAccount(Account.Type.Value);
            if (oldEnabled != IsEnabled)
                NotifyOfPropertyChange(nameof(IsEnabled));
        }

        public AccountModel Account { get; private set; }
        public bool IsBusy { get; private set; }
        public bool IsEnabled { get; private set; }

        protected virtual bool SupportsAccount(AccountType accType)
        {
            return true;
        }

        private void UpdateState()
        {
            var oldIsBusy = IsBusy;
            IsBusy = Account.Connection.IsConnecting;
            if (oldIsBusy != IsBusy)
                NotifyOfPropertyChange(nameof(IsBusy));
        }

        private void StateChanged(ConnectionModel.States from, ConnectionModel.States to)
        {
            UpdateState();
        }
    }
}
