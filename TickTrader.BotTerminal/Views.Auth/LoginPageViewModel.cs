using Caliburn.Micro;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Interop;

namespace TickTrader.BotTerminal
{
    internal class LoginPageViewModel : Screen, ILoginDialogPage, IPasswordContainer
    {
        private Logger logger;
        private ConnectionManager cManager;
        private string login;
        private string password;
        private string server;
        private ConnectionErrorInfo error;
        private bool isConnecting;
        private bool isValid;
        private bool savePassword;

        public LoginPageViewModel(ConnectionManager cManager, AccountAuthEntry displayEntry = null)
        {
            this.cManager = cManager;

            logger = NLog.LogManager.GetCurrentClassLogger();
            DisplayName = "Log In";
            SavePassword = true;

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
        public IEnumerable<AccountAuthEntry> Accounts => cManager.Accounts.OrderBy(u => long.Parse(u.Login));

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
                NotifyOfPropertyChange(nameof(CanConnectProp));
            }
        }

        public bool ShowErrorCode { get; private set; }
        public bool ShowErrorText { get; private set; }
        public ConnectionErrorCodes ErrorCode => error?.Code ?? ConnectionErrorCodes.None;
        public string ErrorText => error?.TextMessage;

        public ConnectionErrorInfo Error
        {
            get { return error; }
            set
            {
                error = value;

                if (error == null)
                {
                    ShowErrorCode = false;
                    ShowErrorText = false;
                }
                else if (error.Code == ConnectionErrorCodes.Unknown && !string.IsNullOrWhiteSpace(error.TextMessage))
                {
                    ShowErrorCode = false;
                    ShowErrorText = true;
                }
                else
                {
                    ShowErrorCode = true;
                    ShowErrorText = false;
                }


                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(ErrorText));
                NotifyOfPropertyChange(nameof(ShowErrorCode));
                NotifyOfPropertyChange(nameof(ShowErrorText));
                NotifyOfPropertyChange(nameof(ErrorCode));
            }
        }

        public bool CanConnectProp { get { return isValid && !isConnecting; } }

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
            if (!CanConnectProp)
                return;

            IsConnecting = true;
            Error = null;
            try
            {
                string address = ResolveServerAddress();
                Error = await cManager.Connect(login, password, address, savePassword, CancellationToken.None);
                if (Error.Code == ConnectionErrorCodes.None)
                    Done();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Connect Failed.");
                Error = ConnectionErrorInfo.UnknownNoText;
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
            SavePassword = acc.Password != null;
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
            NotifyOfPropertyChange(nameof(CanConnectProp));
        }
    }

    internal interface IPasswordContainer : INotifyPropertyChanged
    {
        string Password { get; set; }
    }
}
