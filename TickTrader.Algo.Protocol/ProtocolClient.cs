using Machinarium.State;
using NLog;
using SoftFX.Net.BotAgent;
using System;
using TickTrader.Algo.Protocol.Lib;

namespace TickTrader.Algo.Protocol
{
    public enum ClientStates { Offline, Online, Connecting, Disconnecting, LoggingIn, LoggingOut, Initializing, Deinitializing };


    public enum ClientEvents { Started, Connected, Disconnected, ConnectionError, VersionMismatch, LoggedIn, LoggedOut, Initialized, Deinitialized }


    public class ProtocolClient
    {
        private readonly ILogger _logger;
        private StateMachine<ClientStates> _stateMachine;


        internal ClientSession ClientSession { get; set; }

        internal BotAgentClientListener Listener { get; set; }


        public ClientStates State => _stateMachine.Current;

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
            _stateMachine.AddTransition(ClientStates.Connecting, ClientEvents.ConnectionError, ClientStates.Offline);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.LoggedIn, ClientStates.Initializing);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.VersionMismatch, ClientStates.Offline);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.ConnectionError, ClientStates.Offline);
            _stateMachine.AddTransition(ClientStates.LoggingIn, ClientEvents.Disconnected, ClientStates.Offline);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.Initialized, ClientStates.Online);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Initializing, ClientEvents.LoggedOut, ClientStates.LoggingOut);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Online, ClientEvents.LoggedOut, ClientStates.LoggingOut);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.LoggedOut, ClientStates.Disconnecting);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.LoggingOut, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Disconnecting, ClientEvents.Disconnected, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Disconnecting, ClientEvents.ConnectionError, ClientStates.Deinitializing);
            _stateMachine.AddTransition(ClientStates.Deinitializing, ClientEvents.Deinitialized, ClientStates.Offline);
        }


        public void Connect()
        {
            if (State != ClientStates.Offline)
                throw new Exception($"Cannot connect! Client is in state {State}");

            Listener = new BotAgentClientListener(AgentClient, _logger);

            ClientSession = new ClientSession(SessionSettings.ServerAddress, SessionSettings.CreateClientOptions())
            {
                Listener = Listener,
            };

            ClientSession.Connect(null, SessionSettings.ServerAddress);

            _stateMachine.PushEvent(ClientEvents.Started);
        }

        public void Disconnect()
        {
            if (State == ClientStates.Online)
            {
                ClientSession.Disconnect(null, "User request");
                ClientSession.Join();

                _stateMachine.PushEvent(ClientEvents.Disconnected);
            }
        }

        public void RequestAccounts(AccountListRequestEntity request)
        {
            ClientSession.SendAccountListRequest(null, request.ToMessage());
        }
    }
}
