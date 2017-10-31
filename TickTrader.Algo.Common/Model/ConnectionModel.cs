using Machinarium.ActorModel;
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
        public enum States { Offline, Connecting, Online, Disconnecting }

        private States _state;
        private ActorBlock _primaryQueue;
        private ActorBlock _connectQueue;
        private IStateMachineSync _stateSync;
        private IServerInterop _interop;
        private CancellationTokenSource connectCancelSrc;
        private TaskCompletionSource<object> connectedSrc;
        private ConnectionOptions _options;

        public ConnectionModel(ConnectionOptions options, IStateMachineSync stateSync = null)
        {
            _options = options;
            _stateSync = stateSync ?? new NullSync();

            _primaryQueue = new QueueBlock();
            _connectQueue = new PushOutBlock(_primaryQueue.Actor);
        }

        public IFeedServerApi FeedProxy => _interop.FeedApi;
        public ITradeServerApi TradeProxy => _interop.TradeApi;
        public ConnectionErrorCodes LastError { get; private set; }
        public bool HasError { get { return LastError != ConnectionErrorCodes.None; } }
        public string CurrentLogin { get; private set; }
        public string CurrentServer { get; private set; }
        public bool IsConnecting => State == States.Connecting;
        public event Action Connecting = delegate { };
        public event Action Connected = delegate { };
        public event Action Disconnecting = delegate { };
        public event Action Disconnected = delegate { };
        public event AsyncEventHandler Initalizing; // main thread event
        public event AsyncEventHandler Deinitalizing; // background thread event
        public event Action<States, States> StateChanged;

        public States State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    var oldVal = _state;
                    _state = value;
                    StateChanged(oldVal, value);
                }
            }
        }

        public async Task<ConnectionErrorCodes> Connect(string username, string password, string address, bool useSfxProtocol, CancellationToken cToken)
        {
            using (await _connectQueue.GetLock(cToken))
            {
                try
                {
                    if (State != States.Offline)
                        throw new InvalidOperationException("Invalid state: " + State);

                    connectedSrc = new TaskCompletionSource<object>();
                    connectCancelSrc = new CancellationTokenSource();

                    if (useSfxProtocol)
                        _interop = new SfxInterop();
                    else
                        _interop = new FdkInterop(_options);

                    _stateSync.Synchronized(() =>
                    {
                        State = States.Connecting;
                        LastError = ConnectionErrorCodes.None;
                        CurrentLogin = username;
                        CurrentServer = address;

                        Connecting?.Invoke();
                    });

                    var result = await _interop.Connect(address, username, password, connectCancelSrc.Token);
                    if(result != ConnectionErrorCodes.None)
                        return ConnectionErrorCodes.None;
                    await _stateSync.Synchronized(() => Initalizing.InvokeAsync(this, cToken));

                    _stateSync.Synchronized(() =>
                    {
                        State = States.Online;
                        Connected();
                    });

                    return ConnectionErrorCodes.None;
                }
                catch (Exception ex)
                {
                    _stateSync.Synchronized(() =>
                    {
                        State = States.Offline;
                    });

                    logger.Error(ex);
                    return ConnectionErrorCodes.Unknown;
                }
                finally
                {
                    connectedSrc.SetResult(null);
                }
            }
        }

        public async Task DisconnectAsync()
        {
            using (await _primaryQueue.GetLock())
            {
                if (State == States.Offline)
                    return;

                State = States.Disconnecting;

                connectCancelSrc.Cancel();

                // wait connect routine to stop
                await connectedSrc.Task;

                try
                {
                    await _stateSync.Synchronized(() => Deinitalizing.InvokeAsync(this));
                }
                catch (Exception ex) { logger.Error(ex); }

                try
                {
                    // fire disconnecting event
                    Disconnecting();
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

                State = States.Offline;
                _interop.Disconnected -= _interop_Disconnected;
                _interop = null;
            }
        }

        private async void _interop_Disconnected(IServerInterop sender, ConnectionErrorCodes code)
        {
            using (await _primaryQueue.GetLock())
            {
                if ((State == States.Online || State == States.Connecting) && sender == _interop)
                    await DisconnectAsync();
            }
        }
    }

    public class ConnectionOptions
    {
        public bool EnableFixLogs { get; set; }
    }
}