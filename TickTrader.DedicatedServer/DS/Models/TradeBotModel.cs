using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private void Client_StateChanged(ClientModel client)
        {
            if (State == BotStates.Started && client.ConnectionState == ConnectionStates.Online)
                StartExecutor();
        }

        public event Action<TradeBotModel> StateChanged;
        public event Action<TradeBotModel> IsRunningChanged;

        public void Start()
        {
            lock (_syncObj)
            {
                if (State == BotStates.Broken)
                    throw new InvalidStateException("Trade bot is broken!");

                if (State != BotStates.Offline && State != BotStates.Faulted)
                    throw new InvalidStateException("Trade bot has been already started!");

                SetRunning(true);
                Package.IncrementRef();

                if (_client.ConnectionState == ConnectionStates.Online)
                    StartExecutor();
                else
                    ChangeState(BotStates.Started);
            }
        }

        public Task StopAsync()
        {
            lock (_syncObj)
            {
                if (IsStopped())
                    return Task.FromResult(this);

                SetRunning(false);
                ChangeState(BotStates.Stopping);

                if (TaskIsNullOrStopped(_stopTask))
                    _stopTask = DoStop();

                return _stopTask;
            }
        }

        public void Dispose()
        {
            _packageRepo.PackageChanged -= _packageRepo_PackageChanged;
            _client.StateChanged -= Client_StateChanged;
        }

        private bool IsStopped()
        {
            return State == BotStates.Offline || State == BotStates.Stopping || State == BotStates.Faulted;
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
                executor.Logger = _botLog;
                _stopListener = new ListenerProxy(executor, () =>
                {
                    lock (_syncObj) OnStopped(false);
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
                    FaultMessage = ex.Message;
                    OnStopped(true);
                }
            }
        }

        private async Task DoStop()
        {
            await Task.Factory.StartNew(() => executor.Stop());
            lock (_syncObj) OnStopped(false);
        }

        private void OnStopped(bool isFaulted)
        {
            if (State != BotStates.Offline)
            {
                try
                {
                    _stopListener.Dispose();
                    if (executor != null)
                        executor.Dispose();
                    Package.DecrementRef();
                }
                catch (Exception ex)
                {
                    _log.LogError("TradeBotModel.OnStopped() failed! {0}", ex);
                }
                
                ChangeState(isFaulted ? BotStates.Faulted : BotStates.Offline);
                SetRunning(false);
            }
        }

        private void ChangeState(BotStates newState, string errorMessage = null)
        {
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
