using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.DedicatedServer.Infrastructure;

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract(Name = "tradeBot", Namespace = "")]
    public class TradeBotModel : ITradeBot
    {
        private object _syncObj;
        private ClientModel _client;
        private Task _stopTask;
        private PluginExecutor executor;
        private BotLog _log;
        private PluginSetup _setupModel;
        private AlgoPluginRef _ref;

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
        public IAccount Account => _client;
        public IBotLog Log => _log;

        public void Init(ClientModel client, object syncObj, Func<string, PackageModel> packageProvider, IAlgoGuiMetadata tradeMetadata)
        {
            _syncObj = syncObj;
            _client = client;
            Package = packageProvider(PackageName);

            _ref = Package.GetPluginRef(Descriptor);
            if (_ref == null || _ref.Descriptor.AlgoLogicType != AlgoTypes.Robot)
            {
                // TO DO : faulted state
            }
            else
            {
                if (Config is BarBasedConfig)
                {
                    _setupModel = new BarBasedPluginSetup(_ref);
                    _setupModel.Load(Config);
                }

                if (IsRunning)
                    State = BotStates.Started;

                client.StateChanged += Client_StateChanged;

                _log = new BotLog(syncObj);
            }
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
                if (State == BotStates.Offline)
                    return Task.FromResult(this);

                SetRunning(false);

                if (_stopTask == null)
                    _stopTask = DoStop();

                return _stopTask;
            }
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

                executor.InvokeStrategy = new PriorityInvokeStartegy();
                executor.AccInfoProvider = _client.Account;
                executor.Logger = _log;

                //executor.MainSymbolCode = 
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
                    if (executor != null)
                        executor.Dispose();
                    SetRunning(false);
                    ChangeState(BotStates.Faulted);
                }
            }
        }

        private async Task DoStop()
        {
            await Task.Factory.StartNew(() => executor.Stop());
            lock (_syncObj)
            {
                ChangeState(BotStates.Offline);
                Package.DecrementRef();
            }
        }

        private void ChangeState(BotStates newState)
        {
            State = newState;
            StateChanged?.Invoke(this);       
        }

        private void SetRunning(bool val)
        {
            IsRunning = val;
            IsRunningChanged?.Invoke(this);
        }
    }
}
