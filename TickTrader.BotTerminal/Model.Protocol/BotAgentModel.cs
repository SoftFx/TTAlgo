using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentModel : IBotAgentClient
    {
        private ISyncContext _syncContext;
        private VarDictionary<PackageKey, PackageInfo> _packages;
        private VarDictionary<PluginKey, PluginInfo> _plugins;
        private VarDictionary<AccountKey, AccountModelInfo> _accounts;
        private VarDictionary<string, BotModelInfo> _bots;


        public ApiMetadataInfo ApiMetadata { get; private set; }

        public MappingCollectionInfo Mappings { get; private set; }

        public SetupContextInfo SetupContext { get; private set; }

        public PluginIdProvider IdProvider { get; }


        public IVarSet<PackageKey, PackageInfo> Packages => _packages;

        public IVarSet<PluginKey, PluginInfo> Plugins => _plugins;

        public IVarSet<AccountKey, AccountModelInfo> Accounts => _accounts;

        public IVarSet<string, BotModelInfo> Bots => _bots;


        public Action<PackageInfo> PackageStateChanged = delegate { };

        public Action<AccountModelInfo> AccountStateChanged = delegate { };

        public Action<BotModelInfo> BotStateChanged = delegate { };


        public BotAgentModel()
        {
            _syncContext = new DispatcherSync();

            _packages = new VarDictionary<PackageKey, PackageInfo>();
            _plugins = new VarDictionary<PluginKey, PluginInfo>();
            _accounts = new VarDictionary<AccountKey, AccountModelInfo>();
            _bots = new VarDictionary<string, BotModelInfo>();

            IdProvider = new PluginIdProvider();
        }


        public void ClearCache()
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                _plugins.Clear();
                _accounts.Clear();
                _bots.Clear();
                IdProvider.Reset();
                ApiMetadata = null;
                Mappings = null;
                SetupContext = null;
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


        #region IBotAgentClient implementation

        public void InitPackageList(List<PackageInfo> packages)
        {
            _syncContext.Invoke(() =>
            {
                _packages.Clear();
                packages.ForEach(package =>
                {
                    _packages.Add(package.Key, package);
                    package.Plugins.ForEach(plugin => _plugins.Add(plugin.Key, plugin));
                });
            });
        }

        public void InitAccountList(List<AccountModelInfo> accounts)
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

        public void InitBotList(List<BotModelInfo> bots)
        {
            _syncContext.Invoke(() =>
            {
                _bots.Clear();
                foreach (var bot in bots)
                {
                    _bots.Add(bot.InstanceId, bot);
                    IdProvider.RegisterBot(bot.InstanceId);
                }
            });
        }

        public void UpdatePackage(UpdateInfo<PackageInfo> update)
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

        public void UpdateAccount(UpdateInfo<AccountModelInfo> update)
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

        public void UpdateBot(UpdateInfo<BotModelInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var bot = update.Value;
                switch (update.Type)
                {
                    case UpdateType.Added:
                        _bots.Add(bot.InstanceId, bot);
                        IdProvider.RegisterBot(bot.InstanceId);
                        break;
                    case UpdateType.Replaced:
                        _bots[bot.InstanceId] = bot;
                        break;
                    case UpdateType.Removed:
                        if (_bots.ContainsKey(bot.InstanceId))
                        {
                            _bots.Remove(bot.InstanceId);
                            IdProvider.UnregisterPlugin(bot.InstanceId);
                        }
                        break;
                }
            });
        }

        public void SetApiMetadata(ApiMetadataInfo apiMetadata)
        {
            ApiMetadata = apiMetadata;
        }

        public void SetMappingsInfo(MappingCollectionInfo mappings)
        {
            Mappings = mappings;
        }

        public void SetSetupContext(SetupContextInfo setupContext)
        {
            SetupContext = setupContext;
        }

        public void UpdatePackageState(UpdateInfo<PackageInfo> update)
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

        public void UpdateAccountState(UpdateInfo<AccountModelInfo> update)
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

        public void UpdateBotState(UpdateInfo<BotModelInfo> update)
        {
            _syncContext.Invoke(() =>
            {
                var bot = update.Value;
                if (_bots.TryGetValue(bot.InstanceId, out var botModel))
                {
                    botModel.State = bot.State;
                    botModel.FaultMessage = bot.FaultMessage;
                    BotStateChanged(botModel);
                }
            });
        }

        #endregion
    }
}
