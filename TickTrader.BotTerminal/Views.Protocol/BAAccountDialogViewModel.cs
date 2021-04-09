using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.BotTerminal
{
    internal class BAAccountDialogViewModel : Screen, IWindowModel, IPasswordContainer
    {
        private AlgoEnvironment _algoEnv;
        private AlgoAgentViewModel _selectedAgent;
        private AgentPluginSetupViewModel _selectedPlugin;
        private string _login;
        private string _password;
        private string _server;
        private bool _isValid;
        private AccountModelInfo _account;
        private bool _isEditable;
        private string _error;
        private string _success;


        public IObservableList<AlgoAgentViewModel> Agents { get; }

        public AlgoAgentViewModel SelectedAgent
        {
            get { return _selectedAgent; }
            set
            {
                if (_selectedAgent == value)
                    return;

                DeinitAlgoAgent(_selectedAgent);
                _selectedAgent = value;
                InitAlgoAgent(_selectedAgent);
                NotifyOfPropertyChange(nameof(SelectedAgent));
                ValidateState();
            }
        }

        public string Login
        {
            get { return _login; }
            set
            {
                if (_login == value)
                    return;

                _login = value;

                if (!Accounts.Any(u => u.Login == Login && u.Server.Address == Server))
                    _password = null;

                NotifyOfPropertyChange(nameof(Login));
                NotifyOfPropertyChange(nameof(Password));
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
                NotifyOfPropertyChange(nameof(Accounts));

                SelectedAccount = Accounts.FirstOrDefault();

                ValidateState();
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

        public bool AllowedChangeAgentKey { get; }

        public bool IsNewMode { get; }

        public bool CanChangeAgentKey => AllowedChangeAgentKey && CanChangeAccountKey;

        public bool CanChangeAccountKey => IsNewMode && IsEditable;

        public bool CanOk => _isValid && IsEditable
            && (IsNewMode ? SelectedAgent.Model.AccessManager.CanAddAccount() : SelectedAgent.Model.AccessManager.CanChangeAccount());

        public bool CanTest => _isValid && IsEditable && !string.IsNullOrEmpty(_password) && SelectedAgent.Model.AccessManager.CanTestAccountCreds();

        public string Error
        {
            get { return _error; }
            set
            {
                if (_error == value)
                    return;

                _error = value;
                _success = null;
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
                NotifyOfPropertyChange(nameof(Success));
                NotifyOfPropertyChange(nameof(HasSuccess));
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
                _error = null;
                NotifyOfPropertyChange(nameof(Success));
                NotifyOfPropertyChange(nameof(HasSuccess));
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }

        public bool HasSuccess => !string.IsNullOrEmpty(_success);

        public ObservableCollection<ServerAuthEntry> Servers => _algoEnv.Shell.ConnectionManager.Servers;

        public ServerAuthEntry SelectedServer
        {
            get { return null; } // This is a magic trick to make ComboBox reselect already selected items. Do not remove this.
            set
            {
                if (value != null)
                {
                    Server = value.Address;
                }
                NotifyOfPropertyChange(nameof(SelectedServer));
            }
        }

        public IEnumerable<AccountAuthEntry> Accounts => _algoEnv.Shell.ConnectionManager.Accounts.Where(u => u.Server.Address == Server).OrderBy(u => long.Parse(u.Login));

        public AccountAuthEntry SelectedAccount
        {
            get { return null; } // This is a magic trick to make ComboBox reselect already selected items. Do not remove this.
            set
            {
                if (value != null)
                {
                    Login = value.Login;
                    Password = value.Password;
                }
                else
                {
                    Login = "";
                    Password = "";
                }

                NotifyOfPropertyChange(nameof(SelectedAccount));
            }
        }


        public BAAccountDialogViewModel(AlgoEnvironment algoEnv, AccountModelInfo account, string agentName, AgentPluginSetupViewModel plugin = null, bool allowedChangeAgentKey = true)
        {
            _algoEnv = algoEnv;
            _account = account;
            _selectedPlugin = plugin;

            IsEditable = true;
            AllowedChangeAgentKey = allowedChangeAgentKey;

            Agents = _algoEnv.BotAgents.Select(b => b.Agent).AsObservable();
            SelectedAgent = Agents.FirstOrDefault(a => a.Name == agentName);

            if (_account == null)
            {
                IsNewMode = true;
                DisplayName = "Add account";

                SelectedServer = Servers.FirstOrDefault();
            }
            else
            {
                IsNewMode = false;
                DisplayName = "Edit account";

                AccountId.Unpack(_account.AccountId, out var accId);
                _login = accId.UserId;
                _server = accId.Server;
            }
        }


        public async void Ok()
        {
            IsEditable = false;
            Error = null;
            try
            {
                if (_account == null)
                    await SelectedAgent.Model.AddAccount(new AddAccountRequest(Server, Login, new AccountCreds(Password)));
                else
                {
                    var request = new ChangeAccountRequest(_account.AccountId, null);
                    if (Password != null)
                        request.Creds = new AccountCreds(Password);

                    await SelectedAgent.Model.ChangeAccount(request);
                }

                if (_selectedPlugin != null && _selectedPlugin.SelectedAgent.Name == SelectedAgent.Name)
                {
                    _selectedPlugin.SelectedAccount = _selectedPlugin.Accounts.FirstOrDefault(a => a.Login == Login) ?? (_selectedPlugin.Accounts.Any() ? _selectedPlugin.Accounts.First() : null);
                }
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
                var error = await SelectedAgent.Model.TestAccountCreds(new TestAccountCredsRequest(Server, Login, new AccountCreds(Password)));
                if (error.IsOk)
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
            _isValid = _selectedAgent != null
                && !string.IsNullOrWhiteSpace(_login)
                && !string.IsNullOrWhiteSpace(_server)
                && (!string.IsNullOrEmpty(_password) || _account != null);
            NotifyOfPropertyChange(nameof(CanOk));
            NotifyOfPropertyChange(nameof(CanTest));
        }

        private void InitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.AccessLevelChanged += OnAccessLevelChanged;
            }
        }

        private void DeinitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.AccessLevelChanged -= OnAccessLevelChanged;
            }
        }

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanOk));
            NotifyOfPropertyChange(nameof(CanTest));
        }
    }
}
