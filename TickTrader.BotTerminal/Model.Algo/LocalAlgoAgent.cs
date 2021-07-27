using Machinarium.Qnil;
using NLog;
using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Account;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Indicators.Trend.MovingAverage;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server;
using TickTrader.Algo.Server.Persistence;
using TickTrader.Algo.ServerControl;
using File = System.IO.File;

namespace TickTrader.BotTerminal
{
    internal class LocalAlgoAgent : IAlgoAgent, IAlgoSetupMetadata, IAlgoPluginHost, IAlgoSetupContext
    {
        public const string LocalAgentName = "AlgoTerminal";

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly ApiMetadataInfo _apiMetadata = ApiMetadataInfo.CreateCurrentMetadata();
        private static readonly ISetupSymbolInfo _defaultSymbol = new SymbolToken("none");

        private readonly ReductionCollection _reductions;
        private readonly MappingCollectionInfo _mappingsInfo;
        private ISyncContext _syncContext;
        private VarDictionary<string, PackageInfo> _packages;
        private VarDictionary<PluginKey, PluginInfo> _plugins;
        private VarDictionary<string, AccountModelInfo> _accounts;
        private BotsWarden _botsWarden;
        private VarDictionary<string, TradeBotModel> _bots;
        private PreferencesStorageModel _preferences;

        public string Name => LocalAgentName;

        public bool IsRemote => false;

        public IVarSet<string, PackageInfo> Packages => _packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _plugins;

        public IVarSet<string, AccountModelInfo> Accounts => _accounts;

        public IVarSet<string, ITradeBot> Bots { get; }

        public PluginCatalog Catalog { get; }

        IPluginIdProvider IAlgoAgent.IdProvider => IdProvider;

        public bool SupportsAccountManagement => false;

        public AccessManager AccessManager { get; }

        public PluginIdProvider IdProvider { get; }

        public MappingCollectionInfo Mappings { get; }

        public TraderClientModel ClientModel { get; }

        public IShell Shell { get; }

        public IAlertModel AlertModel { get; }

        public int RunningBotsCnt => _bots.Snapshot.Values.Count(b => !b.State.IsStopped());

        public bool HasRunningBots => _bots.Snapshot.Values.Any(b => !b.State.IsStopped());

        public event Action<PackageInfo> PackageStateChanged;

        public event Action<AccountModelInfo> AccountStateChanged;

        public event Action<ITradeBot> BotStateChanged;

        public event Action<ITradeBot> BotUpdated;

        public event Action AccessLevelChanged { add { } remove { } }


        internal LocalAlgoServer AlgoServer { get; }


        public LocalAlgoAgent(IShell shell, TraderClientModel clientModel, PersistModel storage)
        {
            Shell = shell;
            ClientModel = clientModel;
            _preferences = storage.PreferencesStorage.StorageModel;

            AlgoServer = new LocalAlgoServer();
            AlgoServer.EventBus.PackageUpdated.Subscribe(OnPackageUpdated);

            Task.Factory.StartNew(() => StartServer(storage));//.GetAwaiter().GetResult();

            _reductions = new ReductionCollection();
            IdProvider = new PluginIdProvider();
            _botsWarden = new BotsWarden(this);
            _syncContext = new DispatcherSync();
            _packages = new VarDictionary<string, PackageInfo>();
            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            _accounts = new VarDictionary<string, AccountModelInfo>();
            _bots = new VarDictionary<string, TradeBotModel>();
            AlertModel = new AlgoAlertModel(Name);
            Bots = _bots.Select((k, v) => (ITradeBot)v);

            ClientModel.Connected += ClientModelOnConnected;
            ClientModel.Disconnected += ClientModelOnDisconnected;
            ClientModel.Connection.StateChanged += ClientConnectionOnStateChanged;

            _reductions.LoadDefaultReductions();
            //_reductions.LoadReductions(EnvService.Instance.AlgoExtFolder, SharedConstants.LocalRepositoryId);

            Mappings = _reductions.CreateMappings();
            Catalog = new PluginCatalog(this);
            AccessManager = new AccessManager(ClientClaims.Types.AccessLevel.Admin);
        }


