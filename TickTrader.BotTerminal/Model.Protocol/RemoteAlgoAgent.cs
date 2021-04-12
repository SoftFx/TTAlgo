using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    internal class RemoteAlgoAgent : IAlgoAgent, IAlgoServerClient
    {

        private ISyncContext _syncContext;
        private VarDictionary<string, PackageInfo> _packages;
        private VarDictionary<PluginKey, PluginInfo> _plugins;
        private VarDictionary<string, AccountModelInfo> _accounts;
        private VarDictionary<string, RemoteTradeBot> _bots;
        private PluginIdProvider _idProvider;
        private ProtocolClient _protocolClient;
        private ApiMetadataInfo _apiMetadata;
        private MappingCollectionInfo _mappings;
        private SetupContextInfo _setupContext;


        public string Name { get; }

        public bool IsRemote => true;

        public IVarSet<string, PackageInfo> Packages => _packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _plugins;

        public IVarSet<string, AccountModelInfo> Accounts => _accounts;

        public IVarSet<string, ITradeBot> Bots { get; }

        public PluginCatalog Catalog { get; }

        public IPluginIdProvider IdProvider => _idProvider;

        public bool SupportsAccountManagement => true;

        public AccessManager AccessManager => _protocolClient.AccessManager;

        public IAlertModel AlertModel { get; }

        public event Action<PackageInfo> PackageStateChanged = delegate { };

        public event Action<AccountModelInfo> AccountStateChanged = delegate { };

        public event Action<ITradeBot> BotStateChanged = delegate { };

        public event Action<ITradeBot> BotUpdated = delegate { };

        public event Action AccessLevelChanged = delegate { };


        public RemoteAlgoAgent(string name)
        {
            Name = name;

            _syncContext = new DispatcherSync();

            _packages = new VarDictionary<string, PackageInfo>();
            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            _accounts = new VarDictionary<string, AccountModelInfo>();
            _bots = new VarDictionary<string, RemoteTradeBot>();

            Bots = _bots.Select((k, v) => (ITradeBot)v);
            _idProvider = new PluginIdProvider();

            Catalog = new PluginCatalog(this);
            AlertModel = new AlgoAlertModel(name, this);
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


        internal void SetProtocolClient(ProtocolClient client)
        {
            _protocolClient = client;
        }

        internal Task<string> GetBotStatus(string botId)
        {
            if (_protocolClient != null && _protocolClient.State == ClientStates.Online)
                return _protocolClient.GetPluginStatus(new PluginStatusRequest { PluginId = botId });
            return Task.FromResult("Disconnected!");
        }

        internal Task<LogRecordInfo[]> GetBotLogs(string botId, Timestamp lastLogTimeUtc, int maxCount = 1000)
        {
            if (_protocolClient != null && _protocolClient.State == ClientStates.Online)
                return _protocolClient.GetPluginLogs(new PluginLogsRequest { PluginId = botId, LastLogTimeUtc = lastLogTimeUtc, MaxCount = maxCount });
            return Task.FromResult(new LogRecordInfo[0]);
        }

        internal Task<AlertRecordInfo[]> GetAlerts(Timestamp lastLogTimeUtc, int maxCount = 1000)
        {
            if (_protocolClient != null && _protocolClient.State == ClientStates.Online)
                return _protocolClient.GetAlerts(new PluginAlertsRequest { LastLogTimeUtc = lastLogTimeUtc, MaxCount = maxCount });
            return Task.FromResult(new AlertRecordInfo[0]);
        }

        #region IAlgoAgent implementation

        public async Task<SetupMetadata> GetSetupMetadata(string accountId, SetupContextInfo setupContext)
        {
            var accountMetadata = await _protocolClient.GetAccountMetadata(new AccountMetadataRequest(accountId));
            return new SetupMetadata(_apiMetadata, _mappings, accountMetadata, setupContext ?? _setupContext);
        }

        public Task StartBot(string instanceId)
        {
            return _protocolClient.StartPlugin(new StartPluginRequest(instanceId));
        }

        public Task StopBot(string instanceId)
        {
            return _protocolClient.StopPlugin(new StopPluginRequest(instanceId));
        }

        public Task AddBot(string accountId, PluginConfig config)
        {
            return _protocolClient.AddPlugin(new AddPluginRequest(accountId, config));
        }

        public Task RemoveBot(string instanceId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            return _protocolClient.RemovePlugin(new RemovePluginRequest(instanceId, cleanLog, cleanAlgoData));
        }

        public Task ChangeBotConfig(string instanceId, PluginConfig newConfig)
        {
            return _protocolClient.ChangePluginConfig(new ChangePluginConfigRequest(instanceId, newConfig));
        }

        public Task AddAccount(AddAccountRequest request)
        {
            return _protocolClient.AddAccount(request);
        }

        public Task RemoveAccount(RemoveAccountRequest request)
        {
            return _protocolClient.RemoveAccount(request);
        }

        public Task ChangeAccount(ChangeAccountRequest request)
        {
            return _protocolClient.ChangeAccount(request);
        }

        public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            return _protocolClient.TestAccount(request);
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            return _protocolClient.TestAccountCreds(request);
        }

        public async Task UploadPackage(string fileName, string srcFilePath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.UploadPackage(new UploadPackageRequest(PackageId.Pack(SharedConstants.LocalRepositoryId, fileName), null), srcFilePath, progressListener));
        }

        public Task RemovePackage(string packageId)
        {
            return _protocolClient.RemovePackage(new RemovePackageRequest(packageId));
        }

        public async Task DownloadPackage(string packageId, string dstFilePath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.DownloadPackage(new DownloadPackageRequest(packageId), dstFilePath, progressListener));
        }

        public Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            return _protocolClient.GetPluginFolderInfo(new PluginFolderInfoRequest(botId, folderId));
        }

        public Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            return _protocolClient.ClearPluginFolder(new ClearPluginFolderRequest(botId, folderId));
        }

        public Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
        {
            return _protocolClient.DeletePluginFile(new DeletePluginFileRequest(botId, folderId, fileName));
        }

        public async Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.DownloadPluginFile(new DownloadPluginFileRequest(botId, folderId, fileName), dstPath, progressListener));
        }

        public async Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.UploadPluginFile(new UploadPluginFileRequest(botId, folderId, fileName), srcPath, progressListener));
        }

        #endregion


        #region IAlgoServerClient implementation

        void IAlgoServerClient.AccessLevelChanged()
        {
            _syncContext.Invoke(() =>
            {
                AccessLevelChanged();
            });
        }

        void IAlgoServerClient.InitPackageList(List<PackageInfo> packages)
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                _plugins.Clear();
                packages.ForEach(package =>
                {
                    _packages.Add(package.PackageId, package);
                    foreach (var plugin in package.Plugins)
                    {
                        _plugins.Add(plugin.Key, plugin);
                    }
                });
            });
        }

        void IAlgoServerClient.InitAccountList(List<AccountModelInfo> accounts)
        {
            _syncContext.Invoke(() =>
            {
                _accounts.Clear();
                foreach (var acc in accounts)
                {
                    _accounts.Add(acc.AccountId, acc);
                }
            });
        }

        void IAlgoServerClient.InitBotList(List<PluginModelInfo> bots)
        {
            _syncContext.Invoke(() =>
            {
                _bots.Clear();
                _idProvider.Reset();
                foreach (var bot in bots)
                {
                    _idProvider.RegisterPluginId(bot.InstanceId);
                    _bots.Add(bot.InstanceId, new RemoteTradeBot(bot, this));
                }
            });
        }

        void IAlgoServerClient.UpdatePackage(UpdateInfo.Types.UpdateType updateType, PackageInfo package)
        {
            _syncContext.Invoke(() =>
            {
                switch (updateType)
                {
                    case UpdateInfo.Types.UpdateType.Added:
                        _packages.Add(package.PackageId, package);
                        MergePlugins(package);
                        break;
                    case UpdateInfo.Types.UpdateType.Replaced:
                        _packages[package.PackageId] = package;
                        MergePlugins(package);
                        break;
                    case UpdateInfo.Types.UpdateType.Removed:
                        _packages.Remove(package.PackageId);
                        MergePlugins(new PackageInfo { PackageId = package.PackageId });
                        break;
                }
            });
        }

        void IAlgoServerClient.UpdateAccount(UpdateInfo.Types.UpdateType updateType, AccountModelInfo acc)
        {
            _syncContext.Invoke(() =>
            {
                switch (updateType)
                {
                    case UpdateInfo.Types.UpdateType.Added:
                    case UpdateInfo.Types.UpdateType.Replaced:
                        _accounts[acc.AccountId] = acc;
                        break;
                    case UpdateInfo.Types.UpdateType.Removed:
                        if (_accounts.ContainsKey(acc.AccountId))
                            _accounts.Remove(acc.AccountId);
                        break;
                }
            });
        }

        void IAlgoServerClient.UpdateBot(UpdateInfo.Types.UpdateType updateType, PluginModelInfo bot)
        {
            _syncContext.Invoke(() =>
            {
                switch (updateType)
                {
                    case UpdateInfo.Types.UpdateType.Added:
                        _idProvider.RegisterPluginId(bot.InstanceId);
                        _bots.Add(bot.InstanceId, new RemoteTradeBot(bot, this));
                        break;
                    case UpdateInfo.Types.UpdateType.Replaced:
                        _bots[bot.InstanceId].Update(bot);
                        BotUpdated(_bots[bot.InstanceId]);
                        break;
                    case UpdateInfo.Types.UpdateType.Removed:
                        if (_bots.ContainsKey(bot.InstanceId))
                        {
                            _idProvider.UnregisterPlugin(bot.InstanceId);
                            _bots.Remove(bot.InstanceId);
                        }
                        break;
                }
            });
        }

        void IAlgoServerClient.SetApiMetadata(ApiMetadataInfo apiMetadata)
        {
            _apiMetadata = apiMetadata;
        }

        void IAlgoServerClient.SetMappingsInfo(MappingCollectionInfo mappings)
        {
            _mappings = mappings;
        }

        void IAlgoServerClient.SetSetupContext(SetupContextInfo setupContext)
        {
            _setupContext = setupContext;
        }

        void IAlgoServerClient.UpdatePackageState(PackageStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_packages.TryGetValue(update.PackageId, out var packageModel))
                {
                    packageModel.IsValid = update.IsValid;
                    packageModel.IsLocked = update.IsLocked;
                    PackageStateChanged(packageModel);
                }
            });
        }

        void IAlgoServerClient.UpdateAccountState(AccountStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_accounts.TryGetValue(update.AccountId, out var accountModel))
                {
                    accountModel.ConnectionState = update.ConnectionState;
                    accountModel.LastError = update.LastError;
                    AccountStateChanged(accountModel);
                }
            });
        }

        void IAlgoServerClient.UpdateBotState(PluginStateUpdate update)
        {
            _syncContext.Invoke(() =>
            {
                if (_bots.TryGetValue(update.PluginId, out var botModel))
                {
                    botModel.UpdateState(update);
                    BotStateChanged(botModel);
                }
            });
        }


        private void MergePlugins(PackageInfo package)
        {
            // upsert
            foreach (var plugin in package.Plugins)
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

            // remove
            var newPluginsLookup = package.Plugins.ToDictionary(p => p.Key);
            foreach (var plugin in _plugins.Values.Where(p => p.Key.PackageId == package.PackageId).ToList())
            {
                if (!newPluginsLookup.ContainsKey(plugin.Key))
                {
                    _plugins.Remove(plugin.Key);
                }
            }
        }

        #endregion
    }
}
