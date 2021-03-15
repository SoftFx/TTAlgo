using Google.Protobuf.WellKnownTypes;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    internal class RemoteAlgoAgent : IAlgoAgent, IAlgoServerClient
    {
        private const int DefaultChunkSize = 512 * 1024;

        private ISyncContext _syncContext;
        private VarDictionary<PackageKey, PackageInfo> _packages;
        private VarDictionary<PluginKey, PluginInfo> _plugins;
        private VarDictionary<AccountKey, AccountModelInfo> _accounts;
        private VarDictionary<string, RemoteTradeBot> _bots;
        private PluginIdProvider _idProvider;
        private ProtocolClient _protocolClient;
        private ApiMetadataInfo _apiMetadata;
        private MappingCollectionInfo _mappings;
        private SetupContextInfo _setupContext;


        public string Name { get; }

        public bool IsRemote => true;

        public IVarSet<PackageKey, PackageInfo> Packages => _packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _plugins;

        public IVarSet<AccountKey, AccountModelInfo> Accounts => _accounts;

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

            _packages = new VarDictionary<PackageKey, PackageInfo>();
            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            _accounts = new VarDictionary<AccountKey, AccountModelInfo>();
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
            if (_protocolClient.State == ClientStates.Online)
                return _protocolClient.GetBotStatus(botId);
            return Task.FromResult("Disconnected!");
        }

        internal Task<LogRecordInfo[]> GetBotLogs(string botId, Timestamp lastLogTimeUtc, int maxCount = 1000)
        {
            if (_protocolClient.State == ClientStates.Online)
                return _protocolClient.GetBotLogs(botId, lastLogTimeUtc, maxCount);
            return Task.FromResult(new LogRecordInfo[0]);
        }

        internal Task<AlertRecordInfo[]> GetAlerts(Timestamp lastLogTimeUtc, int maxCount = 1000)
        {
            if (_protocolClient.State == ClientStates.Online)
                return _protocolClient.GetAlerts(lastLogTimeUtc, maxCount);
            return Task.FromResult(new AlertRecordInfo[0]);
        }

        #region IAlgoAgent implementation

        public async Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext)
        {
            var accountMetadata = await _protocolClient.GetAccountMetadata(account);
            return new SetupMetadata(_apiMetadata, _mappings, accountMetadata, setupContext ?? _setupContext);
        }

        public Task StartBot(string instanceId)
        {
            return _protocolClient.StartBot(instanceId);
        }

        public Task StopBot(string instanceId)
        {
            return _protocolClient.StopBot(instanceId);
        }

        public Task AddBot(AccountKey account, PluginConfig config)
        {
            return _protocolClient.AddBot(account, config);
        }

        public Task RemoveBot(string instanceId, bool cleanLog = false, bool cleanAlgoData = false)
        {
            return _protocolClient.RemoveBot(instanceId, cleanLog, cleanAlgoData);
        }

        public Task ChangeBotConfig(string instanceId, PluginConfig newConfig)
        {
            return _protocolClient.ChangeBotConfig(instanceId, newConfig);
        }

        public Task AddAccount(AccountKey account, string password)
        {
            return _protocolClient.AddAccount(account, password);
        }

        public Task RemoveAccount(AccountKey account)
        {
            return _protocolClient.RemoveAccount(account);
        }

        public Task ChangeAccount(AccountKey account, string password)
        {
            return _protocolClient.ChangeAccount(account, password);
        }

        public Task<ConnectionErrorInfo> TestAccount(AccountKey account)
        {
            return _protocolClient.TestAccount(account);
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password)
        {
            return _protocolClient.TestAccountCreds(account, password);
        }

        public async Task UploadPackage(string fileName, string srcFilePath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.UploadPackage(new PackageKey(fileName, RepositoryLocation.LocalRepository), srcFilePath, DefaultChunkSize, 0, progressListener));
        }

        public Task RemovePackage(PackageKey package)
        {
            return _protocolClient.RemovePackage(package);
        }

        public async Task DownloadPackage(PackageKey package, string dstFilePath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.DownloadPackage(package, dstFilePath, DefaultChunkSize, 0, progressListener));
        }

        public Task<PluginFolderInfo> GetBotFolderInfo(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            return _protocolClient.GetBotFolderInfo(botId, folderId);
        }

        public Task ClearBotFolder(string botId, PluginFolderInfo.Types.PluginFolderId folderId)
        {
            return _protocolClient.ClearBotFolder(botId, folderId);
        }

        public Task DeleteBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName)
        {
            return _protocolClient.DeleteBotFile(botId, folderId, fileName);
        }

        public async Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.DownloadBotFile(botId, folderId, fileName, dstPath, DefaultChunkSize, 0, progressListener));
        }

        public async Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.UploadBotFile(botId, folderId, fileName, srcPath, DefaultChunkSize, 0, progressListener));
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
                    _packages.Add(package.Key, package);
                    foreach(var plugin in package.Plugins)
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
                    _accounts.Add(acc.Key, acc);
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
                        _packages.Add(package.Key, package);
                        MergePlugins(package);
                        break;
                    case UpdateInfo.Types.UpdateType.Replaced:
                        _packages[package.Key] = package;
                        MergePlugins(package);
                        break;
                    case UpdateInfo.Types.UpdateType.Removed:
                        _packages.Remove(package.Key);
                        MergePlugins(new PackageInfo { Key = package.Key });
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
                        _accounts[acc.Key] = acc;
                        break;
                    case UpdateInfo.Types.UpdateType.Removed:
                        if (_accounts.ContainsKey(acc.Key))
                            _accounts.Remove(acc.Key);
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

        void IAlgoServerClient.UpdatePackageState(PackageInfo package)
        {
            _syncContext.Invoke(() =>
            {
                if (_packages.TryGetValue(package.Key, out var packageModel))
                {
                    packageModel.IsValid = package.IsValid;
                    packageModel.IsLocked = package.IsLocked;
                    PackageStateChanged(packageModel);
                }
            });
        }

        void IAlgoServerClient.UpdateAccountState(AccountModelInfo account)
        {
            _syncContext.Invoke(() =>
            {
                if (_accounts.TryGetValue(account.Key, out var accountModel))
                {
                    accountModel.ConnectionState = account.ConnectionState;
                    accountModel.LastError = account.LastError;
                    AccountStateChanged(accountModel);
                }
            });
        }

        void IAlgoServerClient.UpdateBotState(PluginModelInfo bot)
        {
            _syncContext.Invoke(() =>
            {
                if (_bots.TryGetValue(bot.InstanceId, out var botModel))
                {
                    botModel.UpdateState(bot);
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
            foreach (var plugin in _plugins.Values.Where(p => p.Key.Package.Equals(package.Key)).ToList())
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
