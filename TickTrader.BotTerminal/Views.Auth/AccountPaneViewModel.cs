using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    internal class AccountPaneViewModel : PropertyChangedBase
    {
        private AccountAuthEntry selectedEntry;
        private ConnectionManager cManager;
        private IConnectionViewModel connectionModel;

        public AccountPaneViewModel(ConnectionManager cManager, IConnectionViewModel connectionModel)
        {
            this.cManager = cManager;
            this.connectionModel = connectionModel;
            this.cManager.StateChanged += s => NotifyOfPropertyChange(nameof(ConnectionState));
            this.cManager.CredsChanged += () =>
            {
                selectedEntry = cManager.Creds;
                NotifyOfPropertyChange(nameof(SelectedAccount));
            };
        }

        public string ConnectionState
        {
            get { return cManager.State.ToString(); }
        }

        public ObservableCollection<AccountAuthEntry> Accounts { get { return cManager.Accounts; } }

        public AccountAuthEntry SelectedAccount
        {
            get { return selectedEntry; }
            set
            {
                if (value != null)
                    Reconnect(value);
            }
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