        private async Task StartServer(PersistModel storage)
        {
            var settings = new AlgoServerSettings();
            settings.DataFolder = AppDomain.CurrentDomain.BaseDirectory;
            settings.PkgStorage.Assemblies.Add(typeof(MovingAverage).Assembly);
            settings.PkgStorage.AddLocation(SharedConstants.LocalRepositoryId, EnvService.Instance.AlgoRepositoryFolder);
            settings.PkgStorage.UploadLocationId = SharedConstants.LocalRepositoryId;
            if (EnvService.Instance.AlgoCommonRepositoryFolder != null)
                settings.PkgStorage.AddLocation(SharedConstants.CommonRepositoryId, EnvService.Instance.AlgoCommonRepositoryFolder);

            await AlgoServer.Init(settings);
            if (await AlgoServer.NeedLegacyState())
            {
                var serverSavedState = BuildServerSavedState(storage);
                await AlgoServer.LoadLegacyState(serverSavedState);
            }
            await AlgoServer.Start();
        }

        private ServerSavedState BuildServerSavedState(PersistModel model)
        {
            var state = new ServerSavedState();

            state = AddAccountsSavedStates(state, model);
            state = AddPluginsSavedStates(state, model);

            return state;
        }

        private static ServerSavedState AddAccountsSavedStates(ServerSavedState state, PersistModel model)
        {
            foreach (var acc in model.AuthSettingsStorage.Accounts.Values)
            {
                var accState = new AccountSavedState
                {
                    Id = AccountId.Pack(acc.ServerAddress, acc.Login),
                    UserId = acc.Login,
                    Server = acc.ServerAddress,
                };

                if (acc.HasPassword)
                    accState.PackCreds(new AccountCreds(acc.Password));

                state.Accounts.Add(accState.Id, accState);
            }

            return state;
        }

        private static ServerSavedState AddPluginsSavedStates(ServerSavedState state, PersistModel model)
        {
            model.ProfileManager.Stop(); // stop stub.profile

            var accLogin = model.AuthSettingsStorage.LastLogin;
            var accServer = model.AuthSettingsStorage.LastServer;

            if (string.IsNullOrEmpty(accLogin) || string.IsNullOrEmpty(accServer))
                return state;

            model.ProfileManager.LoadCachedProfile(accServer, accLogin);

            foreach (var config in model.ProfileManager.CurrentProfile.Bots.Select(u => u.Config))
            {
                var pluginState = new PluginSavedState
                {
                    Id = config.InstanceId,
                    AccountId = AccountId.Pack(accServer, accLogin),
                    IsRunning = false,
                };

                pluginState.PackConfig(config.ToDomain());

                state.Plugins.Add(pluginState.Id, pluginState);
            }

            model.ProfileManager.Stop();

            return state;
        }

        public Task<SetupMetadata> GetSetupMetadata(string accountId, SetupContextInfo setupContext)
        {
            var accountMetadata = new AccountMetadataInfo(AccountId.Pack(ClientModel.Connection.CurrentServer, ClientModel.Connection.CurrentLogin),
                ClientModel.SortedSymbols.Select(s => s.ToInfo()).ToList(), ClientModel.Cache.GetDefaultSymbol().ToInfo());
            var res = new SetupMetadata(_apiMetadata, Mappings, accountMetadata, setupContext ?? this.GetSetupContextInfo());
            return Task.FromResult(res);
        }

        public Task StartBot(string instanceId)
        {
            if (_bots.TryGetValue(instanceId, out var bot))
            {
                return bot.Start();
            }
            return Task.FromResult(this);
        }

        public Task StopBot(string instanceId)
        {
            if (_bots.TryGetValue(instanceId, out var bot))
            {
                return bot.Stop();
            }
            return Task.FromResult(this);
        }

