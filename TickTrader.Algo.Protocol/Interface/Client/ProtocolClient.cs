using Machinarium.State;
using NLog;
using SoftFX.Net.BotAgent;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Protocol.Lib;

namespace TickTrader.Algo.Protocol
{
    public enum ClientStates { Offline, Online, Connecting, Disconnecting, LoggingIn, LoggingOut, Initializing, Deinitializing };


    public enum ClientEvents { Started, Connected, Disconnected, ConnectionError, LoggedIn, LoggedOut, LoginReject, Initialized, Deinitialized, LogoutRequest }


    public class ProtocolClient
    {
        private ILogger _logger;
        private StateMachine<ClientStates> _stateMachine;


        internal ClientSession ClientSession { get; set; }

        internal BotAgentClientListener Listener { get; set; }


        public StateMachine<ClientStates> StateMachine => _stateMachine;

        public ClientStates State => _stateMachine.Current;

        public string LastError { get; internal set; }

        public IBotAgentClient AgentClient { get; }

        public IClientSessionSettings SessionSettings { get; internal set; }

        public VersionSpec VersionSpec { get; internal set; }


        public event Action Connecting = delegate { };
        public event Action Connected = delegate { };
        public event Action Disconnecting = delegate { };
        public event Action Disconnected = delegate { };


