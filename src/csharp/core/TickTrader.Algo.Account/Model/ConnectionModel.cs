using ActorSharp;
using ActorSharp.Lib;
using Machinarium.State;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class ConnectionModel : ActorPart, IStateMachineSync
    {
        private StateMachine<States> _stateControl;
        private IAlgoLogger logger;
        private readonly string _loggerId;
        private readonly ServerInteropFactory _accFactory;
        public enum States { Offline, Connecting, Online, Disconnecting, OfflineRetry }
        public enum Events { LostConnection, ConnectFailed, Connected, DoneDisconnecting, OnRequest, OnRetry, StopRetryRequested }
        //public enum DiconnectReasons { ConnectSequenceFailed, ClientRequest, LostConnection }

        private IServerInterop _interop;
        private CancellationTokenSource connectCancelSrc;
        private ConnectionOptions _options;
        private ConnectRequest connectRequest;
        private ConnectRequest lastConnectRequest;
        private Request disconnectRequest;
        private bool wasConnected;
        private bool wasInitFired;
        private ActorEvent _initListeners = new ActorEvent();
        private ActorEvent _deinitListeners = new ActorEvent();
        private ActorEvent<StateInfo> _stateListeners = new ActorEvent<StateInfo>();

        public ConnectionModel(ServerInteropFactory accFactory, ConnectionOptions options, string loggerId)
        {
            _loggerId = loggerId;
            logger = AlgoLoggerFactory.GetLogger<ConnectionModel>(loggerId);
            _options = options;
            _accFactory = accFactory;

            Func<bool> canRecconect = () => _options.AutoReconnect && wasConnected
                && LastErrorCode != ConnectionErrorInfo.Types.ErrorCode.BlockedAccount
                && LastErrorCode != ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials;

            _stateControl = new StateMachine<States>(this);
            _stateControl.AddTransition(States.Offline, () => connectRequest != null, States.Connecting);
            _stateControl.AddTransition(States.OfflineRetry, Events.OnRetry, canRecconect, States.Connecting);
            _stateControl.AddTransition(States.OfflineRetry, Events.StopRetryRequested, States.Offline);
            _stateControl.AddTransition(States.Connecting, Events.Connected,
                () => disconnectRequest != null || connectRequest != null || LastErrorCode != ConnectionErrorInfo.Types.ErrorCode.NoConnectionError, States.Disconnecting);
            _stateControl.AddTransition(States.Connecting, Events.Connected, States.Online);
            _stateControl.AddTransition(States.Connecting, Events.ConnectFailed, canRecconect, States.OfflineRetry);
            _stateControl.AddTransition(States.Connecting, Events.ConnectFailed, States.Offline);
            _stateControl.AddTransition(States.Online, Events.LostConnection, States.Disconnecting);
            _stateControl.AddTransition(States.Online, Events.OnRequest, States.Disconnecting);
            _stateControl.AddTransition(States.Disconnecting, Events.DoneDisconnecting, () => connectRequest != null, States.Connecting);
            _stateControl.AddTransition(States.Disconnecting, Events.DoneDisconnecting, canRecconect, States.OfflineRetry);
            _stateControl.AddTransition(States.Disconnecting, Events.DoneDisconnecting, States.Offline);

            _stateControl.AddScheduledEvent(States.OfflineRetry, Events.OnRetry, 5000);

            _stateControl.OnEnter(States.Connecting, DoConnect);
            _stateControl.OnEnter(States.Disconnecting, () => DoDisconnect(canRecconect()));

            _stateControl.StateChanged += (f, t) =>
            {
                ContextCheck();

                StateChanged?.Invoke(f, t);
                logger.Debug("STATE {0} ({1}:{2})", t, CurrentLogin, CurrentServer);

                var stateInfo = new StateInfo();
                stateInfo.State = t;
                stateInfo.Login = CurrentLogin;
                stateInfo.Server = CurrentServer;
                stateInfo.Protocol = CurrentProtocol;
                stateInfo.LastError = LastError;

                _stateListeners.FireAndForget(stateInfo);
            };

            _stateControl.EventFired += e => logger.Debug("EVENT {0}", e);
        }

        protected override void ActorInit()
        {
            Ref = this.GetRef();
        }

        public Ref<ConnectionModel> Ref { get; private set; }
        internal IFeedServerApi FeedProxy => _interop.FeedApi;
        internal ITradeServerApi TradeProxy => _interop.TradeApi;
        public ConnectionErrorInfo LastError { get; private set; }
        public ConnectionErrorInfo.Types.ErrorCode LastErrorCode => LastError?.Code ?? ConnectionErrorInfo.Types.ErrorCode.NoConnectionError;
        public bool HasError { get { return LastErrorCode != ConnectionErrorInfo.Types.ErrorCode.NoConnectionError; } }
        public string CurrentLogin { get; private set; }
        public string CurrentServer { get; private set; }
        public string CurrentProtocol { get; private set; }
        public event Action InitProxies; // proxies are created but not yet started
        public event Action DeinitProxies; // proxies are about to be stopped
        public event AsyncEventHandler AsyncInitalizing; // part of connection process, called after proxies are connected
        public event AsyncEventHandler AsyncDeinitalizing; // part of disconnection process, called before proxies are disconnected
        public event AsyncEventHandler AsyncDisconnected; // part of disconnection process, called after proxies are disconnected
        public event Action<States, States> StateChanged;

        public States State => _stateControl.Current;
        public bool IsReconnecting { get; private set; }

        public Task<ConnectionErrorInfo> Connect(string username, string password, string address)
        {
            ContextCheck();

            var request = new ConnectRequest(username, password, address);

            _stateControl.ModifyConditions(() =>
            {
                if (connectRequest != null)
                    connectRequest.Cancel();

                connectRequest = request;

                if (State == States.Connecting)
                    connectCancelSrc.Cancel();

                wasConnected = false;

                _stateControl.PushEvent(Events.OnRequest);
            });

            return request.Completion;
        }

        public Task Disconnect()
        {
            ContextCheck();

            Task completion = null;

            _stateControl.ModifyConditions(() =>
            {
                wasConnected = false;

                if (State == States.Offline)
                    completion = Task.FromResult(ConnectionErrorInfo.Types.ErrorCode.NoConnectionError);
                else if (State == States.OfflineRetry)
                {
                    _stateControl.PushEvent(Events.StopRetryRequested);
                    completion = Task.FromResult(ConnectionErrorInfo.Types.ErrorCode.NoConnectionError);
                }
                else
                {
                    if (connectRequest != null)
                    {
                        connectRequest.Cancel();
                        connectRequest = null;
                    }

                    if (State == States.Connecting)
                        connectCancelSrc.Cancel();

                    if (disconnectRequest == null)
                    {
                        disconnectRequest = new Request();
                        _stateControl.PushEvent(Events.OnRequest);
                    }

                    completion = disconnectRequest.Completion;
                }
            });

            return completion;
        }

        private void _interop_Disconnected(IServerInterop sender, ConnectionErrorInfo errInfo)
        {
            ContextInvoke(() =>
            {
                if (sender == _interop && (State == States.Online || State == States.Connecting))
                {
                    LastError = errInfo;
                    _stateControl.PushEvent(Events.LostConnection);
                }
            });
        }

        private async void DoConnect()
        {
            ContextCheck();

            var request = connectRequest;
            if (request == null)
            {
                // using old request
                IsReconnecting = true;
                request = lastConnectRequest;
            }
            else
            {
                // new request
                wasConnected = false;
                IsReconnecting = false;
                lastConnectRequest = connectRequest;
                connectRequest = null;
            }

            connectCancelSrc = new CancellationTokenSource();
            LastError = null;

            CurrentLogin = request.Usermame;
            CurrentServer = request.Address;
            CurrentProtocol = "SFX";

            try
            {
                var options = _options.WithNewLogsFolder(Path.Combine(_options.LogsFolder, CurrentProtocol, $"{request.Address} - {request.Usermame}"));
                _interop = _accFactory(options, _loggerId);

                _interop.Disconnected += _interop_Disconnected;

                InitProxies?.Invoke();

                var result = await _interop.Connect(request.Address, request.Usermame, request.Password, connectCancelSrc.Token);
                if (result.Code != ConnectionErrorInfo.Types.ErrorCode.NoConnectionError)
                {
                    await Deinitialize();
                    OnFailedConnect(request, result);
                    return;
                }
                else
                {
                    wasInitFired = true;
                    await AsyncInitalizing.InvokeAsync(this, connectCancelSrc.Token);
                    await _initListeners.Invoke();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                await Deinitialize();
                OnFailedConnect(request, ConnectionErrorInfo.UnknownNoText);
                return;
            }

            wasConnected = true;
            _stateControl.PushEvent(Events.Connected);
            request.Complete(ConnectionErrorInfo.Ok);
        }

        private void OnFailedConnect(ConnectRequest requets, ConnectionErrorInfo erroInfo)
        {
            ContextCheck();

            LastError = erroInfo;
            _stateControl.PushEvent(Events.ConnectFailed);

            requets.Complete(erroInfo);
        }

        private async Task Deinitialize()
        {
            ContextCheck();

            _interop.Disconnected -= _interop_Disconnected;

            try
            {
                if (wasInitFired)
                {
                    await AsyncDeinitalizing.InvokeAsync(this);
                    await _deinitListeners.Invoke();
                }
            }
            catch (Exception ex) { logger.Error(ex); }

            try
            {
                DeinitProxies?.Invoke();
            }
            catch (Exception ex) { logger.Error(ex); }

            try
            {
                // wait proxy to stop
                await _interop.Disconnect();
            }
            catch (Exception ex)
            {
                logger.Error("Disconnection error: " + ex.Message);
            }

            try
            {
                if (wasInitFired)
                    await AsyncDisconnected.InvokeAsync(this, CancellationToken.None);
            }
            catch (Exception ex) { logger.Error(ex); }

            wasInitFired = false;
        }

        private async void DoDisconnect(bool canRecconect)
        {
            await Deinitialize();

            if (disconnectRequest != null)
            {
                disconnectRequest.Complete(ConnectionErrorInfo.Ok);
                disconnectRequest = null;
                wasConnected = false;
                IsReconnecting = false;
            };

            _stateControl.PushEvent(Events.DoneDisconnecting);
        }

        #region IStateMachineSync

        void IStateMachineSync.Synchronized(Action syncAction)
        {
            ContextInvoke(syncAction);
        }

        T IStateMachineSync.Synchronized<T>(Func<T> syncAction)
        {
            T result = default(T);
            ContextInvoke(() => result = syncAction());
            return result;
        }

        #endregion

        public class Handler : Handler<ConnectionModel>
        {
            private ActorCallback _initListener;
            private ActorCallback _deinitListener;
            private ActorCallback<StateInfo> _stateListener;

            public Handler(Ref<ConnectionModel> actorRef) : base(actorRef) { }

            public States State { get; private set; }
            public bool IsConnecting => State == States.Connecting;
            public bool IsOnline => State == States.Online;
            public bool IsOffline => State == States.Offline || State == States.OfflineRetry;
            public ConnectionErrorInfo LastError { get; private set; }
            public ConnectionErrorInfo.Types.ErrorCode LastErrorCode => LastError?.Code ?? ConnectionErrorInfo.Types.ErrorCode.NoConnectionError;
            public bool HasError { get { return LastErrorCode != ConnectionErrorInfo.Types.ErrorCode.NoConnectionError; } }
            public bool IsReconnecting { get; private set; }
            public string CurrentLogin { get; private set; }
            public string CurrentServer { get; private set; }
            public string CurrentProtocol { get; private set; }

            public event AsyncEventHandler Initalizing;
            public event AsyncEventHandler Deinitalizing;
            //public event Action Connecting = delegate { };
            public event Action Connected = delegate { };
            //public event Action Disconnecting = delegate { };
            public event Action Disconnected = delegate { };
            public event Action<States, States> StateChanged;

            public async virtual Task OpenHandler()
            {
                var handlerRef = this.GetRef();

                _initListener = ActorCallback.CreateAsync(FireInitEvent);
                _deinitListener = ActorCallback.CreateAsync(FireDeinitEvent);
                _stateListener = ActorCallback.Create<StateInfo>(UpdateState);

                State = await Actor.Call(a =>
                {
                    a._stateListeners.Add(_stateListener);
                    a._initListeners.Add(_initListener);
                    a._deinitListeners.Add(_deinitListener);
                    return a.State;
                });
            }

            public Task<ConnectionErrorInfo> Connect(string userName, string password, string address, CancellationToken cToken)
            {
                return Actor.Call(a => a.Connect(userName, password, address));
            }

            public Task Disconnect()
            {
                return Actor.Call(a => a.Disconnect());
            }

            public virtual Task CloseHandler()
            {
                return Actor.Call(a =>
                {
                    a._stateListeners.Remove(_stateListener);
                    a._initListeners.Remove(_initListener);
                    a._deinitListeners.Remove(_deinitListener);
                });
            }

            private Task FireInitEvent()
            {
                return Initalizing.InvokeAsync(this, CancellationToken.None);
            }

            private Task FireDeinitEvent()
            {
                return Deinitalizing.InvokeAsync(this, CancellationToken.None);
            }

            private void UpdateState(StateInfo info)
            {
                var oldState = State;
                State = info.State;
                LastError = info.LastError;
                CurrentLogin = info.Login;
                CurrentServer = info.Server;
                CurrentProtocol = info.Protocol;

                StateChanged?.Invoke(oldState, State);

                if (State == States.Online)
                    Connected?.Invoke();
                else if (State == States.Offline)
                    Disconnected?.Invoke();
            }
        }

        private class Request
        {
            private TaskCompletionSource<ConnectionErrorInfo> _src = new TaskCompletionSource<ConnectionErrorInfo>();

            public Task<ConnectionErrorInfo> Completion => _src.Task;

            public void Complete(ConnectionErrorInfo errInfo)
            {
                _src.TrySetResult(errInfo);
            }

            public void Cancel()
            {
                _src.SetCanceled();
            }
        }

        private class ConnectRequest : Request
        {
            public ConnectRequest(string username, string password, string address)
            {
                Usermame = username;
                Password = password;
                Address = address;
            }

            public string Usermame { get; }
            public string Password { get; }
            public string Address { get; }
        }

        private class StateInfo
        {
            public States State { get; set; }
            public ConnectionErrorInfo LastError { get; set; }
            public string Login { get; set; }
            public string Server { get; set; }
            public string Protocol { get; set; }
        }
    }

    public class ConnectionOptions
    {
        public bool AutoReconnect { get; set; }
        public bool EnableLogs { get; set; }
        public string LogsFolder { get; set; }

        public AppType Type { get; set; }

        public ConnectionOptions WithNewLogsFolder(string logsFolder)
        {
            return new ConnectionOptions { AutoReconnect = AutoReconnect, EnableLogs = EnableLogs, LogsFolder = logsFolder, Type = Type };
        }
    }

    public enum AppType
    {
        BotTerminal = 0,
        BotAgent = 1,
    }
}