        public Task AddBot(string accountId, PluginConfig config)
        {
            var bot = new TradeBotModel(config, this, this, this, Accounts.Snapshot.Values.First().AccountId);
            IdProvider.RegisterPluginId(bot.InstanceId);
            _bots.Add(bot.InstanceId, bot);
            bot.StateChanged += OnBotStateChanged;
            bot.Updated += OnBotUpdated;
            return Task.FromResult(this);
        }

        public Task RemoveBot(string instanceId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            if (_bots.TryGetValue(instanceId, out var bot))
            {
                IdProvider.UnregisterPlugin(instanceId);
                _bots.Remove(instanceId);
                bot.StateChanged -= OnBotStateChanged;
                bot.Updated -= OnBotUpdated;
            }
            return Task.FromResult(this);
        }

        public Task ChangeBotConfig(string instanceId, PluginConfig newConfig)
        {
            if (_bots.TryGetValue(instanceId, out var bot))
            {
                bot.Configurate(newConfig);
            }
            return Task.FromResult(this);
        }

        public Task AddAccount(AddAccountRequest request)
        {
            throw new NotSupportedException();
        }

        public Task RemoveAccount(RemoveAccountRequest request)
        {
            throw new NotSupportedException();
        }

        public Task ChangeAccount(ChangeAccountRequest request)
        {
            throw new NotSupportedException();
        }

        public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            throw new NotSupportedException();
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            throw new NotSupportedException();
        }

        public Task UploadPackage(string fileName, string srcFilePath, IFileProgressListener progressListener)
        {
            var dstFilePath = Path.Combine(EnvService.Instance.AlgoRepositoryFolder, fileName);
            progressListener.Init(0);
            File.Copy(srcFilePath, dstFilePath, true);
            progressListener.IncrementProgress(new FileInfo(srcFilePath).Length);
            return Task.FromResult(this);
        }

        public Task RemovePackage(string packageId)
        {
            if (!_packages.TryGetValue(packageId, out var package))
                throw new ArgumentException("Can't resolve path to Algo package location");

            File.Delete(package.Identity.FilePath);
            return Task.CompletedTask;
        }

        public Task DownloadPackage(string packageId, string dstFilePath, IFileProgressListener progressListener)
        {
            if (!_packages.TryGetValue(packageId, out var package))
                throw new ArgumentException("Can't resolve path to Algo package location");

            var srcFilePath = package.Identity.FilePath;
            progressListener.Init(0);
            File.Copy(srcFilePath, dstFilePath, true);
            progressListener.IncrementProgress(new FileInfo(dstFilePath).Length);
            return Task.FromResult(new byte[0]);
        }

