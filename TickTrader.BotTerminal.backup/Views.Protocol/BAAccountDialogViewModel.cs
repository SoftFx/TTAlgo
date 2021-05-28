using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.BotTerminal
{
    internal sealed class BAAccountDialogViewModel : Screen, IWindowModel
    {
        private readonly VarContext _context = new VarContext();
        private readonly AlgoEnvironment _algoEnv;
        private readonly AccountModelInfo _account;

        private readonly BoolVar _canAddAccount, _canChangeAccount, _canTestAccountCreds;


        public IObservableList<AlgoAgentViewModel> AlgoServersList { get; }

        public IEnumerable<AccountAuthEntry> LocalAccounts => _algoEnv.Shell.ConnectionManager.Accounts.Where(u => u.Server.Address == TTServerName?.Value)?.OrderBy(u => u.Login.Length).ThenBy(u => u.Login);

        public IEnumerable<AlgoAccountViewModel> ServerAccounts => AlgoServer?.Value?.AccountList.Where(u => u.Server == TTServerName?.Value && u.Info != _account);

        public ObservableCollection<ServerAuthEntry> TTServersList => _algoEnv.Shell.ConnectionManager.Servers;


        public Property<AlgoAgentViewModel> AlgoServer { get; }

        public Property<string> TTServerName { get; }

        public Property<string> Error { get; }

        public Validable<string> Login { get; }

        public Validable<string> Password { get; }

        public Validable<string> DisplayAccountName { get; }


        public BoolVar CanOk { get; }

        public BoolVar CanTest { get; }

        public BoolVar IsEnabled { get; }

        public BoolVar SuccessConnect { get; }


        public bool IsNewAccountMode { get; }


        public BAAccountDialogViewModel(AlgoEnvironment algoEnv, AccountModelInfo account, AlgoAgentViewModel algoServer, string serverName = null)
        {
            _algoEnv = algoEnv;
            _account = account;

            _canAddAccount = new BoolVar();
            _canChangeAccount = new BoolVar();
            _canTestAccountCreds = new BoolVar();

            IsNewAccountMode = account == null;
            DisplayName = $"{(IsNewAccountMode ? "Add" : "Edit")} account";

            AlgoServersList = algoEnv.BotAgents.Select(u => u.Agent).AsObservable();

            var login = LocalAccounts.FirstOrDefault()?.Login;
            var server = serverName ?? TTServersList.FirstOrDefault()?.Address;

            if (!IsNewAccountMode)
                AccountId.Unpack(_account.AccountId, out login, out server);

            AlgoServer = _context.AddProperty(algoServer).AddPreTrigger(DeinitAlgoAgent);
            TTServerName = _context.AddProperty(server).AddPostTrigger(InitTTServerTrigger);
            Login = _context.AddValidable(login).AddPostTrigger(InitLoginTrigger).MustBeNotEmpty();
            DisplayAccountName = _context.AddValidable(_account?.DisplayName ?? login);
            Password = _context.AddValidable<string>().MustBeNotEmpty();
            Error = _context.AddProperty<string>();

            IsEnabled = new BoolVar(true);
            SuccessConnect = new BoolVar();

            CanTest = IsEnabled & !_context.HasError & _canTestAccountCreds;
            CanOk = IsEnabled & !_context.HasError & (IsNewAccountMode ? _canAddAccount : _canChangeAccount);

            AlgoServer.AddPostTrigger(InitAlgoAgent); //should be after AlgoServer initialization
            Login.AddValidationRule((newLogin) => !ServerAccounts?.Any(u => u.Login == newLogin) ?? false, "This value is already in use");
            DisplayAccountName.AddValidationRule((newName) => !ServerAccounts?.Any(u => u.DisplayName == newName) ?? false, "This value is already in use");
        }

        public async void Ok() => await TryToRunConnectionRequest(async () =>
            {
                if (IsNewAccountMode)
                    await AlgoServer.Value.Model.AddAccount(new AddAccountRequest(TTServerName.Value, Login.Value, Password.Value, DisplayAccountName.Value));
                else
                    await AlgoServer.Value.Model.ChangeAccount(new ChangeAccountRequest(_account.AccountId, Password.Value, DisplayAccountName.Value));
            });

        public async void Test() => await TryToRunConnectionRequest(async () =>
            {
                var error = await AlgoServer.Value.Model.TestAccountCreds(new TestAccountCredsRequest(TTServerName.Value, Login.Value, new AccountCreds(Password.Value)));

                SuccessConnect.Value = error.IsOk;
                Error.Value = error.IsOk ? null : $"{error.Code} - {error.TextMessage}";
            }, false);

        private async Task TryToRunConnectionRequest(Func<Task> request, bool closeWindow = true)
        {
            try
            {
                IsEnabled.Value = false;
                Error.Value = null;

                await request();
            }
            catch (Exception ex)
            {
                Error.Value = ex.Message;
            }

            IsEnabled.Value = true;

            if (!Error.HasValue && closeWindow)
                await TryCloseAsync(true);
        }

        private void InitAlgoAgent(AlgoAgentViewModel model)
        {
            if (model != null)
                model.Model.AccessLevelChanged += OnAccessLevelChanged;

            Login?.Validate();
            DisplayAccountName?.Validate();
            OnAccessLevelChanged();
        }

        private void DeinitAlgoAgent(AlgoAgentViewModel model)
        {
            if (model != null)
                model.Model.AccessLevelChanged -= OnAccessLevelChanged;
        }

        private void InitTTServerTrigger(string newTTServer)
        {
            SetProperty(Login, LocalAccounts.FirstOrDefault()?.Login);
            NotifyOfPropertyChange(nameof(LocalAccounts));
        }

        private void InitLoginTrigger(string newLogin)
        {
            SetProperty(DisplayAccountName, newLogin);
            SetProperty(Password, LocalAccounts.FirstOrDefault(u => u.Login == newLogin)?.Password);
        }

        private void OnAccessLevelChanged()
        {
            _canAddAccount.Value = AlgoServer?.Value?.Model.AccessManager.CanAddAccount() ?? false;
            _canChangeAccount.Value = AlgoServer?.Value?.Model.AccessManager.CanChangeAccount() ?? false;
            _canTestAccountCreds.Value = AlgoServer?.Value?.Model.AccessManager.CanTestAccountCreds() ?? false;
        }

        private static void SetProperty<T>(IProperty<T> prop, T value)
        {
            if (prop != null)
                prop.Value = value;
        }
    }
}
