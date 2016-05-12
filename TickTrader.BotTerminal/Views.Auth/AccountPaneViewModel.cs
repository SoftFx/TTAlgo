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
        private AuthManager authModel;
        private ConnectionModel connection;
        private IWindowManager wndManager;

        public AccountPaneViewModel(AuthManager authModel, ConnectionModel connection, IWindowManager wndManager)
        {
            this.authModel = authModel;
            this.connection = connection;
            this.wndManager = wndManager;
            this.connection.State.StateChanged += (s1,s2) => NotifyOfPropertyChange(nameof(ConnectionState));
        }

        public ConnectionModel.States ConnectionState
        {
            get { return connection.State.Current; }
        }
        public ObservableCollection<AccountAuthEntry> Accounts { get { return authModel.Accounts; } }
        public AccountAuthEntry SelectedAccount
        {
            get { return null; } // This is a magic trick to make ComboBox reselect already selected items. Do not remove this.
            set
            {
                if (value != null)
                    Reconnect(value);
                NotifyOfPropertyChange(nameof(SelectedAccount));
            }
        }
        public void Connect()
        {
            try
            {
                LoginDialogViewModel model = new LoginDialogViewModel(authModel, connection);
                wndManager.ShowDialog(model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async void Reconnect(AccountAuthEntry account)
        {
            await connection.Connect(account.Server.Address, account.Login, account.Password);
        }
    }
}
