using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Machinarium.Qnil;
using NLog;
using SciChart.Charting.Visuals.Axes;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Protocol;
using TickTrader.BotTerminal.Lib;
using File = System.IO.File;

namespace TickTrader.BotTerminal
{
    internal class LocalAlgoAgent : IAlgoAgent, IAlgoSetupMetadata, IAlgoPluginHost, IAlgoSetupContext
    {
        public const string LocalAgentName = "BotTerminal";

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly ApiMetadataInfo _apiMetadata = ApiMetadataInfo.CreateCurrentMetadata();
        private static readonly ISetupSymbolInfo _defaultSymbol = new SymbolToken("none");

        private readonly ReductionCollection _reductions;
        private readonly MappingCollectionInfo _mappingsInfo;
        private ISyncContext _syncContext;
        private VarDictionary<PackageKey, PackageInfo> _packages;
        private VarDictionary<PluginKey, PluginInfo> _plugins;
        private VarDictionary<AccountKey, AccountModelInfo> _accounts;
        private BotsWarden _botsWarden;
        private VarDictionary<string, TradeBotModel> _bots;
        private PreferencesStorageModel _preferences;

        public string Name => LocalAgentName;

        public bool IsRemote => false;

        public IVarSet<PackageKey, PackageInfo> Packages => _packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _plugins;

        public IVarSet<AccountKey, AccountModelInfo> Accounts => _accounts;

        public IVarSet<string, ITradeBot> Bots { get; }

        public PluginCatalog Catalog { get; }

        IPluginIdProvider IAlgoAgent.IdProvider => IdProvider;

        public bool SupportsAccountManagement => false;

        public AccessManager AccessManager { get; }

        public PluginIdProvider IdProvider { get; }

        public MappingCollection Mappings { get; }

        public LocalAlgoLibrary Library { get; }

        public TraderClientModel ClientModel { get; }

        public IShell Shell { get; }

        public IAlertModel AlertModel { get; }

        public int RunningBotsCnt => _bots.Snapshot.Values.Count(b => !PluginStateHelper.IsStopped(b.State));

        public bool HasRunningBots => _bots.Snapshot.Values.Any(b => !PluginStateHelper.IsStopped(b.State));

        public event Action<PackageInfo> PackageStateChanged;

        public event Action<AccountModelInfo> AccountStateChanged;

        public event Action<ITradeBot> BotStateChanged;

        public event Action<ITradeBot> BotUpdated;

        public event Action AccessLevelChanged { add { } remove { } }


        internal AlgoServer AlgoServer { get; }


        public LocalAlgoAgent(IShell shell, TraderClientModel clientModel, PersistModel storage)
        {
            Shell = shell;
            ClientModel = clientModel;
            _preferences = storage.PreferencesStorage.StorageModel;

            AlgoServer = new AlgoServer();
            AlgoServer.Start().GetAwaiter().GetResult();
            _logger.Info($"Started AlgoServer on port {AlgoServer.BoundPort}");

            _reductions = new ReductionCollection(new AlgoLogAdapter("Extensions"));
            IdProvider = new PluginIdProvider();
            Library = new LocalAlgoLibrary(new AlgoLogAdapter("AlgoRepository"));
            _botsWarden = new BotsWarden(this);
            _syncContext = new DispatcherSync();
            _packages = new VarDictionary<PackageKey, PackageInfo>();
            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            _accounts = new VarDictionary<AccountKey, AccountModelInfo>();
            _bots = new VarDictionary<string, TradeBotModel>();
            AlertModel = new AlgoAlertModel(Name);
            Bots = _bots.Select((k, v) => (ITradeBot)v);

            Library.PackageUpdated += LibraryOnPackageUpdated;
            Library.PluginUpdated += LibraryOnPluginUpdated;
            Library.PackageStateChanged += OnPackageStateChanged;
            Library.Reset += LibraryOnReset;
            ClientModel.Connected += ClientModelOnConnected;
            ClientModel.Disconnected += ClientModelOnDisconnected;
            ClientModel.Connection.StateChanged += ClientConnectionOnStateChanged;

            Library.AddAssemblyAsPackage(Assembly.Load("TickTrader.Algo.Indicators"));
            Library.RegisterRepositoryLocation(RepositoryLocation.LocalRepository, EnvService.Instance.AlgoRepositoryFolder, Properties.Settings.Default.EnablePluginIsolation);
            if (EnvService.Instance.AlgoCommonRepositoryFolder != null)
                Library.RegisterRepositoryLocation(RepositoryLocation.CommonRepository, EnvService.Instance.AlgoCommonRepositoryFolder, Properties.Settings.Default.EnablePluginIsolation);

            _reductions.LoadReductions(EnvService.Instance.AlgoExtFolder, RepositoryLocation.LocalExtensions);

            Mappings = new MappingCollection(_reductions);
            _mappingsInfo = Mappings.ToInfo();
            Catalog = new PluginCatalog(this);
            AccessManager = new AccessManager(AccessLevels.Admin);
        }


