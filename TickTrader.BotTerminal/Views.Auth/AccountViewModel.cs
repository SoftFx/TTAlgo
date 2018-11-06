using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public enum AccountDisplatMode
    {
        Default,
        ConfirmRemove,
        Removed
    }

    internal class AccountViewModel : PropertyChangedBase
    {
        private AccountAuthEntry account;
        private AccountDisplatMode state;
        private IShell _shell;

        public AccountViewModel(AccountAuthEntry account, IShell shell)
        {
            Account = account;
            _shell = shell;
            _shell.ConnectionManager.CredsChanged += CredsChanged;
        }

        private void CredsChanged()
        {
            if (IsSelected)
                State = AccountDisplatMode.Default;

            NotifyOfPropertyChange(nameof(IsSelected));
        }

        public event Action<AccountViewModel> StateChanged = delegate { };

        public AccountAuthEntry Account
        {
            get { return account; }
            private set
            {
                account = value;
                NotifyOfPropertyChange(nameof(Account));
            }
        }

        public bool IsSelected
        {
            get { return Account.Equals(_shell.ConnectionManager.Creds); }
        }

        public AccountDisplatMode State
        {
            get { return state; }
            private set
            {
                if (state != value)
                {
                    state = value;
                    NotifyOfPropertyChange(nameof(State));
                    StateChanged(this);
                }
            }
        }

        public void Connect()
        {
            _shell.Connect(Account);
        }

        public void Remove()
        {
            if (State == AccountDisplatMode.Default)
            {
                State = AccountDisplatMode.ConfirmRemove;
            }
            else if (State == AccountDisplatMode.ConfirmRemove)
            {
                State = AccountDisplatMode.Removed;
                _shell.ConnectionManager.CredsChanged -= CredsChanged;
                _shell.ConnectionManager.RemoveAccount(Account);
            }
        }

        public void Cancel()
        {
            if (State == AccountDisplatMode.ConfirmRemove)
                ResetState();
        }

        public void ResetState()
        {
            State = AccountDisplatMode.Default;
        }

        public override bool Equals(object obj)
        {
            var entry = obj as AccountViewModel;
            return entry != null && entry.Account.Equals(this.Account);
        }

        public override int GetHashCode()
        {
            return Account.GetHashCode();
        }
    }
}
