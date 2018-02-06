using ActorSharp;
using ActorSharp.Lib;
using Machinarium.State;
//using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class ConnectionModel : ActorPart
    {
        private StateMachine<States> _stateControl;
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<ConnectionModel>();
        public enum States { Offline, Connecting, Online, Disconnecting, OfflineRetry }
        public enum Events { LostConnection, ConnectFailed, Connected, DoneDisconnecting, OnRequest, OnRetry }
        //public enum Events { LostConnection, ConnectFailed, Connected, DoneDisconnecting, OnRequest, OnRetry }

        //private StateMachine<States> _stateControl;
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

        public ConnectionModel(ConnectionOptions options) 
        {
            _options = options;

            Func<bool> canRecconect = () => wasConnected && LastErrorCode != ConnectionErrorCodes.BlockedAccount && LastErrorCode != ConnectionErrorCodes.InvalidCredentials;

            _stateControl = new StateMachine<States>(new NullSync());
            _stateControl.AddTransition(States.Offline, () => connectRequest != null, States.Connecting);
            _stateControl.AddTransition(States.OfflineRetry, Events.OnRetry, canRecconect, States.Connecting);
            _stateControl.AddTransition(States.Connecting, Events.Connected,
                () => disconnectRequest != null || connectRequest != null || LastErrorCode != ConnectionErrorCodes.None, States.Disconnecting);
            _stateControl.AddTransition(States.Connecting, Events.Connected, States.Online);
            _stateControl.AddTransition(States.Connecting, Events.ConnectFailed, canRecconect, States.OfflineRetry);
            _stateControl.AddTransition(States.Connecting, Events.ConnectFailed, States.Offline);
            _stateControl.AddTransition(States.Online, Events.LostConnection, States.Disconnecting);
            _stateControl.AddTransition(States.Online, Events.OnRequest, States.Disconnecting);
            _stateControl.AddTransition(States.Disconnecting, Events.DoneDisconnecting, () => connectRequest != null, States.Connecting);
            _stateControl.AddTransition(States.Disconnecting, Events.DoneDisconnecting, canRecconect, States.OfflineRetry);
            _stateControl.AddTransition(States.Disconnecting, Events.DoneDisconnecting, States.Offline);

            _stateControl.AddScheduledEvent(States.OfflineRetry, Events.OnRetry, 10000);

            _stateControl.OnEnter(States.Connecting, DoConnect);
            _stateControl.OnEnter(States.Disconnecting, DoDisconnect);

            _stateControl.StateChanged += (f, t) =>
            {
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
        public IFeedServerApi FeedProxy => _interop.FeedApi;
        public ITradeServerApi TradeProxy => _interop.TradeApi;
        public ConnectionErrorInfo LastError { get; private set; }
        public ConnectionErrorCodes LastErrorCode => LastError?.Code ?? ConnectionErrorCodes.None;
        public bool HasError { get { return LastErrorCode != ConnectionErrorCodes.None; } }
        public string CurrentLogin { get; private set; }
        public string CurrentServer { get; private set; }
        public string CurrentProtocol { get; private set; }
        public event Action InitProxies; // proxies are created but not yet started
        public event Action DeinitProxies; // proxies are about to be stopped
        //public event Action Connecting = delegate { };
        //public event Action Connected = delegate { };
        //public event Action Disconnecting = delegate { };
        //public event Action Disconnected = delegate { };
        public event AsyncEventHandler AsyncInitalizing; // part of connection process, called after proxies are connected
        public event AsyncEventHandler AsyncDeinitalizing; // part of disconnection process, called before proxies are disconnected
        public event AsyncEventHandler AsyncDisconnected; // part of disconnection process, called after proxies are disconnected
        public event Action<States, States> StateChanged;

        public States State => _stateControl.Current;
        public bool IsReconnecting { get; private set; }

        public Task<ConnectionErrorInfo> Connect(string username, string password, string address, bool useSfxProtocol)
        {
            ContextCheck();

            var request = new ConnectRequest(username, password, address, useSfxProtocol);

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
                if (State == States.Offline)
                    completion = Task.FromResult(ConnectionErrorCodes.None);
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
            ContextInvoke(()=>
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
            CurrentProtocol = request.UseSfx ? "SFX" : "FIX";

            //await ChangeState(States.Connecting);

            try
            {
                if (request.UseSfx)
                    _interop = new SfxInterop(_options);
                else
                    _interop = new FdkInterop(_options);

                _interop.Disconnected += _interop_Disconnected;

                InitProxies?.Invoke();

                var result = await _interop.Connect(request.Address, request.Usermame, request.Password, connectCancelSrc.Token);
                if (result.Code != ConnectionErrorCodes.None)
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

                    //try
                    //{
                        
                    //}
                    //catch (Exception ex)
                    //{
                    //    logger.Error(ex);
                    //    await Deinitialize();
                    //    await DisconnectProxy();
                    //    await OnConnectFailed(request, ConnectionErrorInfo.UnknownNoText);
                    //    return;
                    //}
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
            LastError = erroInfo;
            _stateControl.PushEvent(Events.ConnectFailed);

            requets.Complete(erroInfo);
        }

        private async Task Deinitialize()
        {
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
                    await AsyncDisconnected.InvokeAsync(this);
            }
            catch (Exception ex) { logger.Error(ex); }

            wasInitFired = false;
        }

        private async void DoDisconnect()
        {
            await Deinitialize();

            //try
            //{
            //    // fire disconnecting event
            //    //_stateSync.Synchronized(() => Disconnecting());
            //}
            //catch (Exception ex) { logger.Error(ex); }

            if (disconnectRequest != null)
            {
                disconnectRequest.Complete(ConnectionErrorInfo.Ok);
                disconnectRequest = null;
                wasConnected = false;
                IsReconnecting = false;
            };

            _stateControl.PushEvent(Events.DoneDisconnecting);
        }

        #region State management

        //private async Task OnDisconnectComplete()
        //{
        //    if (connectRequest != null)
        //        DoConnect();
        //    else
        //        await ChangeState(States.Online);
        //}

        //private async Task OnConnectComplete()
        //{
        //    if (connectRequest != null || disconnectRequest != null || lostConnection)
        //        DoDisconnect();
        //    else
        //        await ChangeState(States.Online);
        //}

        //private async Task OnConnectFailed(ConnectRequest requets, ConnectionErrorInfo error)
        //{
        //    if (connectRequest != null)
        //        DoDisconnect();
        //    else
        //    {
        //        await ChangeState(States.Offline);

        //        if (disconnectRequest != null)
        //        {
        //            disconnectRequest.Complete(new ConnectionErrorInfo(ConnectionErrorCodes.None));
        //            disconnectRequest = null;
        //        }
        //    }

        //    requets.Complete(error);
        //}

        //private void OnDisconnected(ConnectionErrorInfo info)
        //{
        //    if (State == States.Connecting)
        //        lostConnection = true;
        //    else if (State == States.Online)
        //        DoDisconnect();
        //}

        //private async Task ChangeState(States newState)
        //{
        //    var oldState = State;
        //    State = newState;

        //    try
        //    {
        //        StateChanged?.Invoke(oldState, newState);
        //    }
        //    catch (Exception ex) { logger.Error(ex); }
            
        //    var stateInfo = new StateInfo();
        //    stateInfo.State = newState;
        //    stateInfo.Login = CurrentLogin;
        //    stateInfo.Server = CurrentServer;
        //    stateInfo.Protocol = CurrentProtocol;
        //    stateInfo.LastError = LastError;

        //    try
        //    {
        //        await _stateListeners.Fire(stateInfo);
        //    }
        //    catch (Exception ex) { logger.Error(ex); }
        //}

        #endregion

        public class Handler : Handler<ConnectionModel>
        {
            private ActorCallback _initListener;
            private ActorCallback _deinitListener;
            private ActorListener<StateInfo> _stateListener;

            public Handler(Ref<ConnectionModel> actorRef) : base(actorRef) { }

            public States State { get; private set; }
            public bool IsConnecting => State == States.Connecting;
            public bool IsOnline => State == States.Online;
            public bool IsOffline => State == States.Offline || State == States.OfflineRetry;
            public ConnectionErrorInfo LastError { get; private set; }
            public ConnectionErrorCodes LastErrorCode => LastError?.Code ?? ConnectionErrorCodes.None;
            public bool HasError { get { return LastErrorCode != ConnectionErrorCodes.None; } }
            public bool IsReconnecting { get; private set; }
            public string CurrentLogin { get; private set; }
            public string CurrentServer { get; private set; }
            public string CurrentProtocol { get; private set; }

            public event AsyncEventHandler Initalizing;
            public event AsyncEventHandler Deinitalizing;
            public event Action Connecting = delegate { };
            public event Action Connected = delegate { };
            public event Action Disconnecting = delegate { };
            public event Action Disconnected = delegate { };
            public event Action<States, States> StateChanged;

            public async virtual Task Start()
            {
                var handlerRef = this.GetRef();

                _initListener = ActorCallback.Create(FireInitEvent);
                _deinitListener = ActorCallback.Create(FireDeinitEvent);
                _stateListener = new ActorListener<StateInfo>(a =>
                {
                    var oldState = State;
                    State = a.State;
                    LastError = a.LastError;
                    CurrentLogin = a.Login;
                    CurrentServer = a.Server;
                    CurrentProtocol = a.Protocol;
                    StateChanged?.Invoke(oldState, State);
                });

                State = await Actor.Call(a =>
                {
                    a._stateListeners.Add(_stateListener.Ref);
                    a._initListeners.Add(_initListener);
                    a._deinitListeners.Add(_deinitListener);
                    return a.State;
                });
            }

            public Task<ConnectionErrorInfo> Connect(string userName, string password, string address, bool useSfxProtocol, CancellationToken cToken)
            {
                return Actor.Call(a => a.Connect(userName, password, address, useSfxProtocol));
            }

            public Task Disconnect()
            {
                return Actor.Call(a => a.Disconnect());
            }

            public virtual Task Stop()
            {
                return Actor.Call(a =>
                {
                    a._stateListeners.Remove(_stateListener.Ref);
                    a._initListeners.Remove(_initListener);
                    a._deinitListeners.Remove(_deinitListener);
                });
            }

            internal Task FireInitEvent()
            {
                return Initalizing.InvokeAsync(this, CancellationToken.None);
            }

            internal Task FireDeinitEvent()
            {
                return Deinitalizing.InvokeAsync(this, CancellationToken.None);
            }

            private void UpdateState(States newState)
            {
                State = newState;
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
            public ConnectRequest(string username, string password, string address, bool useSfxProtocol)
            {
                Usermame = username;
                Password = password;
                Address = address;
                UseSfx = useSfxProtocol;
                //CancelToken = cToken;
            }

            public string Usermame { get; }
            public string Password { get; }
            public string Address { get; }
            public bool UseSfx { get; }
            //public CancellationToken CancelToken { get; }
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
        public bool EnableLogs { get; set; }
        public string LogsFolder { get; set; }
    }
}