        public ProtocolClient(IBotAgentClient agentClient)
        {
            AgentClient = agentClient;

            VersionSpec = new VersionSpec();

            _stateMachine = new StateMachine<ClientStates>(ClientStates.Offline);

            _stateMachine.StateChanged += StateMachineOnStateChanged;
            _stateMachine.EventFired += StateMachineOnEventFired;

            _stateMachine.AddTransition(ClientStates.Offline, ClientEvents.Started, ClientStates.Connecting);
            _stateMachine.AddTransition(ClientStates.Connecting, ClientEvents.Connected, ClientStates.LoggingIn);
            _stateMachine.AddTransition(ClientStates.Connecting, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.LoggedIn, ClientStates.Initializing);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.LoginReject, ClientStates.Disconnecting);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.Initialized, ClientStates.Online);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.LoggedOut, ClientStates.Disconnecting);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.LogoutRequest, ClientStates.LoggingOut);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.LoggedOut, ClientStates.Disconnecting);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.LogoutRequest, ClientStates.LoggingOut);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.LoggedOut, ClientStates.Disconnecting);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Disconnecting, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Disconnecting, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Deinitializing, ClientEvents.Deinitialized, ClientStates.Offline);

            _stateMachine.OnEnter(ClientStates.Connecting, StartConnecting);
            _stateMachine.OnEnter(ClientStates.LoggingIn, SendLogin);
            _stateMachine.OnEnter(ClientStates.Initializing, Init);
            _stateMachine.OnEnter(ClientStates.LoggingOut, SendLogout);
            _stateMachine.OnEnter(ClientStates.Deinitializing, DeInit);
        }


        public void TriggerConnect(IClientSessionSettings settings)
        {
            _stateMachine.SyncContext.Synchronized(() =>
            {
                if (_stateMachine.Current != ClientStates.Offline)
                    throw new Exception($"Cannot connect! Client is in state {State}");

                SessionSettings = settings;

                _stateMachine.PushEvent(ClientEvents.Started);
            });
        }

        public Task Connect(IClientSessionSettings settings)
        {
            TriggerConnect(settings);
            return _stateMachine.AsyncWait(s => s == ClientStates.Online || s == ClientStates.Offline);
        }

        public void TriggerDisconnect()
        {
            _stateMachine.PushEvent(ClientEvents.LogoutRequest);
        }

        public Task Disconnect()
        {
            TriggerDisconnect();
            return _stateMachine.AsyncWait(ClientStates.Offline);
        }


        #region Connection routine

        private void StartConnecting()
        {
            _logger = LoggerHelper.GetLogger("Protocol.Client", SessionSettings.ProtocolSettings.LogDirectoryName, SessionSettings.ServerAddress);

            Listener = new BotAgentClientListener(AgentClient, _logger);

            Listener.Connected += ListenerOnConnected;
            Listener.Disconnected += ListenerOnDisconnected;
            Listener.ConnectionError += ListenerOnConnectionError;
            Listener.Login += ListenerOnLogin;
            Listener.LoginReject += ListenerOnLoginReject;
            Listener.Logout += ListenerOnLogout;
            Listener.Subscribed += ListenerOnSubscribed;

            ClientSession = new ClientSession(SessionSettings.ServerAddress, SessionSettings.CreateClientOptions())
            {
                Listener = Listener,
                Reconnect = false,
            };

            ClientSession.Connect(null, SessionSettings.ServerAddress);
        }

        private void ListenerOnConnected()
        {
            _stateMachine.PushEvent(ClientEvents.Connected);
        }

        private void ListenerOnDisconnected()
        {
            _stateMachine.PushEvent(ClientEvents.Disconnected);
        }

        private void ListenerOnConnectionError(string text)
        {
            LastError = $"Connection error: {text}";
            _stateMachine.PushEvent(ClientEvents.ConnectionError);
        }

        private void ListenerOnLogin(int currentVersion)
        {
            VersionSpec = new VersionSpec(currentVersion);
            _logger.Info($"Current version set to {VersionSpec.CurrentVersionStr}");
            _stateMachine.PushEvent(ClientEvents.LoggedIn);
        }

        private void ListenerOnLoginReject(string reason)
        {
            LastError = reason;
            _stateMachine.PushEvent(ClientEvents.LoginReject);
        }

        private void ListenerOnLogout(string reason)
        {
            LastError = reason;
            _stateMachine.PushEvent(ClientEvents.LoggedOut);
        }

        private void ListenerOnSubscribed()
        {
            _stateMachine.PushEvent(ClientEvents.Initialized);
        }

        private void DeInit()
        {
            Task.Factory.StartNew(() =>
            {
                if (ClientSession != null)
                {
                    ClientSession.Join();

                    ClientSession = null;
                }

                if (Listener != null)
                {
                    Listener.Connected -= ListenerOnConnected;
                    Listener.Disconnected -= ListenerOnDisconnected;
                    Listener.ConnectionError -= ListenerOnConnectionError;
                    Listener.Login -= ListenerOnLogin;
                    Listener.LoginReject -= ListenerOnLoginReject;
                    Listener.Logout -= ListenerOnLogout;
                    Listener.Subscribed -= ListenerOnSubscribed;

                    Listener = null;
                }

                _stateMachine.PushEvent(ClientEvents.Deinitialized);
            });
        }

        private void SendLogin()
        {
            ClientSession.SendLoginRequest(null, new LoginRequest(0) { Username = SessionSettings.Login, Password = SessionSettings.Password });
        }

        private void SendLogout()
        {
            ClientSession.SendLogoutRequest(null, new LogoutRequest(0));
        }

        private void Init()
        {
            ClientSession.SendAccountListRequest(null, new AccountListRequestEntity().ToMessage());
            ClientSession.SendBotListRequest(null, new BotListRequestEntity().ToMessage());
            ClientSession.SendPackageListRequest(null, new PackageListRequestEntity().ToMessage());
            ClientSession.SendSubscribeRequest(null, new SubscribeRequestEntity().ToMessage());
        }

        private void StateMachineOnStateChanged(ClientStates from, ClientStates to)
        {
            _logger?.Debug($"STATE {from} -> {to}");
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (to == ClientStates.Connecting)
                    {
                        Connecting();
                    }
                    if (to == ClientStates.Online)
                    {
                        Connected();
                    }
                    if (from == ClientStates.Online)
                    {
                        Disconnecting();
                    }
                    if (to == ClientStates.Offline)
                    {
                        Disconnected();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, $"Connection event failed: {ex.Message}");
                }
            });
        }

        private void StateMachineOnEventFired(object e)
        {
            _logger?.Debug($"EVENT {e}");
        }

        #endregion Connection routine


        #region Requests

        public Task<AccountListReportEntity> RequestAccounts(AccountListRequestEntity request)
        {
            var tcs = new TaskCompletionSource<AccountListReportEntity>();
            ClientSession.SendAccountListRequest(new AccountListRequestClientContext(false) { Data = tcs }, request.ToMessage());
            return tcs.Task;
        }

        public Task<BotListReportEntity> RequestBots(BotListRequestEntity request)
        {
            var tcs = new TaskCompletionSource<BotListReportEntity>();
            ClientSession.SendBotListRequest(new BotListRequestClientContext(false) { Data = tcs }, request.ToMessage());
            return tcs.Task;
        }

        public Task<PackageListReportEntity> RequestPackages(PackageListRequestEntity request)
        {
            var tcs = new TaskCompletionSource<PackageListReportEntity>();
            ClientSession.SendPackageListRequest(new PackageListRequestClientContext(false) { Data = tcs }, request.ToMessage());
            return tcs.Task;
        }

        #endregion Requests
    }
}
