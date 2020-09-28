using ActorSharp;
using NLog;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.BA.Repository;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "tradeBot", Namespace = "")]
    public class TradeBotModel
    {
        private static readonly ILogger _log = LogManager.GetLogger(nameof(ServerModel));

        private ClientModel _client;
        private Task _stopTask;
        private PluginExecutorCore executor;
        private BotLog.ControlHandler _botLog;
        private AlgoData _algoData;
        private AlgoPluginRef _ref;
        private BotListenerProxy _botListener;
        private PackageStorage _packageRepo;
        private TaskCompletionSource<object> _startedEvent;
        private bool _closed;

        public TradeBotModel(PluginConfig config)
        {
            Config = config;
        }

        [DataMember(Name = "configuration")]
        public PluginConfig Config { get; private set; }
        [DataMember(Name = "running")]
        public bool IsRunning { get; private set; }


        public string Id => Config.InstanceId;
        public PluginPermissions Permissions => Config.Permissions;
        public PluginKey PluginKey => Config.Key;
        public PackageKey PackageKey { get; private set; }
        public PluginStates State { get; private set; }
        public AlgoPackageRef Package { get; private set; }
        public Exception Fault { get; private set; }
        public string FaultMessage { get; private set; }
        public AccountKey Account => _client.GetKey();
        public Ref<BotLog> LogRef => _botLog.Ref;
        public AlgoPluginRef AlgoRef => _ref;

        public IBotFolder AlgoData => _algoData;

        public event Action<TradeBotModel> StateChanged;
        public event Action<TradeBotModel> IsRunningChanged;
        public event Action<TradeBotModel> ConfigurationChanged;

        public bool Init(ClientModel client, PackageStorage packageRepo, string workingFolder, AlertStorage storage)
        {
            try
            {
                _client = client;
                _packageRepo = packageRepo;

                UpdatePackage();

                _client.StateChanged += Client_StateChanged;

                _botLog = new BotLog.ControlHandler(Id, storage);

                _algoData = new AlgoData(workingFolder);

                if (IsRunning && State != PluginStates.Broken)
                    Start();

                return true;
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to init bot {Id}");
            }
            return false;
        }

        public void ChangeBotConfig(PluginConfig config)
        {
            CheckShutdownFlag();

            if (State == PluginStates.Broken)
                return;

            if (IsStopped())
            {
                if (config.Key == null)
                    config.Key = Config.Key;

                Config = config;
                ConfigurationChanged?.Invoke(this);
            }
            else
                throw new InvalidStateException("Make sure that the bot is stopped before installing a new configuration");
        }

        private void Client_StateChanged(ClientModel client)
        {
            if (client.ConnectionState == ConnectionStates.Online)
            {
                if (State == PluginStates.Starting)
                    StartExecutor();
                else if (State == PluginStates.Reconnecting)
                {
                    executor.HandleReconnect();
                    executor.WriteConnectionInfo(GetConnectionInfo());
                    ChangeState(PluginStates.Running);
                }
            }
            else
            {
                if (State == PluginStates.Running)
                {
                    executor.HandleDisconnect();
                    executor.WriteConnectionInfo(GetConnectionInfo());
                    ChangeState(PluginStates.Reconnecting, client.LastError != null && client.LastError.Code != ConnectionErrorCodes.None ? client.ErrorText : null);
                }
            }
        }

        public Task ClearLog()
        {
            return _botLog.Clear();
        }

        public void ClearWorkingFolder()
        {
            AlgoData.Clear();
        }

        public void Start()
        {
            CheckShutdownFlag();

            if (!IsStopped())
                throw new InvalidStateException("Trade bot has been already started!");

            _algoData.EnsureDirectoryCreated();

            UpdatePackage();

            if (State == PluginStates.Broken)
                throw new InvalidStateException("Trade bot is broken!");

            Package.IncrementRef();

            SetRunning(true);

            ChangeState(PluginStates.Starting);

            if (_client.ConnectionState == ConnectionStates.Online)
                StartExecutor();
        }

        public Task StopAsync()
        {
            CheckShutdownFlag();

            return DoStop(null);
        }

        public Task Shutdown()
        {
            _closed = true;
            return DoStop(null);
        }

        public void Remove(bool cleanLog = false, bool cleanAlgoData = false)
        {
            _client.RemoveBot(Id, cleanLog, cleanAlgoData);
        }

        private void OnBotExited()
        {
            DoStop(null).Forget();
        }

        private Task DoStop(string error)
        {
            SetRunning(false);

            if (State == PluginStates.Stopped || State == PluginStates.Faulted)
                return Task.CompletedTask;

            if (State == PluginStates.Starting && (_startedEvent == null || _startedEvent.Task.IsCompleted))
            {
                ChangeState(PluginStates.Stopped);
                return Task.CompletedTask; // acc can't connect at bot start, also bot might be launched before
            }

            if (State == PluginStates.Running || State == PluginStates.Reconnecting || State == PluginStates.Starting)
            {
                ChangeState(PluginStates.Stopping);
                _stopTask = StopExecutor();
            }

            return _stopTask;
        }

        public void Dispose()
        {
            _client.StateChanged -= Client_StateChanged;
        }

        private bool IsStopped()
        {
            return State == PluginStates.Stopped || State == PluginStates.Faulted || State == PluginStates.Broken;
        }

        private bool TaskIsNullOrStopped(Task task)
        {
            return task == null || task.IsCompleted || task.IsFaulted || task.IsCanceled;
        }

        private async void StartExecutor()
        {
            _startedEvent = new TaskCompletionSource<object>();

            try
            {
                if (executor != null)
                    throw new InvalidOperationException("Cannot start executor: old executor instance is not disposed!");

                executor = _ref.CreateExecutor();

                var setupModel = new PluginSetupModel(_ref, new SetupMetadata((await _client.GetMetadata()).Symbols), new SetupContext(), Config.MainSymbol);
                setupModel.Load(Config);

                var feedAdapter = _client.CreatePluginFeedAdapter();
                executor.Feed = feedAdapter;
                executor.FeedHistory = feedAdapter;
                executor.InitBarStrategy(Algo.Api.BarPriceType.Bid);
                executor.MainSymbolCode = setupModel.MainSymbol.Id;
                executor.TimeFrame = setupModel.SelectedTimeFrame;
                executor.Metadata = feedAdapter;
                executor.InitSlidingBuffering(4000);

                executor.InvokeStrategy = new PriorityInvokeStartegy();
                executor.AccInfoProvider = _client.PluginTradeInfo;
                executor.TradeExecutor = _client.PluginTradeApi;
                executor.TradeHistoryProvider = _client.PluginTradeHistory.AlgoAdapter;
                executor.BotWorkingFolder = AlgoData.Folder;
                executor.WorkingFolder = AlgoData.Folder;
                executor.InstanceId = Id;
                executor.Permissions = Permissions;
                _botListener = new BotListenerProxy(executor, OnBotExited, _botLog.GetWriter());

                setupModel.SetWorkingFolder(AlgoData.Folder);
                setupModel.Apply(executor);

                executor.Start();
                _botListener.Start();
                executor.WriteConnectionInfo(GetConnectionInfo());

                ChangeState(PluginStates.Running);
            }
            catch (Exception ex)
            {
                Fault = ex;
                _log.Error(ex, "StartExecutor() failed!");
                _startedEvent.SetResult(null);
                SetRunning(false);
                if (State != PluginStates.Stopping)
                    await StopExecutor(true);
                return;
            }

            _startedEvent.SetResult(null);
        }

        private async Task StopExecutor(bool hasError = false)
        {
            if (_startedEvent != null)
                await _startedEvent.Task;

            executor.WriteConnectionInfo(GetConnectionInfo());

            try
            {
                await Task.Factory.StartNew(() => executor?.Stop());
            }
            catch (Exception ex)
            {
                Fault = ex;
                _log.Error(ex, "StopExecutor() failed!");
            }

            _botListener?.Stop();
            DisposeExecutor();
            ChangeState(hasError ? PluginStates.Faulted : PluginStates.Stopped);
            OnStop();
        }

        private void DisposeExecutor()
        {
            _botListener?.Dispose();
            executor?.Dispose();
            executor = null;
            _botListener = null;
        }

        private void OnStop()
        {
            try
            {
                Package.DecrementRef();
            }
            catch (Exception ex)
            {
                _log.Error(ex, "TradeBotModel.OnStopped() failed! {0}");
            }
        }

        private void ChangeState(PluginStates newState, string errorMessage = null)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                _log.Info("TradeBot '{0}' State: {1}", Id, newState);
            else
                _log.Error("TradeBot '{0}' State: {1} Error: {2}", Id, newState, errorMessage);
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
            PackageKey = PluginKey.GetPackageKey();
            Package = _packageRepo.GetPackageRef(PackageKey);

            if (Package == null)
            {
                BreakBot($"Package {PackageKey.Name} at {PackageKey.Location} is not found!");
                return;
            }

            _ref = _packageRepo.Library.GetPluginRef(PluginKey);
            if (_ref == null || _ref.Metadata.Descriptor.Type != AlgoTypes.Robot)
            {
                BreakBot($"Trade bot {PluginKey.DescriptorId} is missing in package {PluginKey.PackageName} at {PluginKey.PackageLocation}!");
                return;
            }

            if (State == PluginStates.Broken)
                ChangeState(PluginStates.Stopped, null);
        }

        public void Abort()
        {
            CheckShutdownFlag();

            if (State == PluginStates.Stopping)
                executor?.Abort();
        }

        private void BreakBot(string message)
        {
            ChangeState(PluginStates.Broken, message);
            SetRunning(false);
        }

        private void CheckShutdownFlag()
        {
            if (_closed)
                throw new InvalidOperationException("Server is shutting down!");
        }

        private string GetConnectionInfo()
        {
            return $"account {_client.Username} on {_client.Address}";
        }
    }
}
