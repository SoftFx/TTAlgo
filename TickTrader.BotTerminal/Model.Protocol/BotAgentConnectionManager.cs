using Machinarium.State;
using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Server.PublicAPI;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class BotAgentConnectionManager
    {
        public enum States { Offline, Connecting, Online, Disconnecting, WaitReconnect }


        public enum Events { ConnectRequest, Connected, DisconnectRequest, Disconnected, Reconnect }


        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        private IAlgoServerClient _protocolClient;
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
            _protocolClient = AlgoServerClient.Create(RemoteAgent);
            RemoteAgent.SetProtocolClient(_protocolClient);

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
            if (_stateControl.Current != States.Online)
                return Task.CompletedTask;

            Disconnect();
            return _stateControl.AsyncWait(States.Offline);
        }


        private void OnStateChanged(States from, States to)
        {
            StateChanged();
        }

        private void ClientOnConnected()
        {
            if (_protocolClient.State == ClientStates.Online)
            {
                _stateControl.PushEvent(Events.Connected);
                //RemoteAgent.AlertModel.SubscribeToRemoteAgent();
            }
        }

        private void ClientOnDisconnected()
        {
            if (_protocolClient.State == ClientStates.Offline)
            {
                _stateControl.PushEvent(Events.Disconnected);
                RemoteAgent.UnsubscribeToAlerts();
            }
        }

        private void StartConnecting()
        {
            _protocolClient.Connect(Creds).ContinueWith(_ => ClientOnConnected());
        }

        private void StartDisconnecting()
        {
            _protocolClient.Disconnect().ContinueWith(_ => ClientOnDisconnected());
        }
    }
}
