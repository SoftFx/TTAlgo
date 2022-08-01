using Google.Protobuf;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server;
using TickTrader.Algo.Server.PublicAPI.Converters;
using AlgoServerPublicApi = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.BotTerminal
{
    internal class LocalAlgoAgent2 : IAlgoAgent
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly ISyncContext _syncContext;
        private readonly VarDictionary<string, PackageInfo> _packages;
        private readonly VarDictionary<PluginKey, PluginInfo> _plugins;
        private readonly VarDictionary<string, AccountModelInfo> _accounts;
        private readonly VarDictionary<string, LocalTradeBot> _bots;
        private readonly PluginIdProvider _idProvider;

        private LocalAlgoServer _server;
        private ApiMetadataInfo _apiMetadata;
        private MappingCollectionInfo _mappings;
        private SetupContextInfo _setupContext;


        public string Name => "AlgoTerminalServer";

        public bool IsRemote => true;

        public IVarSet<string, PackageInfo> Packages => _packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _plugins;

        public IVarSet<string, AccountModelInfo> Accounts => _accounts;

        public IVarSet<string, ITradeBot> Bots { get; }

        public PluginCatalog Catalog { get; }

        public IPluginIdProvider IdProvider => _idProvider;

        public bool SupportsAccountManagement => true;

        public AlgoServerPublicApi.IAccessManager AccessManager { get; }

        public IAlertModel AlertModel { get; }

        public int RunningBotsCnt => _bots.Snapshot.Values.Count(b => !b.State.IsStopped());

        public bool HasRunningBots => _bots.Snapshot.Values.Any(b => !b.State.IsStopped());

        public MappingCollectionInfo Mappings => _mappings;


        public event Action<PackageInfo> PackageStateChanged = delegate { };

        public event Action<AccountModelInfo> AccountStateChanged = delegate { };

        public event Action<ITradeBot> BotStateChanged = delegate { };

        public event Action<ITradeBot> BotUpdated = delegate { };

        public event Action AccessLevelChanged = delegate { };


        public LocalAlgoAgent2(PersistModel storage)
        {
            _syncContext = new DispatcherSync();

            _packages = new VarDictionary<string, PackageInfo>();
            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            _accounts = new VarDictionary<string, AccountModelInfo>();
            _bots = new VarDictionary<string, LocalTradeBot>();

            Bots = _bots.Select((k, v) => (ITradeBot)v);
            _idProvider = new PluginIdProvider();

            Catalog = new PluginCatalog(this);
            AlertModel = new AlgoAlertModel(Name);
            AccessManager = new AlgoServerPublicApi.ApiAccessManager(AlgoServerPublicApi.ClientClaims.Types.AccessLevel.Admin);

            _ = Init(storage);
        }


        public void ClearCache()
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                _plugins.Clear();
                _accounts.Clear();
                _bots.Clear();
                _idProvider.Reset();
                _apiMetadata = null;
                _mappings = null;
                _setupContext = null;
            });
        }

        public async Task Shutdown()
        {
            try
            {
                await Task.Run(() => _server.Stop());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop AlgoServer");
            }
        }


        private async Task Init(PersistModel storage)
        {
            var settings = LocalAlgoAgent.GetSettings();

            _server = new LocalAlgoServer();
            await _server.Init(settings);
            if (await _server.NeedLegacyState())
            {
                var serverSavedState = LocalAlgoAgent.BuildServerSavedState(storage);
                await _server.LoadLegacyState(serverSavedState);
            }
            await _server.Start();

            _apiMetadata = ApiMetadataInfo.CreateCurrentMetadata();
            _mappings = await _server.GetMappingsInfo(new MappingsInfoRequest());
            _setupContext = new SetupContextInfo(Feed.Types.Timeframe.M1,
                new SymbolConfig("none", SymbolConfig.Types.SymbolOrigin.Online), MappingDefaults.DefaultBarToBarMapping.Key);

            await InitServerListener(_server);
        }

        public Task SubscribeToPluginStatus(string instanceId)
        {
            return Task.CompletedTask;
            //return _server.SubscribeToPluginStatus(new PluginStatusSubscribeRequest { PluginId = instanceId });
        }

        public Task SubscribeToPluginLogs(string instanceId)
        {
            return Task.CompletedTask;
            //return _server.SubscribeToPluginLogs(new PluginLogsSubscribeRequest { PluginId = instanceId });
        }

        public Task UnsubscribeToPluginStatus(string instanceId)
        {
            return Task.CompletedTask;
            //return _server.UnsubscribeToPluginStatus(new PluginStatusUnsubscribeRequest { PluginId = instanceId });
        }

        public Task UnsubscribeToPluginLogs(string instanceId)
        {
            return Task.CompletedTask;
            //return _server.UnsubscribeToPluginLogs(new PluginLogsUnsubscribeRequest { PluginId = instanceId });
        }


        #region IAlgoAgent implementation

        public async Task<SetupMetadata> GetSetupMetadata(string accountId, SetupContextInfo setupContext)
        {
            var accountMetadata = await _server.GetAccountMetadata(new AccountMetadataRequest { AccountId = accountId });
            return new SetupMetadata(_apiMetadata, _mappings, accountMetadata, setupContext ?? _setupContext);
        }

        public Task StartBot(string instanceId)
        {
            return _server.StartPlugin(new StartPluginRequest(instanceId));
        }

        public Task StopBot(string instanceId)
        {
            return _server.StopPlugin(new StopPluginRequest(instanceId));
        }

        public Task AddBot(string accountId, PluginConfig config)
        {
            return _server.AddPlugin(new AddPluginRequest(accountId, config));
        }

        public Task RemoveBot(string instanceId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            return _server.RemovePlugin(new RemovePluginRequest(instanceId, cleanLog, cleanAlgoData));
        }

        public Task ChangeBotConfig(string instanceId, PluginConfig newConfig)
        {
            return _server.UpdatePluginConfig(new ChangePluginConfigRequest(instanceId, newConfig));
        }

        public Task AddAccount(AddAccountRequest request)
        {
            return _server.AddAccount(request);
        }

        public Task RemoveAccount(RemoveAccountRequest request)
        {
            return _server.RemoveAccount(request);
        }

        public Task ChangeAccount(ChangeAccountRequest request)
        {
            return _server.ChangeAccount(request);
        }

        public Task<AlgoServerPublicApi.ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            return _server.TestAccount(request).ContinueWith(t => t.Result.ToApi());
        }

        public Task<AlgoServerPublicApi.ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            return _server.TestCreds(request).ContinueWith(t => t.Result.ToApi());
        }

        public async Task UploadPackage(string fileName, string srcFilePath, AlgoServerPublicApi.IFileProgressListener progressListener)
        {
            progressListener.Init(1);
            await Task.Run(() => _server.UploadPackage(new UploadPackageRequest(null, fileName), srcFilePath));
            progressListener.IncrementProgress(1);
        }

        public Task RemovePackage(string packageId)
        {
            return _server.RemovePackage(new RemovePackageRequest(packageId));
        }

        public Task DownloadPackage(string packageId, string dstFilePath, AlgoServerPublicApi.IFileProgressListener progressListener)
        {
            throw new NotSupportedException();
            //await Task.Run(() => _server.DownloadPackage(new DownloadPackageRequest(packageId), dstFilePath, progressListener));
        }

        public Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            return _server.GetPluginFolderInfo(new PluginFolderInfoRequest(botId, folderId));
        }

        public Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            return _server.ClearPluginFolder(new ClearPluginFolderRequest(botId, folderId));
        }

        public Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
        {
            return _server.DeletePluginFile(new DeletePluginFileRequest(botId, folderId, fileName));
        }

        public Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, AlgoServerPublicApi.IFileProgressListener progressListener)
        {
            throw new NotSupportedException();
            //await Task.Run(() => _server.DownloadPluginFile(new DownloadPluginFileRequest(botId, folderId, fileName), dstPath, progressListener));
        }

        public Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, AlgoServerPublicApi.IFileProgressListener progressListener)
        {
            throw new NotSupportedException();
            //await Task.Run(() => _server.UploadPluginFile(new UploadPluginFileRequest(botId, folderId, fileName), srcPath, progressListener));
        }

        #endregion


        #region LocalAlgoServer listener implementation

        private async Task InitServerListener(LocalAlgoServer server)
        {
            var alertChannel = DefaultChannelFactory.CreateForOneToOne<AlertRecordInfo>();
            await server.SubscribeToAlerts(alertChannel.Writer);
            _ = alertChannel.Consume(ProcessServerAlerts);

            var updateChannel = DefaultChannelFactory.CreateForOneToOne<IMessage>();
            await server.EventBus.SubscribeToUpdates(updateChannel.Writer, true);
            _ = updateChannel.Consume(ProcessServerUpdates);
        }


        private void ProcessServerAlerts(AlertRecordInfo alert)
        {
            _syncContext.Invoke(() =>
            {
                AlertModel.AddAlert(alert);
            });
        }


        private async Task ProcessServerUpdates(IMessage update)
        {
            try
            {
                switch (update)
                {
                    case PackageListSnapshot pkgSnapshot: InitPackageList(pkgSnapshot); break;
                    case AccountListSnapshot accSnapshot: InitAccountList(accSnapshot); break;
                    case PluginListSnapshot pluginSnapshot: await InitPluginList(pluginSnapshot); break;
                    case PackageUpdate pkgUpdate: OnPackageUpdate(pkgUpdate); break;
                    case AccountModelUpdate accUpdate: OnAccountModelUpdate(accUpdate); break;
                    case PluginModelUpdate pluginUpdate: OnPluginModelUpdate(pluginUpdate); break;
                    case PackageStateUpdate pkgStateUpdate: OnPackageStateUpdate(pkgStateUpdate); break;
                    case AccountStateUpdate accStateUpdate: OnAccountStateUpdate(accStateUpdate); break;
                    case PluginStateUpdate pluginStateUpdate: OnPluginStateUpdate(pluginStateUpdate); break;
                    default: _logger.Error($"Failed to proccess update of type '{update.GetType().FullName}'"); break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to proccess update of type '{update.GetType().FullName}'");
            }
        }


        private void InitPackageList(PackageListSnapshot snapshot)
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                _plugins.Clear();
                snapshot.Packages.ForEach(package =>
                {
                    _packages.Add(package.PackageId, package);
                    foreach (var plugin in package.Plugins)
                    {
                        _plugins.Add(plugin.Key, plugin);
                    }
                });
            });
        }

        private void InitAccountList(AccountListSnapshot snapshot)
        {
            _syncContext.Invoke(() =>
            {
                _accounts.Clear();
                foreach (var acc in snapshot.Accounts)
                {
                    _accounts.Add(acc.AccountId, acc);
                }
            });
        }

        private async Task InitPluginList(PluginListSnapshot snapshot)
        {
            var plugins = new List<PluginModelProxy>(snapshot.Plugins.Count);
            foreach (var plugin in snapshot.Plugins)
            {
                plugins.Add(await _server.GetPluginProxy(plugin.InstanceId));
            }

            _syncContext.Invoke(() =>
            {
                _bots.Clear();
                _idProvider.Reset();
                foreach (var bot in plugins)
                {
                    var id = bot.Info.InstanceId;
                    _idProvider.RegisterPluginId(id);
                    _bots.Add(id, new LocalTradeBot(bot, this));
                }
            });
        }

        private void OnPackageUpdate(PackageUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                _packages.TryGetValue(update.Id, out var oldPackage);

                switch (update.Action)
                {
                    case Update.Types.Action.Added:
                        _packages.Add(update.Id, update.Package);
                        MergePlugins(oldPackage, update.Package);
                        break;
                    case Update.Types.Action.Updated:
                        _packages[update.Id] = update.Package;
                        MergePlugins(oldPackage, update.Package);
                        break;
                    case Update.Types.Action.Removed:
                        _packages.Remove(update.Id);
                        MergePlugins(oldPackage, new PackageInfo { PackageId = update.Id });
                        break;
                }
            });
        }

        private void OnAccountModelUpdate(AccountModelUpdate acc)
        {
            _syncContext.Invoke(() =>
            {
                switch (acc.Action)
                {
                    case Update.Types.Action.Added:
                    case Update.Types.Action.Updated:
                        _accounts[acc.Id] = acc.Account;
                        break;
                    case Update.Types.Action.Removed:
                        if (_accounts.ContainsKey(acc.Id))
                            _accounts.Remove(acc.Id);
                        break;
                }
            });
        }

        private async Task OnPluginModelUpdate(PluginModelUpdate update)
        {
            PluginModelProxy proxy = null;
            if (update.Action == Update.Types.Action.Added)
                proxy = await _server.GetPluginProxy(update.Id);

            _syncContext.Invoke(() =>
            {
                switch (update.Action)
                {
                    case Update.Types.Action.Added:
                        _idProvider.RegisterPluginId(update.Id);
                        _bots.Add(update.Id, new LocalTradeBot(proxy, this));
                        break;
                    case Update.Types.Action.Updated:
                        _bots[update.Id].Update(update.Plugin);
                        BotUpdated(_bots[update.Id]);
                        break;
                    case Update.Types.Action.Removed:
                        if (_bots.ContainsKey(update.Id))
                        {
                            _idProvider.UnregisterPlugin(update.Id);
                            _bots.Remove(update.Id);
                        }
                        break;
                }
            });
        }

        private void OnPackageStateUpdate(PackageStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_packages.TryGetValue(update.Id, out var packageModel))
                {
                    packageModel.IsLocked = update.IsLocked;
                    PackageStateChanged(packageModel);
                }
            });
        }

        private void OnAccountStateUpdate(AccountStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_accounts.TryGetValue(update.Id, out var accountModel))
                {
                    accountModel.ConnectionState = update.ConnectionState;
                    accountModel.LastError = update.LastError;
                    AccountStateChanged(accountModel);
                }
            });
        }

        private void OnPluginStateUpdate(PluginStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_bots.TryGetValue(update.Id, out var botModel))
                {
                    botModel.UpdateState(update);
                    BotStateChanged(botModel);
                }
            });
        }

        private void MergePlugins(PackageInfo oldPackage, PackageInfo newPackage)
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

        #endregion
    }
}