        public Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext)
        {
            var accountMetadata = new AccountMetadataInfo(new AccountKey(ClientModel.Connection.CurrentServer, ClientModel.Connection.CurrentLogin),
                ClientModel.SortedSymbols.Select(s => s.ToInfo()).ToList(), ClientModel.Cache.GetDefaultSymbol().ToInfo());
            var res = new SetupMetadata(_apiMetadata, _mappingsInfo, accountMetadata, setupContext ?? this.GetSetupContextInfo());
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

        public Task AddBot(AccountKey account, Algo.Common.Model.Config.PluginConfig config)
        {
            var bot = new TradeBotModel(config, this, this, this, Accounts.Snapshot.Values.First().Key);
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

        public Task ChangeBotConfig(string instanceId, Algo.Common.Model.Config.PluginConfig newConfig)
        {
            if (_bots.TryGetValue(instanceId, out var bot))
            {
                bot.Configurate(newConfig);
            }
            return Task.FromResult(this);
        }

        public Task AddAccount(AccountKey account, string password)
        {
            throw new NotSupportedException();
        }

        public Task RemoveAccount(AccountKey account)
        {
            throw new NotSupportedException();
        }

        public Task ChangeAccount(AccountKey account, string password)
        {
            throw new NotSupportedException();
        }

        public Task<ConnectionErrorInfo> TestAccount(AccountKey account)
        {
            throw new NotSupportedException();
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password)
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

        public Task RemovePackage(PackageKey package)
        {
            string filePath = null;
            switch (package.Location)
            {
                case RepositoryLocation.LocalRepository:
                    filePath = Path.Combine(EnvService.Instance.AlgoRepositoryFolder, package.Name);
                    break;
                case RepositoryLocation.LocalExtensions:
                    filePath = Path.Combine(EnvService.Instance.AlgoExtFolder, package.Name);
                    break;
                case RepositoryLocation.CommonRepository:
                    filePath = Path.Combine(EnvService.Instance.AlgoCommonRepositoryFolder, package.Name);
                    break;
                default:
                    throw new ArgumentException("Can't resolve path to package location");
            }
            File.Delete(filePath);
            return Task.FromResult(this);
        }

        public Task DownloadPackage(PackageKey package, string dstFilePath, IFileProgressListener progressListener)
        {
            string srcFilePath = null;
            switch (package.Location)
            {
                case RepositoryLocation.LocalRepository:
                    srcFilePath = Path.Combine(EnvService.Instance.AlgoRepositoryFolder, package.Name);
                    break;
                case RepositoryLocation.LocalExtensions:
                    srcFilePath = Path.Combine(EnvService.Instance.AlgoExtFolder, package.Name);
                    break;
                case RepositoryLocation.CommonRepository:
                    srcFilePath = Path.Combine(EnvService.Instance.AlgoCommonRepositoryFolder, package.Name);
                    break;
                default:
                    throw new ArgumentException("Can't resolve path to package location");
            }
            progressListener.Init(0);
            File.Copy(srcFilePath, dstFilePath, true);
            progressListener.IncrementProgress(new FileInfo(dstFilePath).Length);
            return Task.FromResult(new byte[0]);
        }

        public Task<BotFolderInfo> GetBotFolderInfo(string botId, BotFolderId folderId)
        {
            var path = GetBotFolderPath(botId, folderId);
            var res = new BotFolderInfo
            {
                BotId = botId,
                FolderId = folderId,
                Path = path,
            };
            if (Directory.Exists(path))
                res.Files = new DirectoryInfo(path).GetFiles().Select(f => new BotFileInfo { Name = f.Name, Size = f.Length }).ToList();
            return Task.FromResult(res);
        }

        public Task ClearBotFolder(string botId, BotFolderId folderId)
        {
            throw new NotSupportedException();
        }

        public Task DeleteBotFile(string botId, BotFolderId folderId, string fileName)
        {
            throw new NotSupportedException();
        }

        public Task DownloadBotFile(string botId, BotFolderId folderId, string fileName, string dstPath, IFileProgressListener progressListener)
        {
            throw new NotSupportedException();
        }

        public Task UploadBotFile(string botId, BotFolderId folderId, string fileName, string srcPath, IFileProgressListener progressListener)
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
            var accountKey = new AccountKey(ClientModel.Connection.CurrentServer, ClientModel.Connection.CurrentLogin);
            if (_accounts.TryGetValue(accountKey, out var account))
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
                    Key = accountKey,
                    ConnectionState = ClientModel.Connection.State.ToInfo(),
                    LastError = ClientModel.Connection.LastError,
                };
                _accounts.Add(accountKey, account);
            }

