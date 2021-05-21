using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server;

namespace TickTrader.Algo.Common.Model
{
    public class LocalAlgoLibrary : IAlgoLibrary
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<LocalAlgoLibrary>();

        private Dictionary<string, PackageInfo> _packages = new Dictionary<string, PackageInfo>();
        private Dictionary<PluginKey, PluginInfo> _plugins = new Dictionary<PluginKey, PluginInfo>();
        private object _updateLock = new object();
        private AlgoServer _server;


        public event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        public event Action<UpdateInfo<PluginInfo>> PluginUpdated;

        public event Action Reset;

        public event Action<PackageInfo> PackageStateChanged;


        public LocalAlgoLibrary(AlgoServer server)
        {
            _server = server;

            _server.PackageStorage.PackageUpdated += OnPackageUpdated;
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
            return _server.PackageStorage.GetPackageRef(packageId).Result;
        }

        public void RegisterRepositoryLocation(string locationId, string repoPath, bool isolation)
        {
            _server.PackageStorage.RegisterRepositoryLocation(locationId, repoPath);
        }

        public void AddAssemblyAsPackage(Assembly assembly)
        {
            _server.PackageStorage.RegisterAssemblyAsPackage(assembly);
        }

        public Task WaitInit()
        {
            return Task.CompletedTask;
        }


        private void OnPackageUpdated(PackageUpdate update)
        {
            switch (update.Action)
            {
                case Domain.Package.Types.UpdateAction.Upsert:
                    if (_packages.ContainsKey(update.Package.PackageId))
                    {
                        RepositoryOnUpdated(update.Package);
                    }
                    else
                    {
                        RepositoryOnAdded(update.Package);
                    }
                    break;
                case Domain.Package.Types.UpdateAction.Removed:
                    if (_packages.TryGetValue(update.Id, out var packageOldRef))
                        RepositoryOnRemoved(packageOldRef);
                    break;
                default:
                    break;
            }
        }

        private void RepositoryOnAdded(PackageInfo pkgInfo)
        {
            lock (_updateLock)
            {
                _packages.Add(pkgInfo.PackageId, pkgInfo);
                OnPackageAdded(pkgInfo);

                MergePlugins(pkgInfo);
            }
        }

        private void RepositoryOnUpdated(PackageInfo pkgInfo)
        {
            lock (_updateLock)
            {
                _packages[pkgInfo.PackageId] = pkgInfo;
                OnPackageReplaced(pkgInfo);

                MergePlugins(pkgInfo);
            }
        }

        private void RepositoryOnRemoved(PackageInfo pkgInfo)
        {
            lock (_updateLock)
            {
                var pkgId = pkgInfo.PackageId;
                if (_packages.TryGetValue(pkgId, out var pkg))
                {
                    _packages.Remove(pkgId);
                    OnPackageRemoved(pkg);

                    MergePlugins(new PackageInfo { PackageId = pkgId });
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

        private void InitPackageRef(AlgoPackageRef packageRef)
        {
            packageRef.IsLockedChanged += PackageRefOnLockedChanged;
        }

        private void DeinitPackageRef(AlgoPackageRef packageRef)
        {
            packageRef.IsLockedChanged -= PackageRefOnLockedChanged;
        }

        private void PackageRefOnLockedChanged(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                if (_packages.TryGetValue(packageRef.PackageInfo.PackageId, out var package))
                {
                    package.IsLocked = packageRef.IsLocked;
                    OnPackageStateChanged(package);
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
