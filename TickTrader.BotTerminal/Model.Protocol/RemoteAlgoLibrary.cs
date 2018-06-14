using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class RemoteAlgoLibrary : IAlgoLibrary
    {
        private Dictionary<PackageKey, PackageInfo> _packages;
        private Dictionary<PluginKey, PluginInfo> _plugins;
        private IAlgoCoreLogger _logger;
        private object _updateLock = new object();


        public event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        public event Action<UpdateInfo<PluginInfo>> PluginUpdated;

        public event Action Reset;


        public RemoteAlgoLibrary(IAlgoCoreLogger logger)
        {
            _logger = logger;

            _packages = new Dictionary<PackageKey, PackageInfo>();
            _plugins = new Dictionary<PluginKey, PluginInfo>();
        }


        public IEnumerable<PackageInfo> GetPackages()
        {
            return _packages.Values.ToList();
        }

        public PackageInfo GetPackage(PackageKey key)
        {
            return _packages.ContainsKey(key) ? _packages[key] : null;
        }

        public IEnumerable<PluginInfo> GetPlugins()
        {
            return _plugins.Values.ToList();
        }

        public IEnumerable<PluginInfo> GetPlugins(AlgoTypes type)
        {
            return _plugins.Values.Where(p => p.Descriptor.Type == type).ToList();
        }

        public PluginInfo GetPlugin(PluginKey key)
        {
            return _plugins.ContainsKey(key) ? _plugins[key] : null;
        }

        public AlgoPackageRef GetPackageRef(PackageKey key)
        {
            throw new NotSupportedException();
        }

        public AlgoPluginRef GetPluginRef(PluginKey key)
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
                case UpdateType.Added:
                    OnAdded(package);
                    break;
                case UpdateType.Replaced:
                    OnUpdated(package);
                    break;
                case UpdateType.Removed:
                    OnRemoved(package);
                    break;
            }
        }

        public void UpdatePackageState(UpdateInfo<PackageInfo> update)
        {
            var package = update.Value;
            lock (_updateLock)
            {
                if (_packages.TryGetValue(package.Key, out var packageModel))
                {
                    packageModel.IsValid = package.IsValid;
                    packageModel.IsObsolete = package.IsObsolete;
                    packageModel.IsLocked = package.IsLocked;
                }
            }
        }


        private void OnAdded(PackageInfo package)
        {
            lock (_updateLock)
            {
                _packages.Add(package.Key, package);
                OnPackageAdded(package);
                MergePlugins(package);
            }
        }

        private void OnUpdated(PackageInfo package)
        {
            lock (_updateLock)
            {
                _packages[package.Key] = package;
                OnPackageReplaced(package);
                MergePlugins(package);
            }
        }

        private void OnRemoved(PackageInfo package)
        {
            lock (_updateLock)
            {
                if (_packages.ContainsKey(package.Key))
                {
                    _packages.Remove(package.Key);
                    OnPackageRemoved(package);
                    MergePlugins(new PackageInfo { Key = package.Key });
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
            foreach (var plugin in _plugins.Values.Where(p => p.Key.IsFromPackage(package.Key)).ToList())
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
                PackageUpdated?.Invoke(new UpdateInfo<PackageInfo>(UpdateType.Added, package));
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
                PackageUpdated?.Invoke(new UpdateInfo<PackageInfo>(UpdateType.Replaced, package));
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
                PackageUpdated?.Invoke(new UpdateInfo<PackageInfo>(UpdateType.Removed, package));
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
                PluginUpdated?.Invoke(new UpdateInfo<PluginInfo>(UpdateType.Added, plugin));
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
                PluginUpdated?.Invoke(new UpdateInfo<PluginInfo>(UpdateType.Replaced, plugin));
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
                PluginUpdated?.Invoke(new UpdateInfo<PluginInfo>(UpdateType.Removed, plugin));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void OnReset()
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

        #endregion Event invokers
    }
}