            StopRunningBotsOnBlockedAccount();
        }

        private void LibraryOnPackageUpdated(UpdateInfo<PackageInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var package = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                        _packages.Add(package.Key, package);
                        break;
                    case UpdateType.Replaced:
                        _packages[package.Key] = package;
                        break;
                    case UpdateType.Removed:
                        _packages.Remove(package.Key);
                        break;
                }
            });
        }

        private void LibraryOnPluginUpdated(UpdateInfo<PluginInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var plugin = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                        _plugins.Add(plugin.Key, plugin);
                        break;
                    case UpdateType.Replaced:
                        _plugins[plugin.Key] = plugin;
                        break;
                    case UpdateType.Removed:
                        _plugins.Remove(plugin.Key);
                        break;
                }
            });
        }

        private void LibraryOnReset()
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                _plugins.Clear();
            });
        }

        private string GetBotFolderPath(string botId, BotFolderId folderId)
        {
            switch (folderId)
            {
                case BotFolderId.AlgoData:
                    return Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.GetSafeFileName(botId));
                case BotFolderId.BotLogs:
                    return Path.Combine(EnvService.Instance.AlgoWorkingFolder, PathHelper.GetSafeFileName(botId));
                default:
                    throw new ArgumentException("Unknown bot folder id");
            }
        }


        #region Local bots management

        public void StopBots()
        {
            _bots.Values.Foreach(StopBot);
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
                    Started = PluginStateHelper.IsRunning(b.State),
                    Config = b.Config,
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


        private async void StopBot(TradeBotModel bot)
        {
            if (bot.State == PluginStates.Running)
            {
                await bot.Stop();
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

            AddBot(null, entry.Config);
            if (entry.Started && _preferences.RestartBotsOnStartup)
                StartBot(entry.Config.InstanceId);
        }

        private void StopRunningBotsOnBlockedAccount()
        {
            if (ClientModel.Connection.LastError?.Code == ConnectionErrorCodes.BlockedAccount)
                _bots.Snapshot.Values.Where(b => PluginStateHelper.IsRunning(b.State)).Foreach(b => b.Stop().Forget());
        }

        #endregion


        #region IAlgoSetupMetadata

        public IReadOnlyList<ISetupSymbolInfo> Symbols => ClientModel.SortedSymbols.Select(u => (ISetupSymbolInfo)u.ToKey()).ToList();

        IPluginIdProvider IAlgoSetupMetadata.IdProvider => IdProvider;

        #endregion IAlgoSetupMetadata implementation


        #region IAlgoSetupContext

        Feed.Types.Timeframe IAlgoSetupContext.DefaultTimeFrame => Feed.Types.Timeframe.M1;

        ISetupSymbolInfo IAlgoSetupContext.DefaultSymbol => _defaultSymbol;

        MappingKey IAlgoSetupContext.DefaultMapping => new MappingKey(MappingCollection.DefaultFullBarToBarReduction);

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

        public virtual void InitializePlugin(RuntimeModel runtime)
        {
            runtime.Config.InitPriorityInvokeStrategy();
            runtime.AccInfoProvider = new PluginTradeInfoProvider(ClientModel.Cache, new DispatcherSync());
            var feedProvider = new PluginFeedProvider(ClientModel.Cache, ClientModel.Distributor, ClientModel.FeedHistory, new DispatcherSync());
            runtime.Metadata = feedProvider;
            runtime.Feed = feedProvider;
            runtime.FeedHistory = feedProvider;
            switch (runtime.Timeframe)
            {
                case Feed.Types.Timeframe.Ticks:
                    runtime.Config.InitQuoteStrategy();
                    break;
                default:
                    runtime.Config.InitBarStrategy(Feed.Types.MarketSide.Bid);
                    break;
            }
            runtime.Config.InitSlidingBuffering(4000);
        }

        public virtual void UpdatePlugin(RuntimeModel runtime)
        {
        }

        bool IExecStateObservable.IsStarted => false;

        public event Action ParamsChanged = delegate { };
        public event Action StartEvent = delegate { };
        public event AsyncEventHandler StopEvent = delegate { return CompletedTask.Default; };
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