        public Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            var path = GetBotFolderPath(botId, folderId);
            var res = new PluginFolderInfo
            {
                PluginId = botId,
                FolderId = folderId,
                Path = path,
            };
            if (Directory.Exists(path))
                res.Files.AddRange(new DirectoryInfo(path).GetFiles().Select(f => new PluginFileInfo { Name = f.Name, Size = f.Length }));
            return Task.FromResult(res);
        }

        public Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            throw new NotSupportedException();
        }

        public Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
        {
            throw new NotSupportedException();
        }

        public Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, IFileProgressListener progressListener)
        {
            throw new NotSupportedException();
        }

        public Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, IFileProgressListener progressListener)
        {
            //throw new NotSupportedException();
            // used in bot setup
            return Task.FromResult(this);
        }


        private void OnPackageStateChanged(PackageInfo package)
        {
            PackageStateChanged?.Invoke(package);
        }

        private void OnAccountStateChanged(AccountModelInfo account)
        {
            AccountStateChanged?.Invoke(account);
        }

        private void OnBotStateChanged(ITradeBot bot)
        {
            if (Bots.Snapshot.TryGetValue(bot.InstanceId, out var botModel))
            {
                BotStateChanged?.Invoke(botModel);
            }
        }

        private void OnBotUpdated(ITradeBot bot)
        {
            if (Bots.Snapshot.TryGetValue(bot.InstanceId, out var botModel))
            {
                BotUpdated?.Invoke(botModel);
            }
        }

        private void ClientConnectionOnStateChanged(ConnectionModel.States oldState, ConnectionModel.States newState)
        {
            var server = ClientModel.Connection.CurrentServer;
            var userId = ClientModel.Connection.CurrentLogin;

            var accountId = AccountId.Pack(server, userId);
            if (_accounts.TryGetValue(accountId, out var account))
            {
                account.ConnectionState = ClientModel.Connection.State.ToInfo();
                account.LastError = ClientModel.Connection.LastError;
                OnAccountStateChanged(account);
            }
            else
            {
                _accounts.Clear();
                account = new AccountModelInfo
                {
                    AccountId = accountId,
                    ConnectionState = ClientModel.Connection.State.ToInfo(),
                    LastError = ClientModel.Connection.LastError,
                    DisplayName = $"{server} - {userId}"
                };
                _accounts.Add(accountId, account);
            }

            StopRunningBotsOnBlockedAccount();
        }

        private void OnPackageUpdated(PackageUpdate update)
        {
            var pkgId = update.Id;
            var newPkg = update.Package;
            _packages.TryGetValue(update.Id, out var oldPkg);
            switch (update.Action)
            {
                case Update.Types.Action.Added:
                case Update.Types.Action.Updated:
                    _packages[pkgId] = newPkg;
                    UpdatePlugins(oldPkg, newPkg);
                    break;
                case Update.Types.Action.Removed:
                    _packages.Remove(pkgId);
                    break;
                default:
                    break;
            }
            UpdatePlugins(oldPkg, newPkg);
        }

        private void UpdatePlugins(PackageInfo oldPackage, PackageInfo newPackage)
        {
            var newPlugins = newPackage?.Plugins ?? Enumerable.Empty<PluginInfo>();
            // upsert
            foreach (var plugin in newPlugins)
            {
                if (!_plugins.ContainsKey(plugin.Key))
                {
                    _plugins.Add(plugin.Key, plugin);
                }
                else
                {
                    _plugins[plugin.Key] = plugin;
                }
            }

            if (oldPackage != null)
            {
                // remove
                var newPluginsLookup = newPlugins.ToDictionary(p => p.Key);
                foreach (var plugin in oldPackage.Plugins)
                {
                    if (!newPluginsLookup.ContainsKey(plugin.Key))
                    {
                        _plugins.Remove(plugin.Key);
                    }
                }
            }
        }

        private string GetBotFolderPath(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            switch (folderId)
            {
                case PluginFolderInfo.Types.PluginFolderId.AlgoData:
                    return Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.Escape(botId));
                case PluginFolderInfo.Types.PluginFolderId.BotLogs:
                    return Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.Escape(botId));
                default:
                    throw new ArgumentException("Unknown bot folder id");
            }
        }


        #region Local bots management

        public async Task Shutdown(bool stopAlgoServer = true)
        {
            try
            {
                await Task.WhenAll(_bots.Values.Where(b => b.State == PluginModelInfo.Types.PluginState.Running).Select(b => b.Stop()));
            }
            catch (AggregateException agEx)
            {
                foreach (var e in agEx.InnerExceptions)
                {
                    _logger.Error(e, "Failed to stop bots");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop bots");
            }

            if (stopAlgoServer)
                try
                {
                    await Task.Run(() => AlgoServer.Stop());
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to stop AlgoServer");
                }
        }

        public void RemoveAllBots(CancellationToken token)
        {
            foreach (var instanceId in _bots.Snapshot.Keys.ToList())
            {
                if (token.IsCancellationRequested)
                    return;

                RemoveBot(instanceId);
            }
        }

        public void SaveBotsSnapshot(ProfileStorageModel profileStorage)
        {
            try
            {
                profileStorage.Bots = _bots.Snapshot.Values.Select(b => new TradeBotStorageEntry
                {
                    Started = b.State.IsRunning(),
                    Config = Algo.Core.Config.PluginConfig.FromDomain(b.Config),
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save bots snapshot");
            }
        }

        public void LoadBotsSnapshot(ProfileStorageModel profileStorage, CancellationToken token)
        {
            try
            {
                if ((profileStorage.Bots?.Count ?? 0) == 0)
                {
                    _logger.Info($"Bots snapshot is empty");
                    return;
                }

                _logger.Info($"Loading bots snapshot({profileStorage.Bots.Count} bots)");

                foreach (var bot in profileStorage.Bots)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    RestoreTradeBot(bot);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load bots snapshot");
            }
        }


        private void RestoreTradeBot(TradeBotStorageEntry entry)
        {
            if (entry.Config == null)
            {
                _logger.Error("Trade bot not configured!");
            }
            if (entry.Config.Key == null)
            {
                _logger.Error("Trade bot key missing!");
            }

            AddBot(null, entry.Config.ToDomain());
            if (entry.Started && _preferences.RestartBotsOnStartup)
                StartBot(entry.Config.InstanceId);
        }

        private void StopRunningBotsOnBlockedAccount()
        {
            if (ClientModel.Connection.LastError?.Code == ConnectionErrorInfo.Types.ErrorCode.BlockedAccount)
                _bots.Snapshot.Values.Where(b => b.State.IsRunning()).ForEach(b => b.Stop().Forget());
        }

        #endregion


        #region IAlgoSetupMetadata

        public IReadOnlyList<ISetupSymbolInfo> Symbols => ClientModel.SortedSymbols.Select(u => (ISetupSymbolInfo)u.ToKey()).ToList();

        IPluginIdProvider IAlgoSetupMetadata.IdProvider => IdProvider;

        #endregion IAlgoSetupMetadata implementation


        #region IAlgoSetupContext

        Feed.Types.Timeframe IAlgoSetupContext.DefaultTimeFrame => Feed.Types.Timeframe.M1;

        ISetupSymbolInfo IAlgoSetupContext.DefaultSymbol => _defaultSymbol;

        MappingKey IAlgoSetupContext.DefaultMapping => MappingDefaults.DefaultBarToBarMapping.Key;

        #endregion


        private readonly ConcurrentQueue<Action> _startQueue = new ConcurrentQueue<Action>();
        private bool _isStartQueueRunning = false;

        private void StartQueueLoop()
        {
            while (_startQueue.TryDequeue(out var action))
            {
                action();
            }

            _isStartQueueRunning = false;
        }

        #region IAlgoPluginHost

        ITimeVectorRef IPluginDataChartModel.TimeSyncRef => null;
        AxisBase IPluginDataChartModel.CreateXAxis() => null;

        void IAlgoPluginHost.Lock()
        {
            Shell.ConnectionLock.Lock();
        }

        void IAlgoPluginHost.Unlock()
        {
            Shell.ConnectionLock.Release();
        }

        void IAlgoPluginHost.EnqueueStartAction(Action action)
        {
            _startQueue.Enqueue(action);

            if (!_isStartQueueRunning)
            {
                _isStartQueueRunning = true;
                Task.Factory.StartNew(() => StartQueueLoop());
            }
        }

        ITradeExecutor IAlgoPluginHost.GetTradeApi()
        {
            return ClientModel.TradeApi;
        }

        ITradeHistoryProvider IAlgoPluginHost.GetTradeHistoryApi()
        {
            return ClientModel.TradeHistory.AlgoAdapter;
        }

        string IAlgoPluginHost.GetConnectionInfo()
        {
            return $"account {ClientModel.Connection.CurrentLogin} on {ClientModel.Connection.CurrentServer} using {ClientModel.Connection.CurrentProtocol}";
        }

        public virtual void InitializePlugin(ExecutorConfig config)
        {
            config.InitPriorityInvokeStrategy();
            config.InitBarStrategy(Feed.Types.MarketSide.Bid);
            config.InitSlidingBuffering(4000);
        }

        bool IExecStateObservable.IsStarted => false;

        public event Action ParamsChanged = delegate { };
        public event Action StartEvent = delegate { };
        public event AsyncEventHandler StopEvent = delegate { return Task.CompletedTask; };
        public event Action Connected;
        public event Action Disconnected;


        private void ClientModelOnConnected()
        {
            Connected?.Invoke();
        }

        private void ClientModelOnDisconnected()
        {
            Disconnected?.Invoke();
        }

        #endregion
    }

}
