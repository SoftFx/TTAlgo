using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.DS.Repository;
using TickTrader.DedicatedServer.Infrastructure;

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract(Name = "tradeBot", Namespace = "")]
    public class TradeBotModel : ITradeBot
    {
        private ILogger _log;
        private object _syncObj;
        private ClientModel _client;
        private Task _stopTask;
        private PluginExecutor executor;
        private BotLog _botLog;
        private AlgoPluginRef _ref;
        private ListenerProxy _stopListener;
        private PackageStorage _packageRepo;

        public TradeBotModel(string id, PluginKey key, PluginConfig cfg)
        {
            Id = id;
            Config = cfg;
            PackageName = key.PackageName;
            Descriptor = key.DescriptorId;
        }

        [DataMember(Name = "configuration")]
        public PluginConfig Config { get; private set; }
        [DataMember(Name = "id")]
        public string Id { get; private set; }
        [DataMember(Name = "package")]
        public string PackageName { get; private set; }
        [DataMember(Name = "descriptor")]
        public string Descriptor { get; private set; }
        [DataMember(Name = "running")]
        public bool IsRunning { get; private set; }

        public BotStates State { get; private set; }
        public PackageModel Package { get; private set; }
        public Exception Fault { get; private set; }
        public string FaultMessage { get; private set; }
        public IAccount Account => _client;
        public IBotLog Log => _botLog;

        public event Action<TradeBotModel> StateChanged;
        public event Action<TradeBotModel> IsRunningChanged;
        public event Action<TradeBotModel> ConfigurationChanged;

        public void Init(ClientModel client, ILogger log, object syncObj, PackageStorage packageRepo, IAlgoGuiMetadata tradeMetadata)
        {
            _syncObj = syncObj;
            _client = client;
            _log = log;
            _packageRepo = packageRepo;
            UpdatePackage();

            _packageRepo.PackageChanged += _packageRepo_PackageChanged;
            _client.StateChanged += Client_StateChanged;

            _botLog = new BotLog(syncObj);

            if (IsRunning)
                Start();
        }

        public void Configurate(PluginConfig cfg)
        {
            lock (_syncObj)
            {
                if (State == BotStates.Broken)
                    return;

                if (IsStopped())
                {
                    Config = cfg;
                    ConfigurationChanged?.Invoke(this);
                }
                else
                    throw new InvalidOperationException("Make sure that the bot is stopped before installing a new configuration");
            }

        }

        private void Client_StateChanged(ClientModel client)
        {
            lock (_syncObj)
            {
                if (client.ConnectionState == ConnectionStates.Online)
                {
                    if (State == BotStates.Starting)
                        StartExecutor();
                    else if (State == BotStates.Reconnecting)
                        ChangeState(BotStates.Online);
                }
                else if (client.ConnectionState == ConnectionStates.Disconnecting || client.ConnectionState == ConnectionStates.Offline)
                {
                    if (State == BotStates.Online && client.IsReconnecting)
                        ChangeState(BotStates.Reconnecting, client.Connection.HasError ? client.Connection.LastError.ToString() : null);
                    if ((State == BotStates.Online || State == BotStates.Starting || State == BotStates.Reconnecting) && !client.IsReconnectionPossible)
                        StopInternal(client.Connection.LastError.ToString());
                }
            }
        }

        public void Start()
        {
            lock (_syncObj)
            {
                if (State == BotStates.Broken)
                    throw new InvalidStateException("Trade bot is broken!");

                if (!IsStopped())
                    throw new InvalidStateException("Trade bot has been already started!");

                SetRunning(true);
                Package.IncrementRef();

                ChangeState(BotStates.Starting);

                if (_client.ConnectionState == ConnectionStates.Online)
                    StartExecutor();
            }
        }

        public Task StopAsync()
        {
            return StopInternal();
        }

        private Task StopInternal(string error = null, bool isExecutorStopped = false)
        {
            lock (_syncObj)
            {
                if (IsStopped())
                    return Task.FromResult(true);

                if (State != BotStates.Stopping)
                    _stopTask = DoStop(error, isExecutorStopped);

                return _stopTask;
            }
        }

        private async Task DoStop(string error, bool isExecutorStopped)
        {
            bool hasError = error != null;

            SetRunning(false);

            if (State == BotStates.Online || State == BotStates.Reconnecting)
            {
                if (!isExecutorStopped)
                {
                    ChangeState(BotStates.Stopping);
                    await Task.Factory.StartNew(() => executor?.Stop());
                }
                DisposeExecutor();
            }

            lock (_syncObj)
            {
                ChangeState(hasError ? BotStates.Faulted : BotStates.Offline, error);
                OnStop();
            }
        }

        public void Dispose()
        {
            _packageRepo.PackageChanged -= _packageRepo_PackageChanged;
            _client.StateChanged -= Client_StateChanged;
        }

        private bool IsStopped()
        {
            return State == BotStates.Offline || State == BotStates.Faulted;
        }

        private bool TaskIsNullOrStopped(Task task)
        {
            return task == null || task.IsCompleted || task.IsFaulted || task.IsCanceled;
        }

        private void StartExecutor()
        {
            try
            {
                executor = _ref.CreateExecutor();

                if (Config is BarBasedConfig)
                {
                    var setupModel = new BarBasedPluginSetup(_ref);
                    setupModel.Load(Config);
                    setupModel.Apply(executor);

                    var feedAdapter = new PluginFeedProvider(_client.Symbols, _client.FeedHistory, _client.Currencies, new SyncAdapter(_syncObj));
                    executor.InitBarStrategy(feedAdapter, setupModel.PriceType);
                    executor.MainSymbolCode = setupModel.MainSymbol;
                    executor.TimeFrame = Algo.Api.TimeFrames.M1;
                    executor.Metadata = feedAdapter;
                }
                else
                    throw new Exception("Unsupported configuration!");

                executor.InitSlidingBuffering(1024);

                executor.InvokeStrategy = new PriorityInvokeStartegy();
                executor.AccInfoProvider = _client.Account;
                executor.TradeApi = _client.TradeApi;
                executor.InitLogging().NewRecords += TradeBotModel_NewRecords;
                _stopListener = new ListenerProxy(executor, () =>
                {
                    StopInternal(null, true);
                });

                executor.Start();

                lock (_syncObj) ChangeState(BotStates.Online);
            }
            catch (Exception ex)
            {
                // TO DO: log
                lock (_syncObj)
                {
                    Fault = ex;
                    StopInternal(ex.Message, true);
                }
            }
        }

        private void TradeBotModel_NewRecords(BotLogRecord[] recs)
        {
            _botLog.Update(recs);
        }

        private void DisposeExecutor()
        {
            _stopListener.Dispose();
            executor?.Dispose();
        }

        private void OnExecutorStopped(string err = null)
        {
            lock (_syncObj)
            {
                if (State == BotStates.Online)
                {
                    SetRunning(false);
                    ChangeState(BotStates.Offline, err);
                    DisposeExecutor();
                }
            }
        }

        private void OnStop()
        {
            try
            {
                Package.DecrementRef();
            }
            catch (Exception ex)
            {
                _log.LogError("TradeBotModel.OnStopped() failed! {0}", ex);
            }
        }

        private void ChangeState(BotStates newState, string errorMessage = null)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                _log.LogInformation("TradeBot '{0}' State: {1}", Id, newState);
            else
                _log.LogWarning("TradeBot '{0}' State: {1} Error: {2}", Id, newState, errorMessage);
            State = newState;
            FaultMessage = errorMessage;
            StateChanged?.Invoke(this);
        }

        private void SetRunning(bool val)
        {
            if (IsRunning != val)
            {
                IsRunning = val;
                IsRunningChanged?.Invoke(this);
            }
        }

        private void UpdatePackage()
        {
            Package = _packageRepo.Get(PackageName);

            if (Package == null)
            {
                ChangeState(BotStates.Broken, "Package '" + PackageName + "' is not found in repository!");
                return;
            }

            _ref = Package.GetPluginRef(Descriptor);
            if (_ref == null || _ref.Descriptor.AlgoLogicType != AlgoTypes.Robot)
            {
                ChangeState(BotStates.Broken, $"Trade bot '{Descriptor}' is missing in package '{PackageName}'!");
                return;
            }

            if (State == BotStates.Broken)
                ChangeState(BotStates.Offline, null);
        }

        private void _packageRepo_PackageChanged(IPackage pckg, ChangeAction action)
        {
            if (pckg.NameEquals(PackageName))
                UpdatePackage();
        }

        private class ListenerProxy : CrossDomainObject
        {
            private PluginExecutor _executor;
            private Action _onStopped;

            public ListenerProxy(PluginExecutor executor, Action onStopped)
            {
                _executor = executor;
                _onStopped = onStopped;
                executor.IsRunningChanged += Executor_IsRunningChanged1;
            }

            private void Executor_IsRunningChanged1(PluginExecutor exec)
            {
                if (!exec.IsRunning)
                    _onStopped();
            }

            protected override void Dispose(bool disposing)
            {
                _executor.IsRunningChanged -= Executor_IsRunningChanged1;
                base.Dispose(disposing);
            }
        }
    }
}
