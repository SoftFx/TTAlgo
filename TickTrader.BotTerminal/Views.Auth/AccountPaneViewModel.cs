using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using TickTrader.Algo.Account;

namespace TickTrader.BotTerminal
{
    internal class AccountPaneViewModel : PropertyChangedBase
    {
        private bool isDropDownOpen;
        private ObservableCollection<AccountViewModel> accounts;
        private IShell _shell;

        public AccountPaneViewModel(IShell shell)
        {
            _shell = shell;
            accounts = new ObservableCollection<AccountViewModel>();
            foreach (var acc in _shell.ConnectionManager.Accounts)
                Accounts.Add(CreateAccountViewModel(acc));

            _shell.ConnectionManager.Accounts.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Accounts.Add(CreateAccountViewModel((AccountAuthEntry)e.NewItems[0]));
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        {
                            var accForRemove = Accounts.FirstOrDefault(a => a.Account.Equals((AccountAuthEntry)e.OldItems[0]));
                            if (accForRemove != null)
                            {
                                accForRemove.StateChanged -= AccountViewModelStateChanged;
                                Accounts.Remove(accForRemove);
                            }
                        }
                        break;
                }
            };

            _shell.ConnectionManager.ConnectionStateChanged += (os, ns) => NotifyOfPropertyChange(nameof(ConnectionState));
            _shell.ConnectionManager.CredsChanged += () =>
            {
                IsDropDownOpen = false;
                DisplayedAccount = _shell.ConnectionManager.Creds;
                NotifyOfPropertyChange(nameof(DisplayedAccount));
            };

            ConnectionLock = shell.ConnectionLock;
        }

        public UiLock ConnectionLock { get; private set; }
        public ConnectionModel.States ConnectionState { get { return _shell.ConnectionManager.State; } }
        public AccountAuthEntry DisplayedAccount { get; set; }
        public ObservableCollection<AccountViewModel> Accounts { get { return accounts; } }
        public bool IsDropDownOpen
        {
            get { return isDropDownOpen; }
            set
            {
                if (isDropDownOpen != value)
                {
                    isDropDownOpen = value;
                    NotifyOfPropertyChange(nameof(IsDropDownOpen));

                    if (!isDropDownOpen)
                        ResetAccountDisplayMode(null);
                }
            }
        }

        public void Connect()
        {
            _shell.Connect(null);
        }

        #region Private methods
        private AccountViewModel CreateAccountViewModel(AccountAuthEntry acc)
        {
            var vm = new AccountViewModel(acc, _shell);
            vm.StateChanged += AccountViewModelStateChanged;
            return vm;
        }
        private void AccountViewModelStateChanged(AccountViewModel accountVM)
        {
            if (accountVM.State == AccountDisplatMode.ConfirmRemove)
                ResetAccountDisplayMode(accountVM);
        }
        private void ResetAccountDisplayMode(AccountViewModel excludeAcc)
        {
            foreach (var account in Accounts)
                if (!account.Equals(excludeAcc))
                    account.ResetState();
        }
        #endregion
    }
}
