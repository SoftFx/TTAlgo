using Machinarium.State;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentConnectionManager
    {
        private enum States { Offline, Connecting, Online, Disconnecting, WaitReconnect }


        private enum Events { ConnectionStarted, Connected, ConnectionLost, Disconnected, Reconnect }


        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        private ProtocolClient _protocolClient;
        private StateMachine<States> _stateControl;
        private bool _needReconnect;
        private bool _hasRequest;


        public string Server => _protocolClient.SessionSettings.ServerAddress;

        public ClientStates State => _protocolClient.State;

        public string LastError => _protocolClient.LastError;

        public string Status => string.IsNullOrEmpty(_protocolClient.LastError) ? $"{_protocolClient.State}" : $"{_protocolClient.State} - {_protocolClient.LastError}";

        public BotAgentStorageEntry Creds { get; private set; }

        public BotAgentModel BotAgent { get; }


        public event Action StateChanged = delegate { };


        public BotAgentConnectionManager(BotAgentStorageEntry botAgentCreds)
        {
            Creds = botAgentCreds;

            BotAgent = new BotAgentModel();
            _protocolClient = new ProtocolClient(BotAgent);

            _protocolClient.StateMachine.StateChanged += ClientOnStateChanged;
            _protocolClient.Connecting += ClientOnConnecting;
            _protocolClient.Connected += ClientOnConnected;
            _protocolClient.Disconnecting += ClientOnDisconnecting;
            _protocolClient.Disconnected += ClientOnDisconnected;

            _stateControl = new StateMachine<States>(new NullSync());
            _stateControl.AddTransition(States.Offline, Events.ConnectionStarted, States.Connecting);
            _stateControl.AddTransition(States.Connecting, Events.Connected, States.Online);
            _stateControl.AddTransition(States.Connecting, Events.ConnectionLost, States.Disconnecting);
            _stateControl.AddTransition(States.Connecting, Events.Disconnected, States.Offline);
            _stateControl.AddTransition(States.Online, Events.ConnectionLost, States.Disconnecting);
            _stateControl.AddTransition(States.Online, Events.Disconnected, States.Offline);
            _stateControl.AddTransition(States.Disconnecting, Events.Disconnected, States.Offline);

            _stateControl.AddTransition(States.WaitReconnect, Events.Reconnect, States.Connecting);
            _stateControl.AddTransition(States.Offline, () => _needReconnect, States.WaitReconnect);
            _stateControl.AddScheduledEvent(States.WaitReconnect, Events.Reconnect, 10000);
        }


        public void Connect()
        {
            _needReconnect = true;
            if (!_hasRequest)
            {
                StartConnecting();
            }
        }

        public Task WaitConnect()
        {
            Connect();
            return _stateControl.AsyncWait(s => s == States.Offline || s == States.Online);
        }

        public void Disconnect()
        {
            _needReconnect = false;
            StartDisconnecting();
        }

        public Task WaitDisconnect()
        {
            Disconnect();
            return _stateControl.AsyncWait(States.Offline);
        }


        private void ClientOnStateChanged(ClientStates from, ClientStates to)
        {
            StateChanged();
        }

        private void ClientOnConnecting()
        {
            _stateControl.PushEvent(Events.ConnectionStarted);
        }

        private void ClientOnConnected()
        {
            _stateControl.PushEvent(Events.Connected);
        }

        private void ClientOnDisconnecting()
        {
            _stateControl.PushEvent(Events.ConnectionLost);
        }

        private void ClientOnDisconnected()
        {
            BotAgent.ClearCache();
            _stateControl.PushEvent(Events.Disconnected);
        }

        private async void StartConnecting()
        {
            _hasRequest = true;
            await _stateControl.AsyncWait(s => s == States.Offline || s == States.Online);
            if (_stateControl.Current == States.Online)
            {
                _protocolClient.TriggerDisconnect();
                await _stateControl.AsyncWait(States.Offline);
            }
            _hasRequest = false;
            _protocolClient.TriggerConnect(Creds.ToClientSettings());
        }

        private async void StartDisconnecting()
        {
            await _stateControl.AsyncWait(s => s == States.Offline || s == States.Online);
            if (_stateControl.Current == States.Online)
            {
                _protocolClient.TriggerDisconnect();
            }
        }
    }
}
