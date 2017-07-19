using Microsoft.Extensions.Logging;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.DedicatedServer.DS.Repository;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.Infrastructure;
using TickTrader.DedicatedServer.DS.Info;
using System.IO;
using TickTrader.DedicatedServer.Extensions;

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract(Name = "account", Namespace = "")]
    public class ClientModel : IAccount
    {
        private object _sync;
        private ILogger _log;
        private ILoggerFactory _loggerFactory;
        private CancellationTokenSource _disconnectCancellation;
        private CancellationTokenSource _connectCancellation;
        private CancellationTokenSource _requestCancellation;
        private List<Task> _requests;
        private ConnectionErrorCodes _lastErrorCode;
        private ConnectionErrorCodes _currentErrorCode;
        private ClientCore _core;
        private TaskCompletionSource<object> _disconnectEvent;

        [DataMember(Name = "bots")]
        private List<TradeBotModel> _bots = new List<TradeBotModel>();
        private PackageStorage _packageProvider;

        private bool _stopRequested;
        private bool _lostConnection;
        private int _startedBotsCount;
        private bool _shutdownRequested;

        public ClientModel(string address, string username, string password)
        {
            Address = address;
            Username = username;
            Password = password;
        }

        public void Init(object syncObj, ILoggerFactory loggerFactory, PackageStorage packageProvider)
        {
            _sync = syncObj;
            _packageProvider = packageProvider;
            _loggerFactory = loggerFactory;
            _log = _loggerFactory.CreateLogger<ClientModel>();
            _requests = new List<Task>();
            _requestCancellation = new CancellationTokenSource();

            var loggerAdapter = new LoggerAdapter(loggerFactory.CreateLogger<ConnectionModel>());

            if (_bots == null)
                _bots = new List<TradeBotModel>();

            var toRemove = new List<TradeBotModel>();

            foreach (var bot in _bots)
            {
                try
                {
                    BotValidation?.Invoke(bot);
                }
                catch (Exception ex)
                {
                    _log.LogError("Bot '{0}' failed validation and was removed! {1}", bot.Id, ex);
                    toRemove.Add(bot);
                    continue;
                }

                if (bot.IsRunning)
                    _startedBotsCount++;
                InitBot(bot);
            }

            foreach (var bot in toRemove)
                _bots.Remove(bot);

            Connection = new ConnectionModel(new ConnectionOptions() { EnableFixLogs = false });
            Connection.Disconnected += () =>
            {
                lock (_sync)
                {
                    _lostConnection = true;
                    ManageConnection();
                }
            };

            var eventSyncAdapter = new SyncAdapter(syncObj);
            _core = new ClientCore(Connection, c => new SymbolManager(c, _sync), eventSyncAdapter, eventSyncAdapter);

            Account = new AccountModel(_core, AccountModelOptions.None);
            Symbols = (SymbolManager)_core.Symbols;
            FeedHistory = new FeedHistoryProviderModel(Connection, ServerModel.Environment.FeedHistoryCacheFolder,
                FeedHistoryFolderOptions.ServerClientHierarchy);
            TradeApi = new TradeExecutor(_core);

            ManageConnection();
        }

        public ConnectionStates ConnectionState { get; private set; }
        public object SyncObj => _sync;
        public ConnectionModel Connection { get; private set; }
        public IEnumerable<ITradeBot> TradeBots => _bots;
        public AccountModel Account { get; private set; }
        public SymbolManager Symbols { get; private set; }
        public Dictionary<string, CurrencyInfo> Currencies { get; private set; }
        public FeedHistoryProviderModel FeedHistory { get; private set; }
        public TradeExecutor TradeApi { get; private set; }
        public bool IsReconnecting
        {
            get
            {
                lock (_sync)
                {
                    return (_requests.Count > 0 || _startedBotsCount > 0)
                        && (ConnectionState == ConnectionStates.Disconnecting
                            || ConnectionState == ConnectionStates.Offline
                            || ConnectionState == ConnectionStates.Connecting)
                        && IsReconnectionPossible;
                }
            }
        }
        public bool IsReconnectionPossible
        {
            get
            {
                lock (_sync)
                {
                    return !(_currentErrorCode == ConnectionErrorCodes.BlockedAccount
                   || _currentErrorCode == ConnectionErrorCodes.LoginDeleted
                   || _currentErrorCode == ConnectionErrorCodes.InvalidCredentials);
                }
            }
        }
        public int RunningBotsCount => _startedBotsCount;
        public bool HasRunningBots => _startedBotsCount > 0;

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

        public Task<ConnectionInfo> GetInfo()
        {
            lock (_sync)
            {
                if (ConnectionState == ConnectionStates.Online)
                    return Task.FromResult(new ConnectionInfo(this));
                else
                {
                    return AddPendingRequest(
                        new Task<ConnectionInfo>(() =>
                        {
                            if (_lastErrorCode != ConnectionErrorCodes.None)
                                throw new CommunicationException("Connection error! Code: " + _lastErrorCode, _lastErrorCode);
                            return new ConnectionInfo(this);
                        }
                        , _requestCancellation.Token));
                }
            }
        }

        #region Connection Management



        public Task<ConnectionErrorCodes> TestConnection()
        {
            lock (_sync)
            {
                if (ConnectionState == ConnectionStates.Online)
                    return Task.FromResult(ConnectionErrorCodes.None);
                else
                    return AddPendingRequest(new Task<ConnectionErrorCodes>(() => _lastErrorCode, _requestCancellation.Token));
            }
        }

        private Task<T> AddPendingRequest<T>(Task<T> requestTask)
        {
            _requests.Add(requestTask);
            ManageConnection();
            return requestTask;
        }

        private void ExecRequests()
        {
            foreach (var req in _requests)
                req.RunSynchronously();
            _requests.Clear();
        }

        private void CancelRequests()
        {
            if (_requests.Count > 0)
            {
                _requestCancellation.Cancel();
                _requests.Clear();
                _requestCancellation = new CancellationTokenSource();
            }
        }

        private async Task ManageConnection()
        {
            if (ConnectionState == ConnectionStates.Offline)
            {
                if (_requests.Count > 0 || _startedBotsCount > 0)
                {
                    _disconnectCancellation?.Cancel();
                    _disconnectCancellation = null;

                    await Connect();
                }
            }
            else if (ConnectionState == ConnectionStates.Online)
            {
                if (_stopRequested || _lostConnection || _shutdownRequested)
                {
                    _disconnectCancellation?.Cancel();
                    _disconnectCancellation = null;

                    await Disconnect();
                }
                else if (_startedBotsCount == 0 && _disconnectCancellation == null)
                {
                    _disconnectCancellation = new CancellationTokenSource();
                    DisconnectAfter(_disconnectCancellation.Token, TimeSpan.FromMinutes(1)).Forget();
                }
            }
        }

        private async Task DisconnectAfter(CancellationToken token, TimeSpan delay)
        {
            await Task.Delay(delay, token);
            token.ThrowIfCancellationRequested();
            lock (_sync)
            {
                if (_stopRequested || _lostConnection || _startedBotsCount == 0)
                    Disconnect();
            }
        }

        public async Task ShutdownAsync()
        {
            Task[] stopBots;
            lock (_sync)
            {
                if (_shutdownRequested)
                    return;

                _shutdownRequested = true;
                stopBots = TradeBots.Select(tb => tb.StopAsync()).ToArray();
            }

            await Task.WhenAll(stopBots);
            await ManageConnection();
        }

        private async Task Disconnect()
        {
            ChangeState(ConnectionStates.Disconnecting);

            await Symbols.Deinit();
            await FeedHistory.Deinit();
            await Connection.DisconnectAsync();

            lock (_sync)
            {
                ChangeState(ConnectionStates.Offline);
                _shutdownRequested = false;
                _stopRequested = false;
                _lostConnection = false;
                _disconnectEvent.SetResult(1);
                ManageConnection();
            }
        }

        private void ChangeState(ConnectionStates newState)
        {
            ConnectionState = newState;
            StateChanged?.Invoke(this);
        }

        private async Task Connect()
        {
            _currentErrorCode = ConnectionErrorCodes.None;

            _disconnectEvent = new TaskCompletionSource<object>();

            ChangeState(ConnectionStates.Connecting);
            _connectCancellation = new CancellationTokenSource();

            _lastErrorCode = await Connection.Connect(Username, Password, Address, _connectCancellation.Token);
            _currentErrorCode = _lastErrorCode;

            if (_lastErrorCode == ConnectionErrorCodes.None)
            {
                await FeedHistory.Init();

                var fCache = Connection.FeedProxy.Cache;
                var tCache = Connection.TradeProxy.Cache;
                var symbols = fCache.Symbols;
                Currencies = fCache.Currencies.ToDictionary(c => c.Name);
                _core.Init();
                await Task.Delay(1500); // ugly fix! Need to wait till quotes snapshot is loaded. Normal solution will be possible after some updates in FDK
                Account.Init();
            }

            lock (_sync)
            {
                if (_lastErrorCode == ConnectionErrorCodes.None)
                {
                    _lostConnection = false;
                    ChangeState(ConnectionStates.Online);
                }
                else
                    ChangeState(ConnectionStates.Offline);
                ExecRequests();

                if (IsReconnectionPossible)
                    ManageConnection();
            }
        }

        //public void Change(string address, string username, string password)
        //{
        //    lock (_sync)
        //    {
        //        Address = address;
        //        Username = username;
        //        Password = password;
        //        testRequest?.TrySetCanceled();
        //        testRequest = null;
        //        stopRequested = true;
        //        connectCancellation?.Cancel();
        //        ManageConnection();
        //    }
        //}

        public void ChangePassword(string password)
        {
            lock (_sync)
            {
                Password = password;
                CancelRequests();
                if (ConnectionState != ConnectionStates.Offline && ConnectionState != ConnectionStates.Disconnecting)
                    _stopRequested = true;
                _connectCancellation?.Cancel();
                Changed?.Invoke(this);
                ManageConnection();
            }
        }

        #endregion Connection Management

        #region Bot Management

        public ITradeBot AddBot(TradeBotModelConfig config)
        {
            lock (_sync)
            {
                var package = _packageProvider.Get(config.Plugin.PackageName);

                if (package == null)
                    throw new PackageNotFoundException($"Package '{config.Plugin.PackageName}' cannot be found!");

                var newBot = new TradeBotModel(config);
                BotValidation?.Invoke(newBot);
                InitBot(newBot);
                _bots.Add(newBot);
                ManageConnection();
                BotChanged?.Invoke(newBot, ChangeAction.Added);
                return newBot;
            }
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
                ManageConnection();
            }
        }

        public void RemoveBot(string botId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            lock (_sync)
            {
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
        }

        public void RemoveAllBots()
        {
            lock (_sync)
            {
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
        }

        private void InitBot(TradeBotModel bot)
        {
            bot.IsRunningChanged += OnBotIsRunningChanged;
            bot.ConfigurationChanged += OnBotConfigurationChanged;
            bot.StateChanged += OnBotStateChanged;
            bot.Init(this, _loggerFactory, _sync, _packageProvider, null, ServerModel.GetWorkingFolderFor(bot.Id));
            BotInitialized?.Invoke(bot);
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

        internal void RemoveBotsFromPackage(PackageModel package)
        {
            var toRemove = _bots.Where(b => b.Package == package).ToList();
            toRemove.ForEach(b => _bots.Remove(b));
        }

       

        #endregion
    }
}
