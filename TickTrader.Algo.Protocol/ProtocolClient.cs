using Machinarium.State;
using NLog;
using SoftFX.Net.BotAgent;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Protocol.Lib;

namespace TickTrader.Algo.Protocol
{
    public enum ClientStates { Offline, Online, Connecting, Disconnecting, LoggingIn, LoggingOut, Initializing, Deinitializing };


    public enum ClientEvents { Started, Connected, Disconnected, ConnectionError, VersionMismatch, LoggedIn, LoggedOut, LoginReject, Initialized, Deinitialized, LogoutRequest }


    public class ProtocolClient
    {
        private readonly ILogger _logger;
        private StateMachine<ClientStates> _stateMachine;


        internal ClientSession ClientSession { get; set; }

        internal BotAgentClientListener Listener { get; set; }


        public ClientStates State => _stateMachine.Current;

        public string LastError { get; internal set; }

        public IBotAgentClient AgentClient { get; }

        public IClientSessionSettings SessionSettings { get; }


        public ProtocolClient(IBotAgentClient agentClient, IClientSessionSettings settings)
        {
            AgentClient = agentClient;
            SessionSettings = settings;

            _logger = LoggerHelper.GetLogger("Protocol.Client", SessionSettings.ProtocolSettings.LogDirectoryName, SessionSettings.ServerAddress);

            _stateMachine = new StateMachine<ClientStates>(ClientStates.Offline);

            _stateMachine.StateChanged += (from, to) => _logger.Debug($"STATE {from} -> {to}");
            _stateMachine.EventFired += e => _logger.Debug($"EVENT {e}");

            _stateMachine.AddTransition(ClientStates.Offline, ClientEvents.Started, ClientStates.Connecting);
            _stateMachine.AddTransition(ClientStates.Connecting, ClientEvents.Connected, ClientStates.LoggingIn);
            _stateMachine.AddTransition(ClientStates.Connecting, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Connecting, ClientEvents.LogoutRequest, ClientStates.Disconnecting);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.LoggedIn, ClientStates.Initializing);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.LoginReject, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.VersionMismatch, ClientStates.Disconnecting);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.Initialized, ClientStates.Online);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.LoggedOut, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.LogoutRequest, ClientStates.LoggingOut);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.LoggedOut, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.LogoutRequest, ClientStates.LoggingOut);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.LoggedOut, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Disconnecting, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Disconnecting, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Deinitializing, ClientEvents.Deinitialized, ClientStates.Offline);

            _stateMachine.OnEnter(ClientStates.Connecting, StartConnecting);
            _stateMachine.OnEnter(ClientStates.LoggingIn, SendLogin);
            _stateMachine.OnEnter(ClientStates.Initializing, Init);
            _stateMachine.OnEnter(ClientStates.LoggingOut, SendLogout);
            _stateMachine.OnEnter(ClientStates.Disconnecting, StartDisconnecting);
            _stateMachine.OnEnter(ClientStates.Deinitializing, DeInit);
        }


        public Task Connect()
        {
            return _stateMachine.ModifyConditionsAndWait(() =>
            {
                if (State != ClientStates.Offline)
                    throw new Exception($"Cannot connect! Client is in state {State}");

                _stateMachine.PushEvent(ClientEvents.Started);
            }, s => s == ClientStates.Online || s == ClientStates.Offline);
        }

        public Task Disconnect()
        {
            return _stateMachine.PushEventAndWait(ClientEvents.LogoutRequest, ClientStates.Offline);
        }

        public void RequestAccounts(AccountListRequestEntity request)
        {
            ClientSession.SendAccountListRequest(null, request.ToMessage());
        }


        private void StartConnecting()
        {
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

        private void ListenerOnLogin()
        {
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

        private void StartDisconnecting()
        {
            if (ClientSession != null)
            {
                ClientSession.Disconnect(null, "User request");
                ClientSession.Join();
            }
        }

        private void DeInit()
        {
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
            ClientSession.SendSubscribeRequest(null, new SubscribeRequestEntity().ToMessage());
        }
    }
}
