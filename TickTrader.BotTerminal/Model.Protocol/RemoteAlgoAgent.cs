using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Server.Common;
using TickTrader.Algo.ServerControl;
using AlgoServerApi = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.BotTerminal
{
    internal class RemoteAlgoAgent : IAlgoAgent, AlgoServerApi.IAlgoServerEventHandler
    {

        private ISyncContext _syncContext;
        private VarDictionary<string, PackageInfo> _packages;
        private VarDictionary<PluginKey, PluginInfo> _plugins;
        private VarDictionary<string, AccountModelInfo> _accounts;
        private VarDictionary<string, RemoteTradeBot> _bots;
        private PluginIdProvider _idProvider;
        private AlgoServerApi.IAlgoServerClient _protocolClient;
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

        public AccessManager AccessManager => (AccessManager)_protocolClient.AccessManager;

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


        internal void SetProtocolClient(AlgoServerApi.IAlgoServerClient client)
        {
            _protocolClient = client;
        }

        //internal Task<string> GetBotStatus(string botId)
        //{
        //    if (_protocolClient != null && _protocolClient.State == ClientStates.Online)
        //        return _protocolClient.GetPluginStatus(new PluginStatusRequest { PluginId = botId });
        //    return Task.FromResult("Disconnected!");
        //}

        //internal Task<LogRecordInfo[]> GetBotLogs(string botId, Timestamp lastLogTimeUtc, int maxCount = 1000)
        //{
        //    if (_protocolClient != null && _protocolClient.State == ClientStates.Online)
        //        return _protocolClient.GetPluginLogs(new PluginLogsRequest { PluginId = botId, LastLogTimeUtc = lastLogTimeUtc, MaxCount = maxCount });
        //    return Task.FromResult(new LogRecordInfo[0]);
        //}

        //internal Task<AlertRecordInfo[]> GetAlerts(Timestamp lastLogTimeUtc, int maxCount = 1000)
        //{
        //    if (_protocolClient != null && _protocolClient.State == ClientStates.Online)
        //        return _protocolClient.GetAlerts(new PluginAlertsRequest { LastLogTimeUtc = lastLogTimeUtc, MaxCount = maxCount });
        //    return Task.FromResult(new AlertRecordInfo[0]);
        //}

        #region IAlgoAgent implementation

        public async Task<SetupMetadata> GetSetupMetadata(string accountId, SetupContextInfo setupContext)
        {
            var accountMetadata = await _protocolClient.GetAccountMetadata(new AlgoServerApi.AccountMetadataRequest(accountId));
            return new SetupMetadata(_apiMetadata, _mappings, accountMetadata.ToServer(), setupContext ?? _setupContext);
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

        public Task<ConnectionErrorInfo> TestAccount(TestAccountRequest request)
        {
            return _protocolClient.TestAccount(request.ToApi()).ContinueWith(u => u.Result.ToServer());
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(TestAccountCredsRequest request)
        {
            return _protocolClient.TestAccountCreds(request.ToApi()).ContinueWith(u => u.Result.ToServer());
        }

        public async Task UploadPackage(string fileName, string srcFilePath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.UploadPackage(new AlgoServerApi.UploadPackageRequest(null, fileName), srcFilePath, (AlgoServerApi.IFileProgressListener)progressListener));
        }

        public Task RemovePackage(string packageId)
        {
            return _protocolClient.RemovePackage(new AlgoServerApi.RemovePackageRequest(packageId));
        }

        public async Task DownloadPackage(string packageId, string dstFilePath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.DownloadPackage(new AlgoServerApi.DownloadPackageRequest(packageId), dstFilePath, (AlgoServerApi.IFileProgressListener)progressListener));
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

        public async Task DownloadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string dstPath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.DownloadPluginFile(new AlgoServerApi.DownloadPluginFileRequest(botId, folderId.ToApi(), fileName), dstPath, (AlgoServerApi.IFileProgressListener)progressListener));
        }

        public async Task UploadBotFile(string botId, PluginFolderInfo.Types.PluginFolderId folderId, string fileName, string srcPath, IFileProgressListener progressListener)
        {
            await Task.Run(() => _protocolClient.UploadPluginFile(new AlgoServerApi.UploadPluginFileRequest(botId, folderId.ToApi(), fileName), srcPath, (AlgoServerApi.IFileProgressListener)progressListener));
        }

        #endregion


        #region IAlgoServerClient implementation

        //void UpdatePackage(UpdateInfo.Types.UpdateType updateType, PackageInfo package)
        //{
        //    _syncContext.Invoke(() =>
        //    {
        //        _packages.TryGetValue(package.PackageId, out var oldPackage);
        //        switch (updateType)
        //        {
        //            case UpdateInfo.Types.UpdateType.Added:
        //                _packages.Add(package.PackageId, package);
        //                MergePlugins(oldPackage, package);
        //                break;
        //            case UpdateInfo.Types.UpdateType.Replaced:
        //                _packages[package.PackageId] = package;
        //                MergePlugins(oldPackage, package);
        //                break;
        //            case UpdateInfo.Types.UpdateType.Removed:
        //                _packages.Remove(package.PackageId);
        //                MergePlugins(oldPackage, new PackageInfo { PackageId = package.PackageId });
        //                break;
        //        }
        //    });
        //}

        //void UpdateAccount(UpdateInfo.Types.UpdateType updateType, AccountModelInfo acc)
        //{
        //    _syncContext.Invoke(() =>
        //    {
        //        switch (updateType)
        //        {
        //            case UpdateInfo.Types.UpdateType.Added:
        //            case UpdateInfo.Types.UpdateType.Replaced:
        //                _accounts[acc.AccountId] = acc;
        //                break;
        //            case UpdateInfo.Types.UpdateType.Removed:
        //                if (_accounts.ContainsKey(acc.AccountId))
        //                    _accounts.Remove(acc.AccountId);
        //                break;
        //        }
        //    });
        //}

        //void UpdateBot(UpdateInfo.Types.UpdateType updateType, PluginModelInfo bot)
        //{
        //    _syncContext.Invoke(() =>
        //    {
        //        switch (updateType)
        //        {
        //            case UpdateInfo.Types.UpdateType.Added:
        //                _idProvider.RegisterPluginId(bot.InstanceId);
        //                _bots.Add(bot.InstanceId, new RemoteTradeBot(bot, this));
        //                break;
        //            case UpdateInfo.Types.UpdateType.Replaced:
        //                _bots[bot.InstanceId].Update(bot);
        //                BotUpdated(_bots[bot.InstanceId]);
        //                break;
        //            case UpdateInfo.Types.UpdateType.Removed:
        //                if (_bots.ContainsKey(bot.InstanceId))
        //                {
        //                    _idProvider.UnregisterPlugin(bot.InstanceId);
        //                    _bots.Remove(bot.InstanceId);
        //                }
        //                break;
        //        }
        //    });
        //}

        void AlgoServerApi.IAlgoServerEventHandler.AccessLevelChanged()
        {
            _syncContext.Invoke(() =>
            {
                AccessLevelChanged();
            });
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

        void AlgoServerApi.IAlgoServerEventHandler.InitAccountList(List<AlgoServerApi.AccountModelInfo> accounts)
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

        void AlgoServerApi.IAlgoServerEventHandler.InitBotList(List<AlgoServerApi.PluginModelInfo> bots)
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

        void AlgoServerApi.IAlgoServerEventHandler.OnPackageUpdate(AlgoServerApi.PackageUpdate update)
        {
            throw new NotImplementedException();
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnAccountUpdate(AlgoServerApi.AccountModelUpdate update)
        {
            throw new NotImplementedException();
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPluginModelUpdate(AlgoServerApi.PluginModelUpdate update)
        {
            throw new NotImplementedException();
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPackageStateUpdate(AlgoServerApi.PackageStateUpdate packageState)
        {
            throw new NotImplementedException();
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnAccountStateUpdate(AlgoServerApi.AccountStateUpdate accountState)
        {
            throw new NotImplementedException();
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPluginStateUpdate(AlgoServerApi.PluginStateUpdate pluginState)
        {
            throw new NotImplementedException();
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPluginStatusUpdate(AlgoServerApi.PluginStatusUpdate update)
        {
            throw new NotImplementedException();
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnPluginLogUpdate(AlgoServerApi.PluginLogUpdate update)
        {
            throw new NotImplementedException();
        }

        void AlgoServerApi.IAlgoServerEventHandler.OnAlertListUpdate(AlgoServerApi.AlertListUpdate update)
        {
            throw new NotImplementedException();
        }


        //void UpdatePackageState(PackageStateUpdate update)
        //{
        //    _syncContext.Invoke(() =>
        //    {
        //        if (_packages.TryGetValue(update.Id, out var packageModel))
        //        {
        //            packageModel.IsLocked = update.IsLocked;
        //            PackageStateChanged(packageModel);
        //        }
        //    });
        //}

        //void UpdateAccountState(AccountStateUpdate update)
        //{
        //    _syncContext.Invoke(() =>
        //    {
        //        if (_accounts.TryGetValue(update.Id, out var accountModel))
        //        {
        //            accountModel.ConnectionState = update.ConnectionState;
        //            accountModel.LastError = update.LastError;
        //            AccountStateChanged(accountModel);
        //        }
        //    });
        //}

        //void UpdateBotState(PluginStateUpdate update)
        //{
        //    _syncContext.Invoke(() =>
        //    {
        //        if (_bots.TryGetValue(update.Id, out var botModel))
        //        {
        //            botModel.UpdateState(update);
        //            BotStateChanged(botModel);
        //        }
        //    });
        //}


        //private void MergePlugins(PackageInfo oldPackage, PackageInfo newPackage)
        //{
        //    var newPlugins = newPackage?.Plugins ?? Enumerable.Empty<PluginInfo>();
        //    // upsert
        //    foreach (var plugin in newPlugins)
        //    {
        //        if (!_plugins.ContainsKey(plugin.Key))
        //        {
        //            _plugins.Add(plugin.Key, plugin);
        //        }
        //        else
        //        {
        //            _plugins[plugin.Key] = plugin;
        //        }
        //    }

        //    if (oldPackage != null)
        //    {
        //        // remove
        //        var newPluginsLookup = newPlugins.ToDictionary(p => p.Key);
        //        foreach (var plugin in oldPackage.Plugins)
        //        {
        //            if (!newPluginsLookup.ContainsKey(plugin.Key))
        //            {
        //                _plugins.Remove(plugin.Key);
        //            }
        //        }
        //    }
        //}

        #endregion
    }
}
