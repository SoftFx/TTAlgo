using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BotAgent.BA.Builders;
using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.BA.Repository;
using TickTrader.BotAgent.Infrastructure;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "tradeBot", Namespace = "")]
    public class TradeBotModel : ITradeBot
    {
        private ILogger _log;
        private ILoggerFactory _loggerFactory;
        private object _syncObj;
        private ClientModel _client;
        private Task _stopTask;
        private PluginExecutor executor;
        private BotLog _botLog;
        private AlgoData _algoData;
        private AlgoPluginRef _ref;
        private BotListenerProxy _botListener;
        private PackageStorage _packageRepo;

        public TradeBotModel(TradeBotModelConfig config)
        {
            Id = config.InstanceId;
            Config = config.PluginConfig;
            PackageName = config.Plugin.PackageName;
            Descriptor =  config.Plugin.DescriptorId;
            Isolated = config.Isolated;
            Permissions = config.Permissions;
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
        [DataMember(Name = "isolated")]
        public bool Isolated { get; private set; }
        [DataMember(Name = "permissions")]
        public PluginPermissions Permissions { get; private set; }

        public BotStates State { get; private set; }
        public PackageModel Package { get; private set; }
        public Exception Fault { get; private set; }
        public string FaultMessage { get; private set; }
        public IAccount Account => _client;
        public IBotLog Log => _botLog;
        public string BotName => _ref?.DisplayName;

        public IAlgoData AlgoData => _algoData;

        public event Action<TradeBotModel> StateChanged;
        public event Action<TradeBotModel> IsRunningChanged;
        public event Action<TradeBotModel> ConfigurationChanged;

        public void Init(ClientModel client, ILoggerFactory logFactory, object syncObj, PackageStorage packageRepo, IAlgoGuiMetadata tradeMetadata, string workingFolder)
        {
            _syncObj = syncObj;
            _client = client;

            _loggerFactory = logFactory;
            _log = _loggerFactory.CreateLogger<TradeBotModel>();

            if (Permissions == null)
                Permissions = new DefaultPermissionsBuilder().Build();

            _packageRepo = packageRepo;
            UpdatePackage();

            _packageRepo.PackageChanged += _packageRepo_PackageChanged;
            _client.StateChanged += Client_StateChanged;

            _botLog = new BotLog(Id, syncObj);

            _algoData = new AlgoData(workingFolder, syncObj);

            if (IsRunning && State != BotStates.Broken)
                Start();
        }

        public void Configurate(TradeBotModelConfig config)
        {
            lock (_syncObj)
            {
                if (State == BotStates.Broken)
                    return;

                if (IsStopped())
                {
                    Config = config.PluginConfig;
                    Isolated = config.Isolated;
                    Permissions = config.Permissions;
                    ConfigurationChanged?.Invoke(this);
                }
                else
                    throw new InvalidStateException("Make sure that the bot is stopped before installing a new configuration");
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
                        ChangeState(BotStates.Reconnecting, client.Connection.HasError ? client.Connection.LastErrorCode.ToString() : null);
                    if ((State == BotStates.Online || State == BotStates.Starting || State == BotStates.Reconnecting) && !client.IsReconnectionPossible)
                        StopInternal(client.Connection.LastErrorCode.ToString());
                }
            }
        }

        public void ClearLog()
        {
            lock (_syncObj)
            {
                foreach (var file in Log.Files)
                {
                    try
                    {
                        Log.DeleteFile(file.Name);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(0, ex, "Could not delete file \"{0}\" of bot \"{1}\"", file.Name, Id);
                    }
                }
                try
                {
                    if (Directory.Exists(Log.Folder))
                        Directory.Delete(Log.Folder);
                }
                catch { }
            }
        }

        public void ClearWorkingFolder()
        {
            lock (_syncObj)
            {
                foreach (var file in AlgoData.Files)
                {
                    try
                    {
                        AlgoData.DeleteFile(file.Name);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(0, ex, "Could not delete file \"{0}\" of bot \"{1}\"", file.Name, Id);
                    }
                }
                try
                {
                    if (Directory.Exists(AlgoData.Folder))
                        Directory.Delete(AlgoData.Folder);
                }
                catch { }
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
                _botListener?.Stop();
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

                if (!(Config is BarBasedConfig))
                    throw new Exception("Unsupported configuration!");

                var setupModel = new BarBasedPluginSetup(_ref);
                setupModel.Load(Config);
                setupModel.SetWorkingFolder(AlgoData.Folder);
                setupModel.Apply(executor);

                var feedAdapter = new PluginFeedProvider(_client.Symbols, _client.FeedHistory, _client.Currencies, new SyncAdapter(_syncObj));
                executor.InitBarStrategy(feedAdapter, setupModel.PriceType);
                executor.MainSymbolCode = setupModel.MainSymbol;
                executor.TimeFrame = Algo.Api.TimeFrames.M1;
                executor.Metadata = feedAdapter;
                executor.InitSlidingBuffering(1024);

                executor.InvokeStrategy = new PriorityInvokeStartegy();
                executor.AccInfoProvider = _client.Account;
                executor.TradeExecutor = _client.TradeApi;
                executor.TradeHistoryProvider = new TradeHistoryProvider(_client.Connection);
                executor.BotWorkingFolder = AlgoData.Folder;
                executor.WorkingFolder = AlgoData.Folder;
                executor.Isolated = Isolated;
                executor.InstanceId = Id;
                executor.Permissions = Permissions;
                _botListener = new BotListenerProxy(executor, () => StopInternal(null, true), _botLog);

                executor.Start();
                _botListener.Start();

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

        private void DisposeExecutor()
        {
            _botListener?.Dispose();
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

        public void Abort()
        {
            lock(_syncObj)
            {
                if (State == BotStates.Stopping)
                    executor?.Abort();
            }
        }
    }
}
