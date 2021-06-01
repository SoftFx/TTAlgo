using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;

namespace TickTrader.BotTerminal
{
    internal class RemoteAlgoLibrary : IAlgoLibrary
    {
        private Dictionary<string, PackageInfo> _packages;
        private Dictionary<PluginKey, PluginInfo> _plugins;
        private IAlgoLogger _logger;
        private object _updateLock = new object();


        public event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        public event Action<UpdateInfo<PluginInfo>> PluginUpdated;

        public event Action Reset;

        public event Action<PackageInfo> PackageStateChanged;


        public RemoteAlgoLibrary(IAlgoLogger logger)
        {
            _logger = logger;

            _packages = new Dictionary<string, PackageInfo>();
            _plugins = new Dictionary<PluginKey, PluginInfo>();
        }


        public IEnumerable<PackageInfo> GetPackages()
        {
            return _packages.Values.ToList();
        }

        public PackageInfo GetPackage(string packageId)
        {
            return _packages.ContainsKey(packageId) ? _packages[packageId] : null;
        }

        public IEnumerable<PluginInfo> GetPlugins()
        {
            return _plugins.Values.ToList();
        }

        public IEnumerable<PluginInfo> GetPlugins(Metadata.Types.PluginType type)
        {
            return _plugins.Values.Where(p => p.Descriptor_.Type == type).ToList();
        }

        public PluginInfo GetPlugin(PluginKey key)
        {
            return _plugins.ContainsKey(key) ? _plugins[key] : null;
        }

        public AlgoPackageRef GetPackageRef(string packageId)
        {
            throw new NotSupportedException();
        }

        public void ResetPackages()
        {
            lock (_updateLock)
            {
                OnReset();
                _packages.Clear();
                _plugins.Clear();
            }
        }

        public void SetPackages(List<PackageInfo> packages)
        {
            lock (_updateLock)
            {
                ResetPackages();
                foreach (var package in packages)
                {
                    OnAdded(package);
                }
            }
        }

        public void UpdatePackage(UpdateInfo<PackageInfo> update)
        {
            var package = update.Value;
            switch (update.Type)
            {
                case UpdateInfo.Types.UpdateType.Added:
                    OnAdded(package);
                    break;
                case UpdateInfo.Types.UpdateType.Replaced:
                    OnUpdated(package);
                    break;
                case UpdateInfo.Types.UpdateType.Removed:
                    OnRemoved(package);
                    break;
            }
        }

        public void UpdatePackageState(UpdateInfo<PackageInfo> update)
        {
            lock (_updateLock)
            {
                var package = update.Value;
                if (_packages.TryGetValue(package.PackageId, out var packageModel))
                {
                    packageModel.IsValid = package.IsValid;
                    packageModel.IsLocked = package.IsLocked;
                    OnPackageStateChanged(packageModel);
                }
            }
        }


        private void OnAdded(PackageInfo package)
        {
            lock (_updateLock)
            {
                _packages.Add(package.PackageId, package);
                OnPackageAdded(package);
                MergePlugins(package);
            }
        }

        private void OnUpdated(PackageInfo package)
        {
            lock (_updateLock)
            {
                _packages[package.PackageId] = package;
                OnPackageReplaced(package);
                MergePlugins(package);
            }
        }

        private void OnRemoved(PackageInfo package)
        {
            lock (_updateLock)
            {
                var packageId = package.PackageId;
                if (_packages.ContainsKey(packageId))
                {
                    _packages.Remove(packageId);
                    OnPackageRemoved(package);
                    MergePlugins(new PackageInfo { PackageId = packageId });
                }
            }
        }

        private void MergePlugins(PackageInfo package)
        {
            // upsert
            foreach (var plugin in package.Plugins)
            {
                if (!_plugins.ContainsKey(plugin.Key))
                {
                    _plugins.Add(plugin.Key, plugin);
                    OnPluginAdded(plugin);
                }
                else
                {
                    _plugins[plugin.Key] = plugin;
                    OnPluginReplaced(plugin);
                }
            }

            // remove
            var newPluginsLookup = package.Plugins.ToDictionary(p => p.Key);
            foreach (var plugin in _plugins.Values.Where(p => p.Key.PackageId == package.PackageId).ToList())
            {
                if (!newPluginsLookup.ContainsKey(plugin.Key))
                {
                    _plugins.Remove(plugin.Key);
                    OnPluginRemoved(plugin);
                }
            }
        }


        #region Event invokers

        private void OnPackageAdded(PackageInfo package)
        {
            try
            {
                PackageUpdated?.Invoke(new UpdateInfo<PackageInfo>(UpdateInfo.Types.UpdateType.Added, package));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnPackageReplaced(PackageInfo package)
        {
            try
            {
                PackageUpdated?.Invoke(new UpdateInfo<PackageInfo>(UpdateInfo.Types.UpdateType.Replaced, package));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnPackageRemoved(PackageInfo package)
        {
            try
            {
                PackageUpdated?.Invoke(new UpdateInfo<PackageInfo>(UpdateInfo.Types.UpdateType.Removed, package));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnPluginAdded(PluginInfo plugin)
        {
            try
            {
                PluginUpdated?.Invoke(new UpdateInfo<PluginInfo>(UpdateInfo.Types.UpdateType.Added, plugin));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnPluginReplaced(PluginInfo plugin)
        {
            try
            {
                PluginUpdated?.Invoke(new UpdateInfo<PluginInfo>(UpdateInfo.Types.UpdateType.Replaced, plugin));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnPluginRemoved(PluginInfo plugin)
        {
            try
            {
                PluginUpdated?.Invoke(new UpdateInfo<PluginInfo>(UpdateInfo.Types.UpdateType.Removed, plugin));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnReset()
        {
            try
            {
                Reset?.Invoke();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void OnPackageStateChanged(PackageInfo package)
        {
            try
            {
                PackageStateChanged?.Invoke(package);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        #endregion Event invokers
    }
}
