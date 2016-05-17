using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class LoginPageViewModel : Screen, ILoginDialogPage, IPasswordContainer
    {
        private ConnectionManager cManager;
        private string login;
        private string password;
        private string server;
        private ConnectionErrorCodes error;
        private bool isConnecting;
        private bool isValid;
        private bool saveAccount = true;
        private bool savePassword;

        public LoginPageViewModel(ConnectionManager cManager, AccountAuthEntry displayEntry = null)
        {
            this.cManager = cManager;

            DisplayName = "Log In";

            if (displayEntry != null)
                ApplyAccount(displayEntry);
            else
            {
                var toApply = cManager.Creds;
                if (toApply == null)
                    toApply = cManager.GetLast();
                if (toApply != null)
                    ApplyAccount(toApply);
            }

            if (server == null)
                server = Servers.FirstOrDefault()?.Name;

            ValidateState();
        }

        #region Bindable Properties

        public string Login
        {
            get { return login; }
            set
            {
                if (login != value)
                {
                    login = value;
                    NotifyOfPropertyChange(nameof(Login));
                    ValidateState();
                    Password = null;
                }
            }
        }

        public string Server
        {
            get { return server; }
            set
            {
                server = value;
                NotifyOfPropertyChange(nameof(Server));
                ValidateState();
            }
        }

        public ObservableCollection<ServerAuthEntry> Servers { get { return cManager.Servers; } }
        public ObservableCollection<AccountAuthEntry> Accounts { get { return cManager.Accounts; } }

        public bool SaveAccount
        {
            get { return saveAccount; }
            set
            {
                saveAccount = value;
                if (!saveAccount)
                    SavePassword = false;
                NotifyOfPropertyChange(nameof(SaveAccount));
                NotifyOfPropertyChange(nameof(SavePasswordEnabled));
            }
        }

        public bool SavePasswordEnabled { get { return SaveAccount; } }

        public bool SavePassword
        {
            get { return savePassword; }
            set
            {
                savePassword = value;
                NotifyOfPropertyChange(nameof(SavePassword));
            }
        }

        public bool IsEditable
        {
            get { return !isConnecting; }
        }

        public bool IsConnecting
        {
            get { return isConnecting; }
            set
            {
                isConnecting = value;
                NotifyOfPropertyChange(nameof(IsConnecting));
                NotifyOfPropertyChange(nameof(IsEditable));
                NotifyOfPropertyChange(nameof(CanConnect));
            }
        }

        public bool HasError { get { return error != ConnectionErrorCodes.None; } }

        public ConnectionErrorCodes Error
        {
            get { return error; }
            set
            {
                error = value;
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }

        public bool CanConnect { get { return isValid && !isConnecting; } }

        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                NotifyOfPropertyChange(nameof(Password));
                ValidateState();
            }
        }

        public AccountAuthEntry SelectedAccount
        {
            get { return null; } // This is a magic trick to make ComboBox reselect already selected items. Do not remove this.
            set
            {
                if (value != null)
                    ApplyAccount(value);
                NotifyOfPropertyChange(nameof(SelectedAccount));
            }
        }

        #endregion

        public event System.Action Done = delegate { };

        public override void CanClose(Action<bool> callback)
        {
            base.CanClose(callback);
        }

        public async void Connect()
        {
            IsConnecting = true;
            Error = ConnectionErrorCodes.None;
            try
            {
                string address = ResolveServerAddress();
                Error = await cManager.Connect(login, password, address, savePassword, CancellationToken.None);
                if (Error == ConnectionErrorCodes.None)
                    Done();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LoginPageViewModel.Connect() failed: " + ex);
                Error = ConnectionErrorCodes.Unknown;
            }

            IsConnecting = false;
        }

        //private void RefreshAccount()
        //{
        //    var acc =  Accounts.FirstOrDefault(a => a.Login == login);
        //    if (acc != null)
        //        ApplyAccount(acc);
        //    else
        //        Password = null;
        //}

        private void ApplyAccount(AccountAuthEntry acc)
        {
            Login = acc.Login;
            Password = acc.Password;
            Server = acc.Server.Name;
            if (acc.Password != null)
                SavePassword = true;
            SaveAccount = true;
        }

        private string ResolveServerAddress()
        {
            var serverEntry = Servers.FirstOrDefault(s => s.Name == Server);
            if (serverEntry != null)
                return serverEntry.Address;
            return Server;
        }

        private void ValidateState()
        {
            isValid = !string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(server) && !string.IsNullOrEmpty(password);
            NotifyOfPropertyChange(nameof(CanConnect));
        }
    }

    internal interface IPasswordContainer : INotifyPropertyChanged
    {
        string Password { get; set; }
    }
}
