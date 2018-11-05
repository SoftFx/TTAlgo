using Caliburn.Micro;
using NLog;
using System;
using System.Collections.ObjectModel;

namespace TickTrader.BotTerminal
{
    internal class BotAgentLoginDialogViewModel : Screen, IPasswordContainer
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();


        private BotAgentManager _botAgentManager;
        private string _login;
        private string _password;
        private string _server;
        private string _certificateName;
        private string _port;
        private string _error;
        private bool _isValid;
        private bool _isConnecting;


        public string Login
        {
            get => _login;
            set
            {
                if (_login == value)
                    return;

                _login = value;
                NotifyOfPropertyChange(nameof(Password));
                ValidateState();
                Password = null;
            }
        }

        public string Password
        {
            get => _password;
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
            get => _server;
            set
            {
                if (_server == value)
                    return;

                _server = value;
                NotifyOfPropertyChange(nameof(Server));
                ValidateState();
            }
        }

        public string CertificateName
        {
            get => _certificateName;
            set
            {
                if (_certificateName == value)
                    return;

                _certificateName = value;
                NotifyOfPropertyChange(nameof(CertificateName));
                ValidateState();
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                if (_port == value)
                    return;

                _port = value;
                NotifyOfPropertyChange(nameof(Port));
                ValidateState();
            }
        }

        public string Error
        {
            get => _error;
            set
            {
                _error = value;
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(_error);

        public bool IsConnecting
        {
            get => _isConnecting;
            set
            {
                _isConnecting = value;
                NotifyOfPropertyChange(nameof(IsConnecting));
                NotifyOfPropertyChange(nameof(CanConnect));
                NotifyOfPropertyChange(nameof(IsEditable));
            }
        }

        public bool CanConnect => _isValid && !_isConnecting;

        public bool IsEditable => !_isConnecting;

        public ObservableCollection<BotAgentServerEntry> Servers => _botAgentManager.PredefinedServers;

        public BotAgentServerEntry SelectedServer
        {
            get => null; // This is a magic trick to make ComboBox reselect already selected items. Do not remove this.
            set
            {
                if (value != null)
                {
                    Server = value.Address;
                    Port = value.Port.ToString();
                    CertificateName = value.CertificateName;
                }
                NotifyOfPropertyChange(nameof(SelectedServer));
            }
        }


        public BotAgentLoginDialogViewModel(BotAgentManager botAgentManager, BotAgentStorageEntry creds = null)
        {
            _botAgentManager = botAgentManager;

            DisplayName = "BotAgent Log In";

            Init(creds);
        }

        public async void Connect()
        {
            IsConnecting = true;
            Error = null;

            try
            {
                Error = await _botAgentManager.Connect(_login, _password, _server, int.Parse(_port), _certificateName);
                if (!HasError)
                {
                    TryClose();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Connect failed: {ex.Message}");
                Error = $"Connect failed: {ex.Message}";
            }

            IsConnecting = false;
        }


        private void Init(BotAgentStorageEntry creds)
        {
            if (creds != null)
            {
                Login = creds.Login;
                Password = creds.Password;
                Server = creds.ServerAddress;
                Port = creds.Port.ToString();
                CertificateName = creds.CertificateName;
            }

            if (_server == null)
            {
                Server = Servers.FirstOrDefault()?.Address;
            }
        }

        private void ValidateState()
        {
            _isValid = !string.IsNullOrWhiteSpace(_login)
                && !string.IsNullOrWhiteSpace(_password)
                && !string.IsNullOrWhiteSpace(_server)
                && !string.IsNullOrWhiteSpace(_certificateName)
                && !string.IsNullOrWhiteSpace(_port);
            if (_isValid)
            {
                var port = -1;
                var parseResult = int.TryParse(_port, out port);
                if (!parseResult || port < 0 || port > 65535)
                {
                    _isValid = false;
                }
            }

            NotifyOfPropertyChange(nameof(CanConnect));
        }
    }
}
