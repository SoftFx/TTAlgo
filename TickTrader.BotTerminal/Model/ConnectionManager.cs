using Machinarium.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class ConnectionManager
    {
        public enum States { Offline, Connecting, Online, Disconnecting }

        private enum InStates { Offline, Connecting, Online, Disconnecting }
        private enum InEvents { LostConnection, Connected, FailedToConnect, DoneDisconnecting, DisconnectRequest }

        private StateMachine<InStates> internalStateControl = new StateMachine<InStates>(new NullSync());
        private ConnectionRequest currentConnectionRequest;
        private AuthStorageModel authStorage;
        private Task connectTask;

        public ConnectionManager(PersistModel appStorage)
        {
            this.authStorage = appStorage.AuthSettingsStorage;
            this.authStorage.Accounts.Changed += Storage_Changed;

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
            internalStateControl.OnEnter(InStates.Online, ()=> UpdateState(States.Online));
            internalStateControl.OnEnter(InStates.Disconnecting, DoDisconnect);

            Connection = new ConnectionModel();
            Connection.Disconnected += () => internalStateControl.PushEvent(InEvents.LostConnection);

            internalStateControl.StateChanged += (from, to) => System.Diagnostics.Debug.WriteLine("ConnectionManager INTERNAL STATE " + to);
            internalStateControl.EventFired += e => System.Diagnostics.Debug.WriteLine("ConnectionManager EVENT " + e);
        }

        private bool HasRequest { get { return currentConnectionRequest != null; } }

        public States State { get; private set; }
        public ConnectionModel Connection { get; private set; }

        public AccountAuthEntry Creds { get; private set; }
        public ObservableCollection<AccountAuthEntry> Accounts { get; private set; }
        public ObservableCollection<ServerAuthEntry> Servers { get; private set; }

        public event Action CredsChanged = delegate { };
        public event Action<States> StateChanged = delegate { };

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
            Debug.WriteLine("ConnectionManager.Connect(" + login + ", " + server + ")");

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
            UpdateState(States.Offline);
            internalStateControl.PushEvent(InEvents.DisconnectRequest);
        }

        public Task Disconnect()
        {
            if (State == States.Offline)
                return Task.FromResult(0);

            CancelRequest();
            UpdateState(States.Disconnecting);
            return internalStateControl.PushEventAndWait(InEvents.DisconnectRequest, state => state == InStates.Offline || state == InStates.Connecting);
        }

        private void InitAuthData()
        {
            AuthConfigSection cfgSection = AuthConfigSection.GetCfgSection();

            foreach (ServerElement predefinedServer in cfgSection.Servers)
                Servers.Add(new ServerAuthEntry(predefinedServer));

            foreach (var acc in authStorage.Accounts)
                Accounts.Add(CreateEntry(acc));
        }

        private void SaveLogin(AccountAuthEntry entry)
        {
            authStorage.Update(new AccountSorageEntry(entry.Login, entry.Password, entry.Server.Address));
            authStorage.UpdateLast(entry.Login, entry.Server.Address);
            authStorage.TriggerSave();
        }

        private void Storage_Changed(object sender, ListChangedEventArgs<AccountSorageEntry> e)
        {
            if (e.Action == CollectionChangeActions.Added)
                Accounts.Add(CreateEntry(e.NewItem));
            else if (e.Action == CollectionChangeActions.Removed)
            {
                var index = Accounts.IndexOf(a => a.Matches(e.OldItem));
                Accounts.RemoveAt(index);
            }
            else if (e.Action == CollectionChangeActions.Replaced)
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
                System.Diagnostics.Debug.WriteLine("ConnectionManager PUBLIC STATE " + newState);

                State = newState;
                StateChanged(newState);
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
            await connectTask;
            await Connection.DisconnectAsync();

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
}
