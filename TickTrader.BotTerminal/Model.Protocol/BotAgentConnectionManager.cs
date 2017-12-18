using Machinarium.Qnil;
using Machinarium.State;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentConnectionManager : IBotAgentClient
    {
        private enum States { Offline, Connecting, Online, Disconnecting }


        private enum Events { ConnectionStarted, Connected, ConnectionLost, Disconnected }


        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();


        private DynamicList<AccountModelEntity> _accounts;
        private DynamicList<BotModelEntity> _bots;
        private DynamicList<PackageModelEntity> _packages;
        private ProtocolClient _protocolClient;
        private IDelayCounter _connectionDelay;
        private StateMachine<States> _stateControl;
        private CancellationTokenSource _reconnectTokenSrc;
        private bool _needReconnect;
        private bool _hasRequest;
        private ISyncContext _syncContext;


        public string Server => _protocolClient.SessionSettings.ServerAddress;

        public ClientStates State => _protocolClient.State;

        public string LastError => _protocolClient.LastError;

        public string Status => string.IsNullOrEmpty(_protocolClient.LastError) ? $"{_protocolClient.State}" : $"{_protocolClient.State} - {_protocolClient.LastError}";

        public BotAgentStorageEntry Creds { get; private set; }

        public IObservableListSource<AccountModelEntity> Accounts { get; }

        public IObservableListSource<BotModelEntity> Bots { get; }

        public IObservableListSource<PackageModelEntity> Packages { get; }


        public event Action StateChanged = delegate { };


        public BotAgentConnectionManager(BotAgentStorageEntry botAgentCreds)
        {
            Creds = botAgentCreds;

            _syncContext = new DispatcherSync();

            _accounts = new DynamicList<AccountModelEntity>();
            _bots = new DynamicList<BotModelEntity>();
            _packages = new DynamicList<PackageModelEntity>();

            Accounts = _accounts.AsObservable();
            Bots = _bots.AsObservable();
            Packages = _packages.AsObservable();

            _protocolClient = new ProtocolClient(this);
            _connectionDelay = new ConnectionDelayCounter(TimeSpan.FromSeconds(2), TimeSpan.FromMinutes(1));

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
            _reconnectTokenSrc?.Cancel();
            _reconnectTokenSrc = null;
            _stateControl.PushEvent(Events.ConnectionStarted);
        }

        private void ClientOnConnected()
        {
            _connectionDelay.Reset();
            _stateControl.PushEvent(Events.Connected);
        }

        private void ClientOnDisconnecting()
        {
            _stateControl.PushEvent(Events.ConnectionLost);
        }

        private void ClientOnDisconnected()
        {
            ClearCache();
            _stateControl.PushEvent(Events.Disconnected);
            if (_needReconnect)
            {
                _reconnectTokenSrc = new CancellationTokenSource();
                RecconectAfter(_reconnectTokenSrc.Token, _connectionDelay.Next());
            }
        }

        private async void RecconectAfter(CancellationToken token, TimeSpan delay)
        {
            try
            {
                await Task.Delay(delay, token);
                token.ThrowIfCancellationRequested();
                if (!_hasRequest)
                {
                    StartConnecting();
                }
            }
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }
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

        private void ClearCache()
        {
            _syncContext.Invoke(() =>
            {
                _accounts.Clear();
                _bots.Clear();
                _packages.Clear();
            });
        }


        #region IBotAgentClient implementation

        public void InitAccountList(AccountListReportEntity report)
        {
            _syncContext.Invoke(() =>
            {
                _accounts.Clear();
                foreach (var acc in report.Accounts)
                {
                    _accounts.Add(acc);
                }
            });
        }

        public void InitBotList(BotListReportEntity report)
        {
            _syncContext.Invoke(() =>
            {
                _bots.Clear();
                foreach (var acc in report.Bots)
                {
                    _bots.Add(acc);
                }
            });
        }

        public void InitPackageList(PackageListReportEntity report)
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                foreach (var package in report.Packages)
                {
                    _packages.Add(package);
                }
            });
        }

        public void UpdateAccount(AccountModelUpdateEntity update)
        {
            _syncContext.Invoke(() =>
            {
                var acc = update.Item;
                switch (update.Type)
                {
                    case UpdateType.Added:
                        _accounts.Add(acc);
                        break;
                    case UpdateType.Updated:
                        var i = _accounts.Values.IndexOf(a => acc.Login == a.Login && acc.Server == a.Server);
                        if (i >= 0)
                        {
                            _accounts[i] = update.Item;
                        }
                        break;
                    case UpdateType.Removed:
                        var j = _accounts.Values.IndexOf(a => acc.Login == a.Login && acc.Server == a.Server);
                        if (j >= 0)
                        {
                            _accounts.RemoveAt(j);
                        }
                        break;
                }
            });
        }

        public void UpdateBot(BotModelUpdateEntity update)
        {
            _syncContext.Invoke(() =>
            {
                var bot = update.Item;
                switch (update.Type)
                {
                    case UpdateType.Added:
                        _bots.Add(bot);
                        break;
                    case UpdateType.Updated:
                        var i = _bots.Values.IndexOf(b => bot.InstanceId == b.InstanceId);
                        if (i >= 0)
                        {
                            _bots[i] = bot;
                        }
                        break;
                    case UpdateType.Removed:
                        var j = _bots.Values.IndexOf(b => bot.InstanceId == b.InstanceId);
                        if (j >= 0)
                        {
                            _bots.RemoveAt(j);
                        }
                        break;
                }
            });
        }

        public void UpdatePackage(PackageModelUpdateEntity update)
        {
            _syncContext.Invoke(() =>
            {
                var package = update.Item;
                switch (update.Type)
                {
                    case UpdateType.Added:
                        _packages.Add(update.Item);
                        break;
                    case UpdateType.Updated:
                        var i = _packages.Values.IndexOf(p => package.Name == p.Name);
                        if (i >= 0)
                        {
                            _packages[i] = package;
                        }
                        break;
                    case UpdateType.Removed:
                        var j = _packages.Values.IndexOf(p => package.Name == p.Name);
                        if (j >= 0)
                        {
                            _packages.RemoveAt(j);
                        }
                        break;
                }
            });
        }

        #endregion
    }
}
