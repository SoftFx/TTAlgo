using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TickTrader.BotTerminal
{
    internal class AccountPaneViewModel : PropertyChangedBase
    {
        private ConnectionManager cManager;
        private IConnectionViewModel connectionModel;

        public AccountPaneViewModel(ConnectionManager cManager, IConnectionViewModel connectionModel)
        {
            this.cManager = cManager;
            this.connectionModel = connectionModel;
            this.cManager.StateChanged += s => NotifyOfPropertyChange(nameof(ConnectionState));
            this.cManager.Accounts.CollectionChanged += (s,o) => NotifyOfPropertyChange(nameof(SelectedAccount));
            this.cManager.CredsChanged += () =>
            {
                DisplayedAccount = cManager.Creds;

                NotifyOfPropertyChange(nameof(DisplayedAccount));
                NotifyOfPropertyChange(nameof(SelectedAccount));
            };

            DisplayedAccount = cManager.GetLast();
        }

        public ConnectionManager.States ConnectionState
        {
            get { return cManager.State; }
        }

        public ObservableCollection<AccountAuthEntry> Accounts { get { return cManager.Accounts; } }

        public AccountAuthEntry SelectedAccount
        {
            get { return Accounts.FirstOrDefault(a => a.Equals(DisplayedAccount)); }
            set
            {
                if (value != null)
                    Reconnect(value);
            }
        }

        public AccountAuthEntry DisplayedAccount { get; set; }

        public void RemoveAccount(AccountAuthEntry account)
        {
            if (!account.Equals(SelectedAccount))
                cManager.RemoveAccount(account);
        }

        public void Connect()
        {
            connectionModel.Connect(null);
        }

        private void Reconnect(AccountAuthEntry account)
        {
            connectionModel.Connect(account);
        }
    }
}
