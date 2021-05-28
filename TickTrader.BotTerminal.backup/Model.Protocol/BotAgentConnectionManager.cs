using Machinarium.State;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.ServerControl;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class BotAgentConnectionManager
    {
        public enum States { Offline, Connecting, Online, Disconnecting, WaitReconnect }


        public enum Events { ConnectRequest, Connected, DisconnectRequest, Disconnected, Reconnect }


        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        private ProtocolClient _protocolClient;
        private StateMachine<States> _stateControl;
        private bool _needReconnect;


        public ClientClaims.Types.AccessLevel AccessLevel => _protocolClient.AccessManager.Level;

        public States State => _stateControl.Current;

        public ClientStates ClientState => _protocolClient.State;

        public string LastError => _protocolClient.LastError;

        public string Status => string.IsNullOrEmpty(_protocolClient.LastError) ? $"{_stateControl.Current}" : $"{_stateControl.Current} - {_protocolClient.LastError}";

        public BotAgentStorageEntry Creds { get; }

        public RemoteAlgoAgent RemoteAgent { get; }


        public event Action StateChanged = delegate { };


        public BotAgentConnectionManager(BotAgentStorageEntry botAgentCreds)
        {
            Creds = botAgentCreds;

            RemoteAgent = new RemoteAlgoAgent(Creds.Name);
            _protocolClient = ProtocolClient.Create(RemoteAgent);
            RemoteAgent.SetProtocolClient(_protocolClient);

            _protocolClient.Connected += ClientOnConnected;
            _protocolClient.Disconnected += ClientOnDisconnected;

            _stateControl = new StateMachine<States>(new DispatcherStateMachineSync());
            _stateControl.AddTransition(States.Offline, Events.ConnectRequest, States.Connecting);
            _stateControl.AddTransition(States.Connecting, Events.Connected, States.Online);
            _stateControl.AddTransition(States.Connecting, Events.Disconnected, () => _needReconnect, States.WaitReconnect);
            _stateControl.AddTransition(States.Connecting, Events.Disconnected, States.Offline);
            _stateControl.AddTransition(States.Online, Events.Disconnected, () => _needReconnect, States.WaitReconnect);
            _stateControl.AddTransition(States.Online, Events.Disconnected, States.Offline);
            _stateControl.AddTransition(States.Online, Events.ConnectRequest, States.Disconnecting);
            _stateControl.AddTransition(States.Online, Events.DisconnectRequest, States.Disconnecting);
            _stateControl.AddTransition(States.Disconnecting, Events.Disconnected, () => _needReconnect, States.Connecting);
            _stateControl.AddTransition(States.Disconnecting, Events.Disconnected, States.Offline);
            _stateControl.AddTransition(States.WaitReconnect, Events.ConnectRequest, States.Connecting);
            _stateControl.AddTransition(States.WaitReconnect, Events.DisconnectRequest, States.Offline);
            _stateControl.AddTransition(States.WaitReconnect, Events.Reconnect, States.Connecting);

            _stateControl.AddScheduledEvent(States.WaitReconnect, Events.Reconnect, 10000);

            _stateControl.OnEnter(States.Connecting, StartConnecting);
            _stateControl.OnEnter(States.Disconnecting, StartDisconnecting);
            _stateControl.OnEnter(States.Offline, () => RemoteAgent.ClearCache());

            _stateControl.StateChanged += OnStateChanged;
        }


        public void Connect()
        {
            _stateControl.ModifyConditions(() =>
            {
                _needReconnect = true;
                _stateControl.PushEvent(Events.ConnectRequest);
            });
        }

        public Task WaitConnect()
        {
            Connect();
            return _stateControl.AsyncWait(s => s == States.Offline || s == States.Online || s == States.WaitReconnect);
        }

        public void Disconnect()
        {
            _stateControl.ModifyConditions(() =>
            {
                _needReconnect = false;
                _stateControl.PushEvent(Events.DisconnectRequest);
            });
        }

        public Task WaitDisconnect()
        {
            Disconnect();
            return _stateControl.AsyncWait(States.Offline);
        }


        private void OnStateChanged(States from, States to)
        {
            StateChanged();
        }

        private void ClientOnConnected()
        {
            _stateControl.PushEvent(Events.Connected);
        }

        private void ClientOnDisconnected()
        {
            _stateControl.PushEvent(Events.Disconnected);
        }

        private void StartConnecting()
        {
            _protocolClient.TriggerConnect(Creds.ToClientSettings());
        }

        private void StartDisconnecting()
        {
            _protocolClient.TriggerDisconnect();
        }
    }
}
