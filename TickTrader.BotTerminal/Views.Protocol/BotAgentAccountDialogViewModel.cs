using Caliburn.Micro;
using System;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class BotAgentAccountDialogViewModel : Screen, IWindowModel, IPasswordContainer
    {
        private RemoteAlgoAgent _remoteAgent;
        private string _login;
        private string _password;
        private string _server;
        private bool _useSfx;
        private bool _isValid;
        private AccountModelInfo _account;
        private bool _isEditable;
        private string _error;
        private string _success;


        public string Login
        {
            get { return _login; }
            set
            {
                if (_login == value)
                    return;

                _login = value;
                NotifyOfPropertyChange(nameof(Login));
                ValidateState();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password == value)
                    return;

                _password = value;
                NotifyOfPropertyChange(nameof(Password));
                ValidateState();
            }
        }

        public string Server
        {
            get { return _server; }
            set
            {
                if (_server == value)
                    return;

                _server = value;
                NotifyOfPropertyChange(nameof(Server));
                ValidateState();
            }
        }

        public bool UseSfxProtocol
        {
            get { return _useSfx; }
            set
            {
                if (_useSfx == value)
                    return;

                _useSfx = value;
                NotifyOfPropertyChange(nameof(UseSfxProtocol));
            }
        }

        public bool IsEditable
        {
            get { return _isEditable; }
            set
            {
                if (_isEditable == value)
                    return;

                _isEditable = value;
                NotifyOfPropertyChange(nameof(IsEditable));
                NotifyOfPropertyChange(nameof(CanChangeAccountKey));
                NotifyOfPropertyChange(nameof(CanOk));
                NotifyOfPropertyChange(nameof(CanTest));
            }
        }

        public bool IsNewMode { get; }

        public bool CanChangeAccountKey => IsNewMode && IsEditable;

        public bool CanOk => _isValid && IsEditable;

        public bool CanTest => _isValid && IsEditable && !string.IsNullOrEmpty(_password);

        public string Error
        {
            get { return _error; }
            set
            {
                if (_error == value)
                    return;

                _error = value;
                Success = null;
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(_error);

        public string Success
        {
            get { return _success; }
            set
            {
                if (_success == value)
                    return;

                _success = value;
                Error = null;
                NotifyOfPropertyChange(nameof(Success));
                NotifyOfPropertyChange(nameof(HasSuccess));
            }
        }

        public bool HasSuccess => !string.IsNullOrEmpty(_success);


        public BotAgentAccountDialogViewModel(RemoteAlgoAgent remoteAgent)
        {
            _remoteAgent = remoteAgent;

            IsEditable = true;
            IsNewMode = true;
            DisplayName = "Add account";
        }

        public BotAgentAccountDialogViewModel(RemoteAlgoAgent remoteAgent, AccountModelInfo account)
        {
            _remoteAgent = remoteAgent;
            _account = account;

            IsEditable = true;
            IsNewMode = false;
            DisplayName = "Edit account";

            Login = _account.Key.Login;
            Server = _account.Key.Server;
            UseSfxProtocol = _account.UseNewProtocol;
        }


        public async void Ok()
        {
            IsEditable = false;
            Error = null;
            try
            {
                if (_account == null)
                    await _remoteAgent.AddAccount(new AccountKey(Server, Login), Password, UseSfxProtocol);
                else await _remoteAgent.ChangeAccount(_account.Key, Password, UseSfxProtocol);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            IsEditable = true;
            if (!HasError)
                TryClose();
        }

        public async void Test()
        {
            IsEditable = false;
            Error = null;
            try
            {
                var error = await _remoteAgent.TestAccountCreds(new AccountKey(Server, Login), Password, UseSfxProtocol);
                if (error.Code == ConnectionErrorCodes.None)
                    Success = "Successfully connected";
                else Error = string.IsNullOrEmpty(error.TextMessage) ? $"{error.Code}" : $"{error.Code} - {error.TextMessage}";
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            IsEditable = true;
        }

        public void Cancel()
        {
            TryClose();
        }


        private void ValidateState()
        {
            _isValid = !string.IsNullOrWhiteSpace(_login) && !string.IsNullOrWhiteSpace(_server) 
                && (!string.IsNullOrEmpty(_password) || _account != null);
            NotifyOfPropertyChange(nameof(CanOk));
            NotifyOfPropertyChange(nameof(CanTest));
        }
    }
}
