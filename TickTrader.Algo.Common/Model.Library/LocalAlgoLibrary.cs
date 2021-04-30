using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.ServerControl;

namespace TickTrader.Algo.Common.Model
{
    public class LocalAlgoLibrary : IAlgoLibrary
    {
        private Dictionary<string, AlgoPackageRef> _packageRefs;
        private Dictionary<string, PackageInfo> _packages;
        private Dictionary<PluginKey, PluginInfo> _plugins;
        private IAlgoLogger _logger;
        private object _updateLock = new object();
        private AlgoServer _server;


        public event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        public event Action<UpdateInfo<PluginInfo>> PluginUpdated;

        public event Action Reset;

        public event Action<PackageInfo> PackageStateChanged;


        public LocalAlgoLibrary(IAlgoLogger logger, AlgoServer server)
        {
            _logger = logger;
            _server = server;

            _server.PackageUpdated += OnPackageUpdated;
            _packageRefs = new Dictionary<string, AlgoPackageRef>();
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
            return _packageRefs.ContainsKey(packageId) ? _packageRefs[packageId] : null;
        }

        public void RegisterRepositoryLocation(string locationId, string repoPath, bool isolation)
        {
            _server.RegisterPackageRepository(locationId, repoPath);
        }

        public void AddAssemblyAsPackage(Assembly assembly)
        {
            _server.AddAssemblyAsPackage(assembly.Location);
            //var location = assembly.Location;

            //var packageRef = new AlgoPackageRef(Path.GetFileName(assembly.Location).ToLowerInvariant(), RepositoryLocation.Embedded,
            //    PackageIdentity.Create(new FileInfo(assembly.Location)), AlgoAssemblyInspector.FindPlugins(assembly).Select(m => new AlgoPluginRef(m, assembly.Location)));

            //RepositoryOnAdded(packageRef);
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
                    if (_server.TryGetPackage(update.Id, out var packageRef)) // ???
                    {
                        if (_packageRefs.ContainsKey(update.Package.PackageId))
                        {
                            RepositoryOnUpdated(packageRef);
                        }
                        else
                        {
                            RepositoryOnAdded(packageRef);
                        }
                    }
                    break;
                case Domain.Package.Types.UpdateAction.Removed:
                    if (_packageRefs.TryGetValue(update.Id, out var packageOldRef))
                        RepositoryOnRemoved(packageOldRef);
                    break;
                default:
                    break;
            }
        }

        private void RepositoryOnAdded(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var package = packageRef.PackageInfo;

                _packageRefs.Add(package.PackageId, packageRef);
                InitPackageRef(packageRef);

                _packages.Add(package.PackageId, package);
                OnPackageAdded(package);

                MergePlugins(package);
            }
        }

        private void RepositoryOnUpdated(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var package = packageRef.PackageInfo;

                DeinitPackageRef(_packageRefs[package.PackageId]);
                _packageRefs[package.PackageId] = packageRef;
                InitPackageRef(packageRef);

                _packages[package.PackageId] = package;
                OnPackageReplaced(package);

                MergePlugins(package);
            }
        }

        private void RepositoryOnRemoved(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var packageId = packageRef.PackageInfo.PackageId;
                if (_packages.TryGetValue(packageId, out var package))
                {
                    DeinitPackageRef(_packageRefs[package.PackageId]);
                    _packageRefs.Remove(packageId);

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
