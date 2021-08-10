using Machinarium.State;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    public abstract class ProtocolClient
    {
        private const int LossConnectionTimeOut = 15000;

        protected readonly IAlgoServerEventHandler _serverHandler;
        protected readonly StateMachine<ClientStates> _stateMachine;

        private Timer _lossConnectionTimer;
        protected ILogger _logger;


        public string LastError { get; private set; }

        public ClientStates State => _stateMachine.Current;

        public IVersionSpec VersionSpec { get; protected set; }

        public IAccessManager AccessManager { get; protected set; }

        protected IClientSessionSettings SessionSettings { get; private set; }


        public event Action<ClientStates> ClientStateChanged = delegate { };


        internal ProtocolClient(IAlgoServerEventHandler handler)
        {
            _serverHandler = handler;
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
            _stateMachine.OnEnter(ClientStates.Disconnecting, SendDisconnect);
            _stateMachine.OnEnter(ClientStates.Deinitializing, DeInit);
        }


        public Task Connect(IClientSessionSettings settings)
        {
            _stateMachine.SyncContext.Synchronized(() =>
            {
                if (_stateMachine.Current != ClientStates.Offline)
                    throw new Exception($"Cannot connect! Client is in state {_stateMachine.Current}");

                SessionSettings = settings;

                _stateMachine.PushEvent(ClientEvents.Started);
            });

            return _stateMachine.AsyncWait(s => s == ClientStates.Online || s == ClientStates.Offline);
        }

        private void StateMachineOnStateChanged(ClientStates from, ClientStates to)
        {
            _logger?.Debug($"STATE {from} -> {to}");

            Task.Run(() =>
            {
                try
                {
                    ClientStateChanged?.Invoke(to);
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, $"{to} event failed: {ex.Message}");
                }
            });
        }

        private void StartConnecting()
        {
            _logger = LoggerHelper.GetLogger(GetType().Name, System.IO.Path.Combine(SessionSettings.LogDirectory, GetType().Name), SessionSettings.ServerAddress);

            LastError = null;

            try
            {
                StartClient();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start client");
                OnConnectionError("Client failed to start");
            }
        }

        private void DeInit()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    StopClient();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to stop client");
                }

                _stateMachine.PushEvent(ClientEvents.Deinitialized);
            });
        }

        private void StateMachineOnEventFired(object e)
        {
            _logger?.Debug($"EVENT {e}");
        }

        public Task Disconnect()
        {
            if (_stateMachine.Current == ClientStates.Offline)
                return Task.CompletedTask;

            _stateMachine.PushEvent(ClientEvents.LogoutRequest);
            return _stateMachine.AsyncWait(ClientStates.Offline);
        }


        public abstract void StartClient();

        public abstract void StopClient();

        public abstract void SendLogin();

        public abstract void SendLogout();

        public abstract void SendDisconnect();

        public abstract void Init();


        protected void OnConnected()
        {
            _stateMachine.PushEvent(ClientEvents.Connected);
        }

        protected void OnDisconnected()
        {
            StopConnectionTimer();
            _stateMachine.PushEvent(ClientEvents.Disconnected);
        }

        protected void OnConnectionError(string text, Exception ex = null)
        {
            LastError = text;
            _logger.Error(ex, $"Connection error: {text}");

            _stateMachine.PushEvent(ClientEvents.ConnectionError);
        }

        protected void OnLogin(int serverMajorVersion, int serverMinorVersion, ClientClaims.Types.AccessLevel accessLevel)
        {
            VersionSpec = new ApiVersionSpec(serverMinorVersion);
            AccessManager = new ApiAccessManager(accessLevel);

            _serverHandler.AccessLevelChanged();

            _logger.Info($"Client version - {VersionSpec.MajorVersion}.{VersionSpec.MinorVersion}; Server version - {serverMajorVersion}.{serverMinorVersion}");
            _logger.Info($"Current version set to {VersionSpec.MajorVersion}.{VersionSpec.CurrentVersion}");

            _stateMachine.PushEvent(ClientEvents.LoggedIn);
        }

        protected void OnLogout(LogoutResponse.Types.LogoutReason reason)
        {
            LastError = reason == LogoutResponse.Types.LogoutReason.ClientRequest ? string.Empty : reason.ToString();

            AccessManager = new ApiAccessManager(ClientClaims.Types.AccessLevel.Anonymous);
            _serverHandler.AccessLevelChanged();

            _stateMachine.PushEvent(ClientEvents.LoggedOut);
        }

        protected void OnSubscribed()
        {
            StartConnectionTimer();

            _stateMachine.PushEvent(ClientEvents.Initialized);
        }

        protected void StartConnectionTimer()
        {
            _lossConnectionTimer = new Timer(BreakServerConnection, null, LossConnectionTimeOut, -1);
        }

        protected void StopConnectionTimer()
        {
            _lossConnectionTimer?.Dispose();
            _lossConnectionTimer = null;
        }

        protected void RefreshConnectionTimer()
        {
            _lossConnectionTimer?.Change(LossConnectionTimeOut, -1);
        }

        protected void BreakServerConnection(object obj) => OnConnectionError("Server connection has been lost");
    }
}
