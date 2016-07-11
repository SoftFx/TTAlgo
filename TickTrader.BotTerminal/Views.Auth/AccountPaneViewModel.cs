using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TickTrader.BotTerminal
{
    internal class AccountPaneViewModel : PropertyChangedBase
    {
        private bool isDropDownOpen;
        private ConnectionManager cManager;
        private IConnectionViewModel connectionModel;
        private ObservableCollection<AccountViewModel> accounts;

        public AccountPaneViewModel(ConnectionManager cManager, IConnectionViewModel connectionModel)
        {
            this.cManager = cManager;
            this.connectionModel = connectionModel;
            accounts = new ObservableCollection<AccountViewModel>();
            foreach (var acc in cManager.Accounts)
                Accounts.Add(CreateAccountViewModel(acc, connectionModel, cManager));

            cManager.Accounts.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Accounts.Add(CreateAccountViewModel((AccountAuthEntry)e.NewItems[0], connectionModel, cManager));
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

            this.cManager.StateChanged += s => NotifyOfPropertyChange(nameof(ConnectionState));
            this.cManager.CredsChanged += () =>
            {
                IsDropDownOpen = false;
                DisplayedAccount = cManager.Creds;
                NotifyOfPropertyChange(nameof(DisplayedAccount));
            };

            UI.Instance.StateChanged += () => NotifyOfPropertyChange(nameof(Enabled));
        }

        public bool Enabled { get { return !UI.Instance.Locked; } }
        public ConnectionManager.States ConnectionState { get { return cManager.State; } }
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
            connectionModel.Connect(null);
        }

        #region Private methods
        private AccountViewModel CreateAccountViewModel(AccountAuthEntry acc, IConnectionViewModel connVM, ConnectionManager cManager)
        {
            var vm = new AccountViewModel(acc, connectionModel, cManager);
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
