using Machinarium.ActorModel;
using Machinarium.State;
using Microsoft.Extensions.Logging;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.DedicatedServer.DS.Repository;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.Infrastructure;

namespace TickTrader.DedicatedServer.DS.Models
{
    [DataContract(Name = "account", Namespace = "")]
    public class ClientModel : IAccount
    {
        private object _sync;
        private ILogger _log;
        //private object _syncEvents = new object();
        private CancellationTokenSource connectCancellation;
        private TaskCompletionSource<ConnectionErrorCodes> testRequest;
        private ClientCore _core;

        [DataMember(Name = "bots")]
        private List<TradeBotModel> _bots = new List<TradeBotModel>();
        private Func<string, PackageModel> _packageProvider;

        private bool stopRequested;
        private bool lostConnection;
        private int _startedBotsCount;

        public ClientModel(string address, string username, string password)
        {
            Address = address;
            Username = username;
            Password = password;
        }

        public void Init(object syncObj, ILoggerFactory loggerFactory, Func<string, PackageModel> packageProvider)
        {
            _sync = syncObj;
            _packageProvider = packageProvider;
            _log = loggerFactory.CreateLogger<ClientModel>();
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
                    lostConnection = true;
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

        #region Connection Management

        public async Task<ConnectionErrorCodes> TestConnection()
        {
            Task<ConnectionErrorCodes> resultTask = null;

            lock (_sync)
            {
                if (ConnectionState == ConnectionStates.Online)
                    return ConnectionErrorCodes.None;
                else
                {
                    if (testRequest == null)
                    {
                        testRequest = new TaskCompletionSource<ConnectionErrorCodes>();
                        ManageConnection();
                    }

                    resultTask = testRequest.Task;
                }
            }

            return await resultTask;
        }

        private void ManageConnection()
        {
            if (ConnectionState == ConnectionStates.Offline)
            {
                if (testRequest != null || _startedBotsCount > 0)
                    Connect();
            }
            else if (ConnectionState == ConnectionStates.Online)
            {
                if (stopRequested || lostConnection || _startedBotsCount == 0)
                    Disconnect();
            }
        }

        private async void Disconnect()
        {
            ChangeState(ConnectionStates.Disconnecting);

            await Symbols.Deinit();
            await FeedHistory.Deinit();
            await Connection.DisconnectAsync();

            lock (_sync)
            {
                ChangeState(ConnectionStates.Offline);
                stopRequested = false;
                lostConnection = false;
                ManageConnection();
            }
        }

        private void ChangeState(ConnectionStates newState)
        {
            ConnectionState = newState;
            StateChanged?.Invoke(this);
        }

        private async void Connect()
        {
            ChangeState(ConnectionStates.Connecting);
            connectCancellation = new CancellationTokenSource();
            var result = await Connection.Connect(Username, Password, Address, connectCancellation.Token);

            if (result == ConnectionErrorCodes.None)
            {
                await FeedHistory.Init();

                var fCache = Connection.FeedProxy.Cache;
                var tCache = Connection.TradeProxy.Cache;
                var symbols = fCache.Symbols;
                Currencies = fCache.Currencies.ToDictionary(c => c.Name);
                _core.Init();
                Symbols.Initialize(symbols, Currencies);
                Account.Init();
            }

            lock (_sync)
            {
                if (result == ConnectionErrorCodes.None)
                    ChangeState(ConnectionStates.Online);
                else
                    ChangeState(ConnectionStates.Offline);
                testRequest?.TrySetResult(result);
                testRequest = null;
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
                testRequest?.TrySetCanceled();
                testRequest = null;
                stopRequested = true;
                connectCancellation?.Cancel();
                Changed?.Invoke(this);
                ManageConnection();
            }
        }

        #endregion Connection Management

        #region Bot Management

        public ITradeBot AddBot(string botId, PluginKey pluginId, PluginConfig botConfig)
        {
            lock (_sync)
            {
                var package = _packageProvider(pluginId.PackageName);

                if (package == null)
                    throw new PackageNotFoundException($"Package '{pluginId.PackageName}' cannot be found!");

                var newBot = new TradeBotModel(botId, pluginId, botConfig);
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
            BotChanged?.Invoke(bot, ChangeAction.Modified);
            ManageConnection();
        }

        public void RemoveBot(string botId)
        {
            lock (_sync)
            {
                var bot = _bots.FirstOrDefault(b => b.Id == botId);
                if (bot != null)
                {
                    if (bot.IsRunning)
                        throw new InvalidStateException("Cannot remove running bot!");
                    _bots.Remove(bot);
                    DeinitBot(bot);
                    BotChanged?.Invoke(bot, ChangeAction.Removed);
                }
            }
        }

        private void InitBot(TradeBotModel bot)
        {
            bot.IsRunningChanged += OnBotIsRunningChanged;
            bot.ConfigurationChanged += OnBotConfigurationChanged;
            bot.StateChanged += OnBotStateChanged;
            bot.Init(this, _log, _sync, _packageProvider, null);
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
