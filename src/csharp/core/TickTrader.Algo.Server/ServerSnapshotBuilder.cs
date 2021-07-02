using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    /// <summary>
    /// Not thread-safe. Requires external sync for concurrent use
    /// </summary>
    public class ServerSnapshotBuilder
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<ServerSnapshotBuilder>();

        private readonly Dictionary<string, PackageInfo> _packages = new Dictionary<string, PackageInfo>();
        private readonly Dictionary<string, AccountModelInfo> _accounts = new Dictionary<string, AccountModelInfo>();
        private readonly Dictionary<string, PluginModelInfo> _plugins = new Dictionary<string, PluginModelInfo>();

        private ulong _packagesVersion, _accountsVersion, _pluginsVersion;
        private ulong _packageSnapshotVersion, _accountSnapshotVersion, _pluginSnapshotVersion;
        private PackageListSnapshot _packageSnapshot = new PackageListSnapshot();
        private AccountListSnapshot _accountSnapshot = new AccountListSnapshot();
        private PluginListSnapshot _pluginSnapshot = new PluginListSnapshot();


        #region Snapshots

        public PackageListSnapshot GetPackageSnapshot()
        {
            if (_packageSnapshotVersion != _packagesVersion)
            {
                var newSnapshot = new PackageListSnapshot();
                newSnapshot.Packages.AddRange(_packages.Values.Select(p => p.Clone()));
                _packageSnapshot = newSnapshot;
                _packageSnapshotVersion = _packagesVersion;
            }

            return _packageSnapshot;
        }

        public AccountListSnapshot GetAccountSnapshot()
        {
            if (_accountSnapshotVersion != _accountsVersion)
            {
                var newSnapshot = new AccountListSnapshot();
                newSnapshot.Accounts.AddRange(_accounts.Values.Select(p => p.Clone()));
                _accountSnapshot = newSnapshot;
                _accountSnapshotVersion = _accountsVersion;
            }

            return _accountSnapshot;
        }

        public PluginListSnapshot GetPluginSnapshot()
        {
            if (_pluginSnapshotVersion != _pluginsVersion)
            {
                var newSnapshot = new PluginListSnapshot();
                newSnapshot.Plugins.AddRange(_plugins.Values.Select(p => p.Clone()));
                _pluginSnapshot = newSnapshot;
                _pluginSnapshotVersion = _pluginsVersion;
            }
            return _pluginSnapshot;
        }

        public bool UpdatePackage(PackageUpdate update)
        {
            var id = update.Id;
            var pkg = update.Package;
            switch (update.Action)
            {
                case Update.Types.Action.Added:
                    _packages.Add(id, pkg);
                    break;
                case Update.Types.Action.Updated:
                    _packages[id] = pkg;
                    break;
                case Update.Types.Action.Removed:
                    _packages.Remove(id);
                    break;
                default:
                    _logger.Error($"Unknown update action: {update}");
                    return false;
            }
            _packagesVersion++;
            return true;
        }

        public bool UpdatePackageState(PackageStateUpdate update)
        {
            var id = update.Id;
            if (!_packages.TryGetValue(id, out var pkg))
            {
                _logger.Error($"Id not found. Can't update state: {update}");
                return false;
            }

            pkg.IsLocked = update.IsLocked;

            _packagesVersion++;
            return true;
        }

        public bool UpdateAccount(AccountModelUpdate update)
        {
            var id = update.Id;
            var acc = update.Account;
            switch (update.Action)
            {
                case Update.Types.Action.Added:
                    _accounts.Add(id, acc);
                    break;
                case Update.Types.Action.Updated:
                    _accounts[id] = acc;
                    break;
                case Update.Types.Action.Removed:
                    _accounts.Remove(id);
                    break;
                default:
                    _logger.Error($"Unknown update action: {update}");
                    return false;
            }
            _accountsVersion++;
            return true;
        }

        public bool UpdateAccountState(AccountStateUpdate update)
        {
            var id = update.Id;
            if (!_accounts.TryGetValue(id, out var acc))
            {
                _logger.Error($"Id not found. Can't update state: {update}");
                return false;
            }

            acc.ConnectionState = update.ConnectionState;
            var lastError = update.LastError;
            if (lastError != null)
                acc.LastError = update.LastError;

            _accountsVersion++;
            return true;
        }

        public bool UpdatePlugin(PluginModelUpdate update)
        {
            var id = update.Id;
            var plugin = update.Plugin;
            switch (update.Action)
            {
                case Update.Types.Action.Added:
                    _plugins.Add(id, plugin);
                    break;
                case Update.Types.Action.Updated:
                    _plugins[id] = plugin;
                    break;
                case Update.Types.Action.Removed:
                    _plugins.Remove(id);
                    break;
                default:
                    _logger.Error($"Unknown update action: {update}");
                    return false;
            }
            _pluginsVersion++;
            return true;
        }

        public bool UpdatePluginState(PluginStateUpdate update)
        {
            var id = update.Id;
            if (!_plugins.TryGetValue(id, out var plugin))
            {
                _logger.Error($"Id not found. Can't update state: {update}");
                return false;
            }

            plugin.State = update.State;
            var faultMsg = update.FaultMessage;
            if (faultMsg != null)
                plugin.FaultMessage = faultMsg;

            _pluginsVersion++;
            return true;
        }        

        #endregion Snapshots
    }
}
