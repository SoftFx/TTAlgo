using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class RemoteAlgoAgent : IAlgoAgent, IBotAgentClient
    {
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

        public IVarSet<PackageKey, PackageInfo> Packages => _packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _plugins;

        public IVarSet<AccountKey, AccountModelInfo> Accounts => _accounts;

        public IVarSet<string, ITradeBot> Bots { get; }

        public PluginCatalog Catalog { get; }

        public IPluginIdProvider IdProvider => _idProvider;

        public bool SupportsAccountManagement => true;


        public event Action<PackageInfo> PackageStateChanged = delegate { };

        public event Action<AccountModelInfo> AccountStateChanged = delegate { };

        public event Action<ITradeBot> BotStateChanged = delegate { };


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

        internal Task<LogRecordInfo[]> GetBotLogs(string botId, DateTime lastLogTimeUtc, int maxCount = 1000)
        {
            if (_protocolClient.State == ClientStates.Online)
                return _protocolClient.GetBotLogs(botId, lastLogTimeUtc, maxCount);
            return Task.FromResult(new LogRecordInfo[0]);
        }


        #region IAlgoAgent implementation

        public async Task<SetupMetadata> GetSetupMetadata(AccountKey account, SetupContextInfo setupContext)
        {
            var accountMetadata = await _protocolClient.GetAccountMetadata(account);
            return new SetupMetadata(_apiMetadata, _mappings, accountMetadata, _setupContext);
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

        public Task AddAccount(AccountKey account, string password, bool useNewProtocol)
        {
            return _protocolClient.AddAccount(account, password, useNewProtocol);
        }

        public Task RemoveAccount(AccountKey account)
        {
            return _protocolClient.RemoveAccount(account);
        }

        public Task ChangeAccount(AccountKey account, string password, bool useNewProtocol)
        {
            return _protocolClient.ChangeAccount(account, password, useNewProtocol);
        }

        public Task<ConnectionErrorInfo> TestAccount(AccountKey account)
        {
            return _protocolClient.TestAccount(account);
        }

        public Task<ConnectionErrorInfo> TestAccountCreds(AccountKey account, string password, bool useNewProtocol)
        {
            return _protocolClient.TestAccountCreds(account, password, useNewProtocol);
        }

        public Task UploadPackage(string fileName, string srcFilePath)
        {
            var bytes = File.ReadAllBytes(srcFilePath);
            return _protocolClient.UploadPackage(fileName, bytes);
        }

        public Task RemovePackage(PackageKey package)
        {
            return _protocolClient.RemovePackage(package);
        }

        public async Task DownloadPackage(PackageKey package, string dstFilePath)
        {
            var bytes = await _protocolClient.DownloadPackage(package);
            File.WriteAllBytes(dstFilePath, bytes);
        }

        #endregion


        #region IBotAgentClient implementation

        void IBotAgentClient.InitPackageList(List<PackageInfo> packages)
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                _plugins.Clear();
                packages.ForEach(package =>
                {
                    _packages.Add(package.Key, package);
                    package.Plugins.ForEach(plugin => _plugins.Add(plugin.Key, plugin));
                });
            });
        }

        void IBotAgentClient.InitAccountList(List<AccountModelInfo> accounts)
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

        void IBotAgentClient.InitBotList(List<BotModelInfo> bots)
        {
            _syncContext.Invoke(() =>
            {
                _bots.Clear();
                _idProvider.Reset();
                foreach (var bot in bots)
                {
                    _bots.Add(bot.InstanceId, new RemoteTradeBot(bot, this));
                    _idProvider.RegisterBot(bot.InstanceId);
                }
            });
        }

        void IBotAgentClient.UpdatePackage(UpdateInfo<PackageInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var package = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                        _packages.Add(package.Key, package);
                        MergePlugins(package);
                        break;
                    case UpdateType.Replaced:
                        _packages[package.Key] = package;
                        MergePlugins(package);
                        break;
                    case UpdateType.Removed:
                        _packages.Remove(package.Key);
                        MergePlugins(new PackageInfo { Key = package.Key });
                        break;
                }
            });
        }

        void IBotAgentClient.UpdateAccount(UpdateInfo<AccountModelInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var acc = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                    case UpdateType.Replaced:
                        _accounts[acc.Key] = acc;
                        break;
                    case UpdateType.Removed:
                        if (_accounts.ContainsKey(acc.Key))
                            _accounts.Remove(acc.Key);
                        break;
                }
            });
        }

        void IBotAgentClient.UpdateBot(UpdateInfo<BotModelInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var bot = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                        _bots.Add(bot.InstanceId, new RemoteTradeBot(bot, this));
                        _idProvider.RegisterBot(bot.InstanceId);
                        break;
                    case UpdateType.Replaced:
                        _bots[bot.InstanceId].Update(bot);
                        break;
                    case UpdateType.Removed:
                        if (_bots.ContainsKey(bot.InstanceId))
                        {
                            _bots.Remove(bot.InstanceId);
                            _idProvider.UnregisterPlugin(bot.InstanceId);
                        }
                        break;
                }
            });
        }

        void IBotAgentClient.SetApiMetadata(ApiMetadataInfo apiMetadata)
        {
            _apiMetadata = apiMetadata;
        }

        void IBotAgentClient.SetMappingsInfo(MappingCollectionInfo mappings)
        {
            _mappings = mappings;
        }

        void IBotAgentClient.SetSetupContext(SetupContextInfo setupContext)
        {
            _setupContext = setupContext;
        }

        void IBotAgentClient.UpdatePackageState(UpdateInfo<PackageInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var package = update.Value;
                if (_packages.TryGetValue(package.Key, out var packageModel))
                {
                    packageModel.IsValid = package.IsValid;
                    packageModel.IsObsolete = package.IsObsolete;
                    packageModel.IsLocked = package.IsLocked;
                    PackageStateChanged(packageModel);
                }
            });
        }

        void IBotAgentClient.UpdateAccountState(UpdateInfo<AccountModelInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var account = update.Value;
                if (_accounts.TryGetValue(account.Key, out var accountModel))
                {
                    accountModel.ConnectionState = account.ConnectionState;
                    accountModel.LastError = account.LastError;
                    AccountStateChanged(accountModel);
                }
            });
        }

        void IBotAgentClient.UpdateBotState(UpdateInfo<BotModelInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var bot = update.Value;
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
            foreach (var plugin in _plugins.Values.Where(p => p.Key.IsFromPackage(package.Key)).ToList())
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
