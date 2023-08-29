using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.PublicAPI.Converters;

using AlgoServerApi = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.BotTerminal
{
    internal sealed class RemoteAlgoAgent : IAlgoAgent, AlgoServerApi.IAlgoServerEventHandler
    {
        private readonly Func<string, Task<string>> _get2FAHandler;

        private readonly VarDictionary<string, AccountModelInfo> _accounts;
        private readonly VarDictionary<string, PackageInfo> _packages;
        private readonly VarDictionary<PluginKey, PluginInfo> _plugins;
        private readonly VarDictionary<string, RemoteTradeBot> _bots;

        private readonly PluginIdProvider _idProvider;
        private readonly ISyncContext _syncContext;

        private AlgoServerApi.IAlgoServerClient _protocolClient;
        private ApiMetadataInfo _apiMetadata;
        private MappingCollectionInfo _mappings;
        private SetupContextInfo _setupContext;
        private ServerVersionInfo _currentVersion;
        private UpdateServiceInfo _updateSvcInfo;


        public string Name { get; }

        public bool IsRemote => true;

        public IVarSet<string, PackageInfo> Packages => _packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _plugins;

        public IVarSet<string, AccountModelInfo> Accounts => _accounts;

        public IVarSet<string, ITradeBot> Bots { get; }

        public PluginCatalog Catalog { get; }

        public IPluginIdProvider IdProvider => _idProvider;

        public bool SupportsAccountManagement => true;

        public AlgoServerApi.IAccessManager AccessManager => _protocolClient.AccessManager;

        public AlgoServerApi.IVersionSpec VersionSpec => _protocolClient.VersionSpec;

        public AlertManagerModel AlertModel { get; }

        public ServerVersionInfo CurrentVersion => _currentVersion;

        public UpdateServiceInfo UpdateSvcInfo => _updateSvcInfo;


        public event Action<PackageInfo> PackageStateChanged = delegate { };

        public event Action<AccountModelInfo> AccountStateChanged = delegate { };

        public event Action<ITradeBot> BotStateChanged = delegate { };

        public event Action<ITradeBot> BotUpdated = delegate { };

        public event Action AccessLevelChanged = delegate { };

        public event Action UpdateServiceStateChanged = delegate { };

        public event Action SnapshotLoaded = delegate { };


        public RemoteAlgoAgent(BotAgentStorageEntry creds, Func<string, Task<string>> get2FAHandler)
        {
            Name = creds.Name;
            _get2FAHandler = get2FAHandler;

            _syncContext = new DispatcherSync();

            _packages = new VarDictionary<string, PackageInfo>();
            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            _accounts = new VarDictionary<string, AccountModelInfo>();
            _bots = new VarDictionary<string, RemoteTradeBot>();

            Bots = _bots.Select((k, v) => (ITradeBot)v);
            _idProvider = new PluginIdProvider();

            Catalog = new PluginCatalog(this);
            AlertModel = new AlertManagerModel(creds);
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
                _currentVersion = null;
                _updateSvcInfo = null;
            });
        }


        internal void SetProtocolClient(AlgoServerApi.IAlgoServerClient client)
        {
            _protocolClient = client;
        }

        #region IAlgoAgent implementation

        public async Task<SetupMetadata> GetSetupMetadata(string accountId, SetupContextInfo setupContext)
        {
            var accountMetadata = await _protocolClient.GetAccountMetadata(new AlgoServerApi.AccountMetadataRequest { AccountId = accountId });
            return new SetupMetadata(_apiMetadata, _mappings, accountMetadata.ToServer(), setupContext ?? _setupContext);
        }

        public Task SubscribeToPluginStatus(string instanceId)
        {
            return _protocolClient.SubscribeToPluginStatus(new AlgoServerApi.PluginStatusSubscribeRequest { PluginId = instanceId });
        }

        public Task SubscribeToPluginLogs(string instanceId)
        {
            return _protocolClient.SubscribeToPluginLogs(new AlgoServerApi.PluginLogsSubscribeRequest { PluginId = instanceId });
        }

        public Task UnsubscribeToPluginStatus(string instanceId)
        {
            return _protocolClient.UnsubscribeToPluginStatus(new AlgoServerApi.PluginStatusUnsubscribeRequest { PluginId = instanceId });
        }

        public Task UnsubscribeToPluginLogs(string instanceId)
        {
            return _protocolClient.UnsubscribeToPluginLogs(new AlgoServerApi.PluginLogsUnsubscribeRequest { PluginId = instanceId });
        }

        public Task StartBot(string instanceId)
        {
            return _protocolClient.StartPlugin(new AlgoServerApi.StartPluginRequest(instanceId));
        }

        public Task StopBot(string instanceId)
        {
            return _protocolClient.StopPlugin(new AlgoServerApi.StopPluginRequest(instanceId));
        }

        public Task AddBot(string accountId, PluginConfig config)
        {
            return _protocolClient.AddPlugin(new AlgoServerApi.AddPluginRequest(accountId, config.ToApi()));
        }

        public Task RemoveBot(string instanceId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            return _protocolClient.RemovePlugin(new AlgoServerApi.RemovePluginRequest(instanceId, cleanLog, cleanAlgoData));
        }

        public Task ChangeBotConfig(string instanceId, PluginConfig newConfig)
        {
            return _protocolClient.ChangePluginConfig(new AlgoServerApi.ChangePluginConfigRequest(instanceId, newConfig.ToApi()));
        }

        public Task AddAccount(AddAccountRequest request)
        {
            return _protocolClient.AddAccount(request.ToApi());
        }

        public Task RemoveAccount(RemoveAccountRequest request)
        {
            return _protocolClient.RemoveAccount(request.ToApi());
        }

        public Task ChangeAccount(ChangeAccountRequest request)
        {
            return _protocolClient.ChangeAccount(request.ToApi());
        }

        public Task<AlgoServerApi.ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            return _protocolClient.TestAccount(request.ToApi());
        }

        public Task<AlgoServerApi.ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            return _protocolClient.TestAccountCreds(request.ToApi());
        }

        public async Task UploadPackage(string fileName, string srcFilePath, AlgoServerApi.IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.UploadPackage(new AlgoServerApi.UploadPackageRequest(null, fileName), srcFilePath, progressListener));
        }

        public Task RemovePackage(string packageId)
        {
            return _protocolClient.RemovePackage(new AlgoServerApi.RemovePackageRequest(packageId));
        }

        public async Task DownloadPackage(string packageId, string dstFilePath, AlgoServerApi.IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.DownloadPackage(new AlgoServerApi.DownloadPackageRequest(packageId), dstFilePath, progressListener));
        }

        public Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            return _protocolClient.GetPluginFolderInfo(new AlgoServerApi.PluginFolderInfoRequest(botId, folderId.ToApi())).ContinueWith(u => u.Result.ToServer());
        }

        public Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            return _protocolClient.ClearPluginFolder(new AlgoServerApi.ClearPluginFolderRequest(botId, folderId.ToApi()));
        }

        public Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
        {
            return _protocolClient.DeletePluginFile(new AlgoServerApi.DeletePluginFileRequest(botId, folderId.ToApi(), fileName));
        }

        public async Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, AlgoServerApi.IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.DownloadPluginFile(new AlgoServerApi.DownloadPluginFileRequest(botId, folderId.ToApi(), fileName), dstPath, progressListener));
        }

        public async Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, AlgoServerApi.IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.UploadPluginFile(new AlgoServerApi.UploadPluginFileRequest(botId, folderId.ToApi(), fileName), srcPath, progressListener));
        }

        public async Task<ServerUpdateList> GetServerUpdateList(bool forced)
        {
            return (await _protocolClient.GetServerUpdateList(ServerUpdateListRequest.Get(forced).ToApi())).ToServer();
        }

        public async Task<StartServerUpdateResponse> StartServerUpdate(string releaseId)
        {
            return (await _protocolClient.StartServerUpdate(new AlgoServerApi.StartServerUpdateRequest { ReleaseId = releaseId })).ToServer();
        }

        public async Task<StartServerUpdateResponse> StartServerUpdateFromFile(string version, string srcPath)
        {
            return (await Task.Run(() => _protocolClient.StartCustomUpdate(
                new AlgoServerApi.StartCustomServerUpdateRequest { Version = version, TransferSettings = AlgoServerApi.FileTransferSettings.Default }, srcPath))).ToServer();
        }

        public Task DiscardServerUpdateResult()
        {
            return _protocolClient.DiscardServerUpdateResult();
        }

        #endregion


        #region IAlgoServerClient implementation

        void AlgoServerApi.IAlgoServerEventHandler.AccessLevelChanged()
        {
            _syncContext.Invoke(() =>
            {
                AccessLevelChanged();
            });
        }

        Task<string> AlgoServerApi.IAlgoServerEventHandler.Get2FACode()
        {
            return _get2FAHandler(Name);
        }

        void AlgoServerApi.IAlgoServerEventHandler.InitPackageList(List<AlgoServerApi.PackageInfo> packages)
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                _plugins.Clear();
                packages.ForEach(package =>
                {
                    _packages.Add(package.PackageId, package.ToServer());
                    foreach (var plugin in package.Plugins)
                    {
                        _plugins.Add(plugin.Key.ToServer(), plugin.ToServer());
                    }
                });
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.InitAccountModelList(List<AlgoServerApi.AccountModelInfo> accounts)
        {
            _syncContext.Invoke(() =>
            {
                _accounts.Clear();
                foreach (var acc in accounts)
                {
                    _accounts.Add(acc.AccountId, acc.ToServer());
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.InitPluginModelList(List<AlgoServerApi.PluginModelInfo> bots)
        {
            _syncContext.Invoke(() =>
            {
                _bots.Clear();
                _idProvider.Reset();
                foreach (var bot in bots)
                {
                    _idProvider.RegisterPluginId(bot.InstanceId);
                    _bots.Add(bot.InstanceId, new RemoteTradeBot(bot.ToServer(), this));
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.SetApiMetadata(AlgoServerApi.ApiMetadataInfo apiMetadata)
        {
            _apiMetadata = apiMetadata.ToServer();
        }

        void AlgoServerApi.IAlgoServerEventHandler.SetMappingsInfo(AlgoServerApi.MappingCollectionInfo mappings)
        {
            _mappings = mappings.ToServer();
        }

        void AlgoServerApi.IAlgoServerEventHandler.SetSetupContext(AlgoServerApi.SetupContextInfo setupContext)
        {
            _setupContext = setupContext.ToServer();
        }

        void AlgoServerApi.IAlgoServerEventHandler.InitCurrentVersion(AlgoServerApi.ServerVersionInfo currentVersion)
        {
            _currentVersion = currentVersion?.ToServer();
        }

        void AlgoServerApi.IAlgoServerEventHandler.InitUpdateSvcInfo(AlgoServerApi.UpdateServiceInfo updateSvcInfo)
        {
            _updateSvcInfo = updateSvcInfo?.ToServer();
        }

        void AlgoServerApi.IAlgoServerEventHandler.InitCompleted()
        {
            _syncContext.Invoke(() =>
            {
                SnapshotLoaded();
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPackageUpdate(AlgoServerApi.PackageUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                _packages.TryGetValue(update.Id, out var oldPackage);

                switch (update.Action)
                {
                    case AlgoServerApi.Update.Types.Action.Added:
                        _packages.Add(update.Id, update.Package.ToServer());
                        MergePlugins(oldPackage, update.Package.ToServer());
                        break;
                    case AlgoServerApi.Update.Types.Action.Updated:
                        _packages[update.Id] = update.Package.ToServer();
                        MergePlugins(oldPackage, update.Package.ToServer());
                        break;
                    case AlgoServerApi.Update.Types.Action.Removed:
                        _packages.Remove(update.Id);
                        MergePlugins(oldPackage, new PackageInfo { PackageId = update.Id });
                        break;
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnAccountModelUpdate(AlgoServerApi.AccountModelUpdate acc)
        {
            _syncContext.Invoke(() =>
            {
                switch (acc.Action)
                {
                    case AlgoServerApi.Update.Types.Action.Added:
                    case AlgoServerApi.Update.Types.Action.Updated:
                        _accounts[acc.Id] = acc.Account.ToServer();
                        break;
                    case AlgoServerApi.Update.Types.Action.Removed:
                        if (_accounts.ContainsKey(acc.Id))
                            _accounts.Remove(acc.Id);
                        break;
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPluginModelUpdate(AlgoServerApi.PluginModelUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                switch (update.Action)
                {
                    case AlgoServerApi.Update.Types.Action.Added:
                        _idProvider.RegisterPluginId(update.Id);
                        _bots.Add(update.Id, new RemoteTradeBot(update.Plugin.ToServer(), this));
                        break;
                    case AlgoServerApi.Update.Types.Action.Updated:
                        _bots[update.Id].Update(update.Plugin.ToServer());
                        BotUpdated(_bots[update.Id]);
                        break;
                    case AlgoServerApi.Update.Types.Action.Removed:
                        if (_bots.ContainsKey(update.Id))
                        {
                            _idProvider.UnregisterPluginId(update.Id);
                            _bots.Remove(update.Id);
                        }
                        break;
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPackageStateUpdate(AlgoServerApi.PackageStateUpdate update)
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

        void AlgoServerApi.IAlgoServerEventHandler.OnAccountStateUpdate(AlgoServerApi.AccountStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_accounts.TryGetValue(update.Id, out var accountModel))
                {
                    accountModel.ConnectionState = update.ConnectionState.ToServer();
                    accountModel.LastError = update.LastError.ToServer();
                    AccountStateChanged(accountModel);
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPluginStateUpdate(AlgoServerApi.PluginStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_bots.TryGetValue(update.Id, out var botModel))
                {
                    botModel.UpdateState(update.ToServer());
                    BotStateChanged(botModel);
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPluginStatusUpdate(AlgoServerApi.PluginStatusUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_bots.TryGetValue(update.PluginId, out var botModel))
                {
                    botModel.UpdateStatus(update.Message);
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPluginLogUpdate(AlgoServerApi.PluginLogUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_bots.TryGetValue(update.PluginId, out var botModel))
                {
                    botModel.UpdateLogs(update.Records.Select(u => u.ToServer()).ToList());
                }
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnAlertListUpdate(AlgoServerApi.AlertListUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                AlertModel.UpdateRemoteAlert(update.Alerts.Select(u => u.ToServer()).ToList());
            });
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnUpdateSvcStateUpdate(AlgoServerApi.UpdateServiceStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                _updateSvcInfo = update.Snapshot.ToServer();
                UpdateServiceStateChanged();
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
