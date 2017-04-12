using Machinarium.Qnil;
using Machinarium.State;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class ConnectionManager
    {
        private Logger logger;
        internal enum States { Offline, Connecting, Online, Disconnecting }

        private enum InStates { Offline, Connecting, Online, Disconnecting }
        private enum InEvents { LostConnection, Connected, FailedToConnect, DoneDisconnecting, DisconnectRequest, RecconectTimer }

        private StateMachine<InStates> internalStateControl = new StateMachine<InStates>(new NullSync());
        private ConnectionRequest currentConnectionRequest;
        private AuthStorageModel authStorage;
        private EventJournal journal;
        private Task connectTask;
        private IDelayCounter connectionDelay;
        private CancellationTokenSource recconectTokenSource;

        public ConnectionManager(PersistModel appStorage, EventJournal journal)
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
            this.authStorage = appStorage.AuthSettingsStorage;
            this.authStorage.Accounts.Updated += Storage_Changed;
            this.journal = journal;
            this.connectionDelay = new ConnectionDelayCounter(TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(1));


            Accounts = new ObservableCollection<AccountAuthEntry>();
            Servers = new ObservableCollection<ServerAuthEntry>();

            InitAuthData();

            internalStateControl.AddTransition(InStates.Offline, () => HasRequest, InStates.Connecting);
            internalStateControl.AddTransition(InStates.Connecting, InEvents.Connected, InStates.Online);
            internalStateControl.AddTransition(InStates.Connecting, () => HasRequest, InStates.Disconnecting);
            internalStateControl.AddTransition(InStates.Connecting, InEvents.FailedToConnect, InStates.Offline);
            internalStateControl.AddTransition(InStates.Connecting, InEvents.DisconnectRequest, InStates.Disconnecting);
            internalStateControl.AddTransition(InStates.Online, () => HasRequest, InStates.Disconnecting);
            internalStateControl.AddTransition(InStates.Online, InEvents.LostConnection, InStates.Offline);
            internalStateControl.AddTransition(InStates.Online, InEvents.DisconnectRequest, InStates.Disconnecting);
            internalStateControl.AddTransition(InStates.Disconnecting, InEvents.DoneDisconnecting, InStates.Offline);

            internalStateControl.OnEnter(InStates.Offline, () =>
            {
                if (!HasRequest)
                    UpdateState(States.Offline);
            });

            internalStateControl.OnEnter(InStates.Connecting, () => connectTask = DoConnect());
            internalStateControl.OnEnter(InStates.Online, () => UpdateState(States.Online));
            internalStateControl.OnEnter(InStates.Disconnecting, DoDisconnect);

            var connectionOptions = new ConnectionOptions() { EnableFixLogs = BotTerminal.Properties.Settings.Default.EnableFixLogs };
            Connection = new ConnectionModel(connectionOptions, new DispatcherStateMachineSync());
            Connection.Connecting += () => { recconectTokenSource?.Cancel(); recconectTokenSource = null; };
            Connection.Connected += () => { connectionDelay.Reset(); };
            Connection.Disconnected += () =>
            {
                internalStateControl.PushEvent(InEvents.LostConnection);
                if (NeedReconnect)
                {
                    recconectTokenSource = new CancellationTokenSource();
                    RecconectAfter(recconectTokenSource.Token, connectionDelay.Next()).Forget();
                }
            };

            internalStateControl.StateChanged += (from, to) =>
            {
                if (IsConnected(from, to))
                    journal.Info("{0}: login on {1}", Creds.Login, Creds.Server.Name);
                else if (IsUsualDisconnect(from, to))
                    journal.Info("{0}: logout from {1}", GetLast().Login, GetLast().Server.Name);
                else if (IsFailedConnection(from, to))
                    journal.Error("{0}: connect failed [{1}]", Creds.Login, Connection.LastError);
                else if (IsUnexpectedDisconnect(from, to))
                    journal.Error("{0}: connection to {1} lost [{2}]",  GetLast().Login, GetLast().Server.Name, Connection.LastError);

                logger.Debug("INTERNAL STATE {0}", to);
            };
            internalStateControl.EventFired += e => logger.Debug("EVENT {0}", e);
        }

        private async Task RecconectAfter(CancellationToken token, TimeSpan delay)
        {
            await Task.Delay(delay, token);
            token.ThrowIfCancellationRequested();
            TriggerConnect(Creds);
        }

        private bool NeedReconnect
        {
            get
            {
                return !HasRequest
                  && Connection.HasError
                  && Connection.LastError != ConnectionErrorCodes.BlockedAccount
                  && Connection.LastError != ConnectionErrorCodes.InvalidCredentials;
            }
        }

        private bool IsConnected(InStates from, InStates to)
        {
            return to == InStates.Online;
        }
        private bool IsUnexpectedDisconnect(InStates from, InStates to)
        {
            return from == InStates.Online && to == InStates.Offline;
        }
        private bool IsFailedConnection(InStates from, InStates to)
        {
            return from == InStates.Connecting && to == InStates.Offline && Connection.HasError;
        }
        private bool IsUsualDisconnect(InStates from, InStates to)
        {
            return from == InStates.Disconnecting && to == InStates.Offline;
        }

        internal void RemoveAccount(AccountAuthEntry entry)
        {
            authStorage.Remove(new AccountSorageEntry(entry.Login, entry.Password, entry.Server.Address));
            authStorage.TriggerSave();
        }

        private bool HasRequest { get { return currentConnectionRequest != null; } }

        public States State { get; private set; }
        public ConnectionModel Connection { get; private set; }

        public AccountAuthEntry Creds { get; private set; }
        public ObservableCollection<AccountAuthEntry> Accounts { get; private set; }
        public ObservableCollection<ServerAuthEntry> Servers { get; private set; }

        public event Action CredsChanged = delegate { };
        public event Action<States, States> StateChanged = delegate { };

        public AccountAuthEntry GetLast()
        {
            if (authStorage.LastLogin != null && authStorage.LastServer != null)
                return Accounts.FirstOrDefault(a => a.Login == authStorage.LastLogin && a.Server.Address == authStorage.LastServer);
            return null;
        }

        public void TriggerConnect(AccountAuthEntry entry)
        {
            if (!entry.HasPassword)
                throw new InvalidOperationException("TriggerConnect() can be called only for accounts with saved password!");

            Connect(entry.Login, entry.Password, entry.Server.Address, true, CancellationToken.None).Forget();
        }

        public async Task<ConnectionErrorCodes> Connect(string login, string password, string server, bool savePwd, CancellationToken cToken)
        {
            logger.Debug("Connect to {0}, {1}", login, server);

            string entryPassword = savePwd ? password : null;
            var newCreds = CreateEntry(login, entryPassword, server);

            CancelRequest();
            Creds = newCreds;
            CredsChanged();
            UpdateState(States.Connecting);

            var request = new ConnectionRequest(Creds.Login, password, Creds.Server.Address, cToken);
            internalStateControl.ModifyConditions(() => currentConnectionRequest = request);

            var error = await request.WaitHandler;

            if (error == ConnectionErrorCodes.None)
                SaveLogin(newCreds);

            return error;
        }

        public void TriggerDisconnect()
        {
            if (State == States.Offline)
                return;

            CancelRequest();
            UpdateState(States.Disconnecting);
            internalStateControl.PushEvent(InEvents.DisconnectRequest);
        }

        public async Task Disconnect()
        {
            if (State == States.Offline)
                return;

            CancelRequest();
            UpdateState(States.Disconnecting);
            await internalStateControl.PushEventAndWait(InEvents.DisconnectRequest, InStates.Offline);
        }

        private void InitAuthData()
        {
            AuthConfigSection cfgSection = AuthConfigSection.GetCfgSection();

            foreach (ServerElement predefinedServer in cfgSection.Servers)
                Servers.Add(new ServerAuthEntry(predefinedServer));

            foreach (var acc in authStorage.Accounts.Values)
                Accounts.Add(CreateEntry(acc));
        }

        private void SaveLogin(AccountAuthEntry entry)
        {
            authStorage.Update(new AccountSorageEntry(entry.Login, entry.Password, entry.Server.Address));
            authStorage.UpdateLast(entry.Login, entry.Server.Address);
            authStorage.TriggerSave();
        }

        private void Storage_Changed(ListUpdateArgs<AccountSorageEntry> e)
        {
            if (e.Action == DLinqAction.Insert)
                Accounts.Add(CreateEntry(e.NewItem));
            else if (e.Action == DLinqAction.Remove)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts.RemoveAt(index);
            }
            else if (e.Action == DLinqAction.Replace)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts[index] = CreateEntry(e.NewItem);
            }
        }

        private AccountAuthEntry CreateEntry(string login, string password, string server)
        {
            return CreateEntry(new AccountSorageEntry(login, password, server));
        }

        private AccountAuthEntry CreateEntry(AccountSorageEntry record)
        {
            return new AccountAuthEntry(record, GetServer(record.ServerAddress));
        }

        private ServerAuthEntry GetServer(string address)
        {
            var server = Servers.FirstOrDefault(s => s.Address == address);

            if (server != null)
                return server;

            return new ServerAuthEntry(address);
        }

        private void UpdateState(States newState)
        {
            if (State != newState)
            {
                logger.Debug("PUBLIC STATE {0}", newState);

                var oldState = State;
                State = newState;
                StateChanged(oldState, newState);
            }
        }

        private async Task DoConnect()
        {
            var request = currentConnectionRequest;
            currentConnectionRequest = null;
            var code = await Connection.Connect(request.Login, request.Password, request.Server, request.CancelToken);
            request.SetResult(code);
            if (code == ConnectionErrorCodes.None)
                internalStateControl.PushEvent(InEvents.Connected);
            else
                internalStateControl.PushEvent(InEvents.FailedToConnect);
        }

        private async void DoDisconnect()
        {
            await Task.WhenAll(connectTask, Connection.DisconnectAsync());

            internalStateControl.PushEvent(InEvents.DoneDisconnecting);
        }

        private void CancelRequest()
        {
            if (currentConnectionRequest != null)
            {
                currentConnectionRequest.SetCanceled();
                currentConnectionRequest = null;
            }
        }

        internal class ConnectionRequest
        {
            private TaskCompletionSource<ConnectionErrorCodes> connectionWaitEvent = new TaskCompletionSource<ConnectionErrorCodes>();

            public ConnectionRequest(string login, string password, string server, CancellationToken cToken)
            {
                this.Login = login;
                this.Password = password;
                this.Server = server;
                this.CancelToken = cToken;
            }

            public string Login { get; private set; }
            public string Password { get; private set; }
            public string Server { get; private set; }
            public CancellationToken CancelToken { get; private set; }

            public Task<ConnectionErrorCodes> WaitHandler { get { return connectionWaitEvent.Task; } }

            public void SetCanceled()
            {
                connectionWaitEvent.TrySetResult(ConnectionErrorCodes.Canceled);
            }

            public void SetResult(ConnectionErrorCodes code)
            {
                connectionWaitEvent.TrySetResult(code);
            }
        }
    }

    internal class ConnectionDelayCounter : IDelayCounter
    {
        private TimeSpan _minDelay;
        private TimeSpan _maxDelay;
        private TimeSpan? _currentDelay;
        private object _syncOnj = new object();

        public ConnectionDelayCounter(TimeSpan minDelay, TimeSpan maxDelay)
        {
            if (minDelay > maxDelay)
                throw new ArgumentException("maxDelay should be more then minDelay");

            _maxDelay = maxDelay;
            _minDelay = minDelay;
            _currentDelay = null;
        }

        public TimeSpan Value
        {
            get
            {
                lock (_syncOnj)
                    return _currentDelay ?? _minDelay;
            }
        }

        public TimeSpan Next()
        {
            lock (_syncOnj)
            {
                if (!_currentDelay.HasValue)
                    _currentDelay = _minDelay;
                else if (_currentDelay.Value == _maxDelay)
                    _currentDelay = _maxDelay;
                else
                {
                    var nextDelay = TimeSpan.FromMilliseconds(_currentDelay.Value.TotalMilliseconds * 2);

                    if (_maxDelay <= nextDelay)
                        _currentDelay = _maxDelay;
                    else
                        _currentDelay = nextDelay;
                }

                return _currentDelay.Value;
            }
        }

        public void Reset()
        {
            lock (_syncOnj)
                _currentDelay = null;
        }
    }

    internal interface IDelayCounter
    {
        TimeSpan Value { get; }
        TimeSpan Next();
        void Reset();
    }
}
