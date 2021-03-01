using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.BotAgent.BA.Repository;
using TickTrader.BotAgent.BA.Exceptions;
using Machinarium.Qnil;
using ActorSharp.Lib;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotAgent.BA.Models
{
    [DataContract(Name = "account", Namespace = "")]
    public class ClientModel
    {
        private IAlgoCoreLogger _log;
        private int _loggerId;
        private static int LoggerNameIdSeed = 0;

        private static readonly TimeSpan KeepAliveThreshold = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan ReconnectThreshold = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan ReconnectThreshold_BadCreds = TimeSpan.FromMinutes(1);

        private AlertStorage _alertStorage;
        private AlgoServer _server;

        private CancellationTokenSource _connectCancellation;
        private AsyncGate _requestGate;
        private ConnectionErrorInfo _lastError = ConnectionErrorInfo.Ok;
        private Algo.Common.Model.ClientModel.ControlHandler2 _core;
        private TaskCompletionSource<object> _shutdownCompletedSrc;
        private DateTime _pendingDisconnect;
        private DateTime _pendingReconnect;

        [DataMember(Name = "bots")]
        private List<TradeBotModel> _bots = new List<TradeBotModel>();
        private PackageStorage _packageProvider;

        private bool _isInitialized;
        private bool _credsChanged;
        private bool _lostConnection;
        private int _startedBotsCount;
        private bool _shutdownRequested => _shutdownCompletedSrc != null;

        public ClientModel(string address, string username, string password)
        {
            Address = address;
            Username = username;
            Password = password;
        }

        public async Task Init(PackageStorage packageProvider, IFdkOptionsProvider fdkOptionsProvider, AlertStorage storage, AlgoServer server)
        {
            _loggerId = Interlocked.Increment(ref LoggerNameIdSeed);
            _log = CoreLoggerFactory.GetLogger<ClientModel>(_loggerId);

            try
            {
                _packageProvider = packageProvider;
                _alertStorage = storage;
                _server = server;
                _requestGate = new AsyncGate();
                _requestGate.OnWait += ManageConnection;
                _requestGate.OnExit += KeepAlive;

                if (_bots == null)
                    _bots = new List<TradeBotModel>();

                var toRemove = new List<TradeBotModel>();

                foreach (var bot in _bots)
                {
                    if (!bot.OnDeserialized())
                    {
                        toRemove.Add(bot);
                        continue;
                    }

                    try
                    {
                        BotValidation?.Invoke(bot);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Bot '{0}' failed validation and was removed! {1}", bot.Id);
                        toRemove.Add(bot);
                        continue;
                    }

                    if (bot.IsRunning)
                        _startedBotsCount++;
                    InitBot(bot);
                }

                foreach (var bot in toRemove)
                    _bots.Remove(bot);

                //var options = new ConnectionOptions() { EnableLogs = false, LogsFolder = ServerModel.Environment.LogFolder, Type = AppType.BotAgent };

                _core = new Algo.Common.Model.ClientModel.ControlHandler2(fdkOptionsProvider.GetConnectionOptions(),
                    ServerModel.Environment.FeedHistoryCacheFolder, FeedHistoryFolderOptions.ServerClientHierarchy, _loggerId);

                await _core.OpenHandler();

                _core.Connection.Disconnected += () =>
                {
                    _lostConnection = true;
                    ManageConnection();
                };

                PluginFeedAdapter = await _core.CreateFeedProvider();
                PluginTradeApi = await _core.CreateTradeApi();
                PluginTradeInfo = await _core.CreateTradeProvider();
                PluginTradeHistory = await _core.CreateTradeHistory();

                _isInitialized = true;

                ManageConnectionLoop();
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to init account {Username} on {Address}");
            }
        }

        public IAccountProxy GetAccountProxy()
        {
            return new LocalAccountProxy(_core.Id)
            {
                Feed = PluginFeedAdapter,
                FeedHistory = PluginFeedAdapter,
                Metadata = PluginFeedAdapter,
                AccInfoProvider = PluginTradeInfo,
                TradeExecutor = PluginTradeApi,
                TradeHistoryProvider = PluginTradeHistory.AlgoAdapter,
            };
        }

        public AlertStorage AlertStorage { get; }
        public ConnectionStates ConnectionState { get; private set; }
        public ConnectionErrorInfo LastError => _lastError;
        public PluginFeedProvider PluginFeedAdapter { get; private set; }
        public PluginTradeApiProvider.Handler PluginTradeApi { get; private set; }
        public PluginTradeInfoProvider PluginTradeInfo { get; private set; }
        public TradeHistoryProvider.Handler PluginTradeHistory { get; private set; }

        public string Id => _core.Id;
        public int TotalBotsCount => _bots.Count;
        public int RunningBotsCount => _startedBotsCount;
        public bool HasRunningBots => _startedBotsCount > 0;
        public bool HasError => _lastError != null && _lastError.Code != ConnectionErrorCodes.None;
        public string ErrorText => _lastError?.TextMessage ?? _lastError?.Code.ToString();

        public event Action<ClientModel> StateChanged;
        public event Action<ClientModel> Changed;
        public event Action<TradeBotModel> BotValidation;
        public event Action<TradeBotModel> BotInitialized;
        public event Action<TradeBotModel, ChangeAction> BotChanged;
        public event Action<TradeBotModel> BotStateChanged;
        public event Action<TradeBotModel> BotConfigurationChanged;

        [DataMember(Name = "server")]
        public string Address { get; private set; }
        [DataMember(Name = "login")]
        public string Username { get; private set; }
        [DataMember(Name = "password")]
        public string Password { get; private set; }

        public async Task<AccountMetadataInfo> GetMetadata()
        {
            using (await _requestGate.Enter())
            {
                if (_lastError.Code != ConnectionErrorCodes.None)
                    throw new CommunicationException("Connection error! Code: " + _lastError.Code, _lastError.Code);

                var symbols = await _core.GetSymbols();
                var defaultSymbol = await _core.GetDefaultSymbol();
                return new AccountMetadataInfo(GetKey(), symbols.Select(s => s.ToInfo()).ToList(), defaultSymbol.ToInfo());
            }

            //if (ConnectionState == ConnectionStates.Online)
            //    return Task.FromResult(GetMetadataInfo());
            //else
            //{
            //    return AddPendingRequest(
            //        new Task<TradeMetadataInfo>(() =>
            //        {
            //            if (_lastError.Code != ConnectionErrorCodes.None)
            //                throw new CommunicationException("Connection error! Code: " + _lastError.Code, _lastError.Code);
            //            return GetMetadataInfo();
            //        }));
            //}
        }

        public AccountKey GetKey()
        {
            return new AccountKey(Address, Username);
        }

        public PluginFeedProvider CreatePluginFeedAdapter()
        {
            return _core.CreateFeedProvider().Result;
        }

        private void CheckInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Not Initialized!");
        }

        #region Connection Management

        public async Task<ConnectionErrorInfo> TestConnection()
        {
            using (await _requestGate.Enter())
                return _lastError;

            //if (ConnectionState == ConnectionStates.Online)
            //    return Task.FromResult(ConnectionErrorInfo.Ok);
            //else
            //    return AddPendingRequest(new Task<ConnectionErrorInfo>(() => _lastError, _requestCancellation.Token));
        }

        private void KeepAlive()
        {
            _pendingDisconnect = DateTime.UtcNow + KeepAliveThreshold;
        }

        private void ScheduleReconnect(bool authProblem)
        {
            _pendingReconnect = DateTime.UtcNow + (authProblem ? ReconnectThreshold_BadCreds : ReconnectThreshold);
        }

        private void ManageConnection()
        {
            if (ConnectionState == ConnectionStates.Offline)
            {
                var forcedConnect = (_startedBotsCount > 0 && _credsChanged) || _requestGate.WatingCount > 0;
                var scheduledConnect = _startedBotsCount > 0 && _pendingReconnect < DateTime.UtcNow;

                if (forcedConnect || scheduledConnect)
                    Connect();
            }
            else if (ConnectionState == ConnectionStates.Online)
            {
                var forcedDisconnect = _credsChanged || _lostConnection || _shutdownRequested;
                var scheduledDisconnect = _startedBotsCount == 0 && _pendingDisconnect < DateTime.UtcNow;

                if (forcedDisconnect || scheduledDisconnect)
                    Disconnect();
            }
        }

        private async void ManageConnectionLoop()
        {
            while (!_shutdownRequested)
            {
                ManageConnection();
                await Task.Delay(1000);
            }
        }

        public async Task ShutdownAsync()
        {
            //_connectAfterCancellation?.Cancel();
            CheckInitialized();

            if (!_shutdownRequested)
            {
                _shutdownCompletedSrc = new TaskCompletionSource<object>();

                Task[] stopBots = _bots.Select(tb => tb.StopAsync()).ToArray();
                try
                {
                    await Task.WhenAll(stopBots);
                }
                catch(Exception ex)
                {
                    _log.Error("Failed to shutdown bots", ex);
                }

                if (ConnectionState == ConnectionStates.Offline)
                    _shutdownCompletedSrc.TrySetResult(null);
                else
                    ManageConnection();

                await _core.CloseHandler();
            }

            await _shutdownCompletedSrc.Task;
            //if (_disconnectCompletionSource != null)
            //await _disconnectCompletionSource.Task;
        }

        private async void Disconnect()
        {
            ChangeState(ConnectionStates.Disconnecting);

            _log.Debug("Closing gate...");

            await _requestGate.Close();

            _log.Debug("Closing connection...");

            await _core.Connection.Disconnect();

            _lostConnection = false;
            ScheduleReconnect(false);
            ChangeState(ConnectionStates.Offline);

            _log.Debug("Offline!");

            _shutdownCompletedSrc?.TrySetResult(null);

            ManageConnection();
        }

        private void ChangeState(ConnectionStates newState)
        {
            LogConnectionState(ConnectionState, newState);
            ConnectionState = newState;
            StateChanged?.Invoke(this);
        }

        private void LogConnectionState(ConnectionStates oldState, ConnectionStates newState)
        {
            if (IsConnected(oldState, newState))
                _log.Info("{0}: login on {1}", Username, Address);
            else if (IsUsualDisconnect(oldState, newState))
                _log.Info("{0}: logout from {1}", Username, Address);
            else if (IsFailedConnection(oldState, newState))
                _log.Info("{0}: connect to {1} failed [{2}]", Username, Address, _lastError?.Code);
            else if (IsUnexpectedDisconnect(oldState, newState))
                _log.Info("{0}: connection to {1} lost [{2}]", Username, Address, _lastError?.Code);
        }

        private bool IsConnected(ConnectionStates from, ConnectionStates to)
        {
            return to == ConnectionStates.Online;
        }

        private bool IsUnexpectedDisconnect(ConnectionStates from, ConnectionStates to)
        {
            return HasError && from == ConnectionStates.Online && (to == ConnectionStates.Offline || to == ConnectionStates.Disconnecting);
        }

        private bool IsFailedConnection(ConnectionStates from, ConnectionStates to)
        {
            return from == ConnectionStates.Connecting && to == ConnectionStates.Offline && HasError;
        }

        private bool IsUsualDisconnect(ConnectionStates from, ConnectionStates to)
        {
            return from == ConnectionStates.Disconnecting && to == ConnectionStates.Offline && !HasError;
        }

        private async void Connect()
        {
            ChangeState(ConnectionStates.Connecting);
            _credsChanged = false;

            _connectCancellation = new CancellationTokenSource();

            _lastError = await _core.Connection.Connect(Username, Password, Address, _connectCancellation.Token);

            if (_lastError.Code == ConnectionErrorCodes.None)
            {
                _lostConnection = false;
                KeepAlive();
                ChangeState(ConnectionStates.Online);

                _requestGate.Open();
            }
            else
            {
                await _requestGate.ExecQueuedRequests();

                ScheduleReconnect(_lastError.Code == ConnectionErrorCodes.BlockedAccount || _lastError.Code == ConnectionErrorCodes.InvalidCredentials);
                ChangeState(ConnectionStates.Offline);
            }
        }

        public void ChangePassword(string password)
        {
            Password = password;
            OnCredsChanged();
        }

        public void ChangeConnectionSettings(string password)
        {
            var updated = false;
            if (!string.IsNullOrEmpty(password))
            {
                Password = password;
                updated = true;
            }

            if (updated)
                OnCredsChanged();
        }

        private void OnCredsChanged()
        {
            _credsChanged = true;
            _connectCancellation?.Cancel();
            Changed?.Invoke(this);
            ManageConnection();
        }

        #endregion Connection Management

        #region Bot Management

        public TradeBotModel AddBot(PluginConfig config)
        {
            CheckInitialized();

            var algoKey = config.Key;

            var package = _packageProvider.GetPackageRef(algoKey.Package.Name);

            if (package == null)
                throw new PackageNotFoundException($"Algo Package {algoKey.Package.Name} at {algoKey.Package.Location} cannot be found!");

            var newBot = new TradeBotModel(config);
            BotValidation?.Invoke(newBot);
            InitBot(newBot);
            _bots.Add(newBot);
            ManageConnection();
            BotChanged?.Invoke(newBot, ChangeAction.Added);
            return newBot;
        }

        private void OnBotIsRunningChanged(TradeBotModel bot)
        {
            if (bot.IsRunning)
                _startedBotsCount++;
            else
                _startedBotsCount--;

            if (!_shutdownRequested)
            {
                BotChanged?.Invoke(bot, ChangeAction.Modified);
                KeepAlive();
                ManageConnection();
            }
        }

        public void RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            CheckInitialized();

            var bot = _bots.FirstOrDefault(b => b.Id == botId);
            if (bot != null)
            {
                if (bot.IsRunning)
                    throw new InvalidStateException("Cannot remove running bot!");

                if (cleanLog)
                    bot.ClearLog();

                if (cleanAlgoData)
                    bot.ClearWorkingFolder();

                _bots.Remove(bot);
                DeinitBot(bot);
                BotChanged?.Invoke(bot, ChangeAction.Removed);
            }
        }

        public void RemoveAllBots()
        {
            CheckInitialized();

            if (HasRunningBots)
                throw new InvalidStateException("Some bots are running. Remove is not possible.");

            foreach (var bot in _bots)
            {
                DeinitBot(bot);
                bot.ClearLog();
                bot.ClearWorkingFolder();
                BotChanged?.Invoke(bot, ChangeAction.Removed);
            }

            _bots.Clear();
        }

        private void InitBot(TradeBotModel bot)
        {
            bot.IsRunningChanged += OnBotIsRunningChanged;
            bot.ConfigurationChanged += OnBotConfigurationChanged;
            bot.StateChanged += OnBotStateChanged;

            if (bot.Init(_server, this, _packageProvider, ServerModel.GetWorkingFolderFor(bot.Id), _alertStorage))
            {
                BotInitialized?.Invoke(bot);
            }
        }

        private void OnBotConfigurationChanged(TradeBotModel bot)
        {
            BotConfigurationChanged?.Invoke(bot);
            BotChanged?.Invoke(bot, ChangeAction.Modified);
        }

        private void DeinitBot(TradeBotModel bot)
        {
            bot.ConfigurationChanged -= OnBotConfigurationChanged;
            bot.IsRunningChanged -= OnBotIsRunningChanged;
            bot.StateChanged -= OnBotStateChanged;
            bot.Dispose();
        }

        private void OnBotStateChanged(TradeBotModel bot)
        {
            BotStateChanged?.Invoke(bot);
        }

        internal void RemoveBotsFromPackage(AlgoPackageRef package)
        {
            var toRemove = _bots.Where(b => b.Package == package).ToList();
            toRemove.ForEach(b => _bots.Remove(b));
        }

        #endregion
    }
}
