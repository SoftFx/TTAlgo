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
    public class ConnectionModel
    {
        private static readonly IAlgoCoreLogger logger = CoreLoggerFactory.GetLogger<ConnectionModel>();
        public enum States { Offline, Connecting, Online, Disconnecting, OfflineRetry }
        public enum Events { LostConnection, ConnectFailed, Connected, DoneDisconnecting, OnRequest, OnRetry }

        private StateMachine<States> _stateControl;
        private IStateMachineSync _stateSync;
        private IServerInterop _interop;
        private CancellationTokenSource connectCancelSrc;
        private ConnectionOptions _options;
        private ConnectRequest connectRequest;
        private ConnectRequest lastConnectRequest;
        private Request disconnectRequest;
        private bool wasConnected;

        public ConnectionModel(ConnectionOptions options, IStateMachineSync stateSync = null)
        {
            _options = options;

            Func<bool> canRecconect = () => wasConnected && LastErrorCode != ConnectionErrorCodes.BlockedAccount && LastErrorCode != ConnectionErrorCodes.InvalidCredentials;

            _stateControl = new StateMachine<States>(stateSync);
            _stateSync = _stateControl.SyncContext;
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
            _stateControl.OnEnter(States.Online, () =>
            {
                wasConnected = true;
                Connected?.Invoke();
            });

            _stateControl.StateChanged += (f, t) =>
            {
                StateChanged?.Invoke(f, t);
                logger.Debug("STATE {0} ({1}:{2})", t, CurrentLogin, CurrentServer);
            };

            _stateControl.EventFired += e => logger.Debug("EVENT {0}", e);
        }

        public IFeedServerApi FeedProxy => _interop.FeedApi;
        public ITradeServerApi TradeProxy => _interop.TradeApi;
        public ConnectionErrorInfo LastError { get; private set; }
        public ConnectionErrorCodes LastErrorCode => LastError?.Code ?? ConnectionErrorCodes.None;
        public bool HasError { get { return LastErrorCode != ConnectionErrorCodes.None; } }
        public string CurrentLogin { get; private set; }
        public string CurrentServer { get; private set; }
        public string CurrentProtocol { get; private set; }
        public bool IsConnecting => State == States.Connecting;
        public bool IsOnline => State == States.Online;
        public bool IsOffline => State == States.Offline || State == States.OfflineRetry;
        public event Action Connecting = delegate { };
        public event Action Connected = delegate { };
        public event Action Disconnecting = delegate { };
        public event Action Disconnected = delegate { };
        public event AsyncEventHandler Initalizing; // main thread event
        public event AsyncEventHandler Deinitalizing; // background thread event
        public event Action<States, States> StateChanged;

        public States State => _stateControl.Current;
        public bool IsReconnecting { get; private set; }

        public Task<ConnectionErrorInfo> Connect(string username, string password, string address, bool useSfxProtocol, CancellationToken cToken)
        {
            var request = new ConnectRequest(username, password, address, useSfxProtocol, cToken);

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
            _stateControl.ModifyConditions(() =>
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

            try
            {
                connectCancelSrc = new CancellationTokenSource();
                LastError = null;

                CurrentLogin = request.Usermame;
                CurrentServer = request.Address;
                CurrentProtocol = request.UseSfx ? "SFX" : "FIX";

                request.CancelToken.Register(() =>
                {
                     // TO DO
                });

                if (request.UseSfx)
                    _interop = new SfxInterop(_options);
                else
                    _interop = new FdkInterop(_options);

                _interop.Disconnected += _interop_Disconnected;

                Connecting?.Invoke();

                var result = await _interop.Connect(request.Address, request.Usermame, request.Password, connectCancelSrc.Token).ConfigureAwait(false);
                if (result.Code != ConnectionErrorCodes.None)
                {
                    OnFailedConnect(request, result);
                    return;
                }
                else
                {
                    try
                    {
                        await _stateSync.Synchronized(() => Initalizing.InvokeAsync(this, connectCancelSrc.Token)).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        await Deinitialize();
                        await DisconnectProxy();
                        OnFailedConnect(request, ConnectionErrorInfo.UnknownNoText);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                OnFailedConnect(request, ConnectionErrorInfo.UnknownNoText);
                return;
            }

            _stateControl.PushEvent(Events.Connected);
            request.Complete(ConnectionErrorInfo.Ok);
        }

        private void OnFailedConnect(ConnectRequest requets, ConnectionErrorInfo erroInfo)
        {
            _stateControl.ModifyConditions(() =>
            {
                LastError = erroInfo;
                _stateControl.PushEvent(Events.ConnectFailed);
            });

            requets.Complete(erroInfo);
        }

        private async Task DisconnectProxy()
        {
            try
            {
                _interop.Disconnected -= _interop_Disconnected;

                // wait proxy to stop
                await _interop.Disconnect();
            }
            catch (Exception ex)
            {
                logger.Error("Disconnection error: " + ex.Message);
            }
        }

        private async Task Deinitialize()
        {
            try
            {
                await _stateSync.Synchronized(() => Deinitalizing.InvokeAsync(this));
            }
            catch (Exception ex) { logger.Error(ex); }
        }

        private async void DoDisconnect()
        {
            await Deinitialize();

            try
            {
                // fire disconnecting event
                _stateSync.Synchronized(() => Disconnecting());
            }
            catch (Exception ex) { logger.Error(ex); }

            await DisconnectProxy();

            _stateControl.ModifyConditions(() =>
            {
                if (disconnectRequest != null)
                {
                    disconnectRequest.Complete(ConnectionErrorInfo.Ok);
                    disconnectRequest = null;
                    wasConnected = false;
                    IsReconnecting = false;
                };
                _stateControl.PushEvent(Events.DoneDisconnecting);
            });
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
            public ConnectRequest(string username, string password, string address, bool useSfxProtocol, CancellationToken cToken)
            {
                Usermame = username;
                Password = password;
                Address = address;
                UseSfx = useSfxProtocol;
                CancelToken = cToken;
            }

            public string Usermame { get; }
            public string Password { get; }
            public string Address { get; }
            public bool UseSfx { get; }
            public CancellationToken CancelToken { get; }
        }
    }

    public class ConnectionOptions
    {
        public bool EnableLogs { get; set; }
        public string LogsFolder { get; set; }
    }
}