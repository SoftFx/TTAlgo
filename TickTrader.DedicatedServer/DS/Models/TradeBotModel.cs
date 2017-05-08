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
        private PluginSetup _setupModel;
        private AlgoPluginRef _ref;
        private ListenerProxy _stopListener;

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

        public void Init(ClientModel client, ILogger log, object syncObj, Func<string, PackageModel> packageProvider, IAlgoGuiMetadata tradeMetadata)
        {
            _syncObj = syncObj;
            _client = client;
            _log = log;
            Package = packageProvider(PackageName);

            _ref = Package.GetPluginRef(Descriptor);
            if (_ref == null || _ref.Descriptor.AlgoLogicType != AlgoTypes.Robot)
            {
                State = BotStates.Broken;
                FaultMessage = "Package '" + PackageName + "' is not found in repository!";
            }
            else
            {
                ApplyConfig();

                _client.StateChanged += Client_StateChanged;

                _botLog = new BotLog(_syncObj);

                if (IsRunning)
                    Start();
            }
        }

        public void Configurate(PluginConfig cfg)
        {
            if (State == BotStates.Broken)
                return;

            if (IsStopped())
            {
                Config = cfg;
                ApplyConfig();
                ConfigurationChanged?.Invoke(this);
            }
            else
                throw new InvalidOperationException("Make sure that the bot is stopped before installing a new configuration");
            
        }

        private void ApplyConfig()
        {
            if (Config is BarBasedConfig)
            {
                _setupModel = new BarBasedPluginSetup(_ref);
                _setupModel.Load(Config);
            }
        }

        private void Client_StateChanged(ClientModel client)
        {
            if (State == BotStates.Started && client.ConnectionState == ConnectionStates.Online)
                StartExecutor();
        }

       

        public void Start()
        {
            lock (_syncObj)
            {
                if (State != BotStates.Offline && State != BotStates.Faulted)
                    throw new InvalidStateException("Bot has been already started!");

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
                if (IsStopped() || State == BotStates.Stopping)
                    return Task.FromResult(this);

                SetRunning(false);
                ChangeState(BotStates.Stopping);

                if (TaskIsNullOrStopped(_stopTask))
                    _stopTask = DoStop();

                return _stopTask;
            }
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

                if (_setupModel is BarBasedPluginSetup)
                {
                    var barSetup = (BarBasedPluginSetup)_setupModel;
                    var feedAdapter = new PluginFeedProvider(_client.Symbols, _client.FeedHistory, _client.Currencies, new SyncAdapter(_syncObj));
                    executor.InitBarStrategy(feedAdapter, barSetup.PriceType);
                    executor.MainSymbolCode = barSetup.MainSymbol;
                }
                else
                    throw new Exception("Unsupported configuration!");

                executor.InitSlidingBuffering(10);

                executor.InvokeStrategy = new PriorityInvokeStartegy();
                executor.AccInfoProvider = _client.Account;
                executor.TradeApi = _client.TradeApi;
                executor.Logger = _botLog;
                _stopListener = new ListenerProxy(executor, () =>
                {
                    lock (_syncObj) OnStopped(false);
                });

                _setupModel.Apply(executor);
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

        private void ChangeState(BotStates newState)
        {
            State = newState;
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
