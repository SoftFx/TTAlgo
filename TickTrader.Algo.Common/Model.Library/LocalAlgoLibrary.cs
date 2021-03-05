using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Common.Model
{
    public class LocalAlgoLibrary : IAlgoLibrary
    {
        private Dictionary<RepositoryLocation, PackageRepository> _repositories;
        private Dictionary<PackageKey, AlgoPackageRef> _packageRefs;
        private Dictionary<PackageKey, PackageInfo> _packages;
        private Dictionary<PluginKey, PluginInfo> _plugins;
        private IAlgoCoreLogger _logger;
        private object _updateLock = new object();
        private AlgoServer _server;


        public event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        public event Action<UpdateInfo<PluginInfo>> PluginUpdated;

        public event Action Reset;

        public event Action<PackageInfo> PackageStateChanged;


        public LocalAlgoLibrary(IAlgoCoreLogger logger, AlgoServer server)
        {
            _logger = logger;
            _server = server;

            _server.PackageUpdated += OnPackageUpdated;

            _repositories = new Dictionary<RepositoryLocation, PackageRepository>();
            _packageRefs = new Dictionary<PackageKey, AlgoPackageRef>();
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

        public IEnumerable<PluginInfo> GetPlugins(Metadata.Types.PluginType type)
        {
            return _plugins.Values.Where(p => p.Descriptor_.Type == type).ToList();
        }

        public PluginInfo GetPlugin(PluginKey key)
        {
            return _plugins.ContainsKey(key) ? _plugins[key] : null;
        }

        public AlgoPackageRef GetPackageRef(PackageKey key)
        {
            return _packageRefs.ContainsKey(key) ? _packageRefs[key] : null;
        }

        public void RegisterRepositoryLocation(RepositoryLocation location, string repoPath, bool isolation)
        {
            _server.RegisterPackageRepository(location, repoPath);
        }

        public void AddAssemblyAsPackage(Assembly assembly)
        {
            //var location = assembly.Location;

            //var packageRef = new AlgoPackageRef(Path.GetFileName(assembly.Location).ToLowerInvariant(), RepositoryLocation.Embedded,
            //    PackageIdentity.Create(new FileInfo(assembly.Location)), AlgoAssemblyInspector.FindPlugins(assembly).Select(m => new AlgoPluginRef(m, assembly.Location)));

            //RepositoryOnAdded(packageRef);
        }

        public Task WaitInit()
        {
            return Task.CompletedTask;
            //return Task.WhenAll(_repositories.Values.Select(r => r.WaitInit()));
        }


        private void OnPackageUpdated(PackageUpdate update)
        {
            switch (update.Action)
            {
                case Domain.Package.Types.UpdateAction.Upsert:
                    _server.TryGetPackage(update.Id, out var packageRef);
                    if (_packageRefs.ContainsKey(update.Package.Key))
                    {
                        RepositoryOnUpdated(packageRef);
                    }
                    else
                    {
                        RepositoryOnAdded(packageRef);
                    }
                    break;
                case Domain.Package.Types.UpdateAction.Removed:
                    break;
            }
        }

        private void RepositoryOnAdded(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var package = packageRef.PackageInfo;

                _packageRefs.Add(package.Key, packageRef);
                InitPackageRef(packageRef);

                _packages.Add(package.Key, package);
                OnPackageAdded(package);

                MergePlugins(package);
            }
        }

        private void RepositoryOnUpdated(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var package = packageRef.PackageInfo;

                DeinitPackageRef(_packageRefs[package.Key]);
                _packageRefs[package.Key] = packageRef;
                InitPackageRef(packageRef);

                _packages[package.Key] = package;
                OnPackageReplaced(package);

                MergePlugins(package);
            }
        }

        private void RepositoryOnRemoved(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var packageKey = packageRef.PackageInfo.Key;
                if (_packages.TryGetValue(packageKey, out var package))
                {
                    DeinitPackageRef(_packageRefs[package.Key]);
                    _packageRefs.Remove(packageKey);

                    _packages.Remove(packageKey);
                    OnPackageRemoved(package);

                    MergePlugins(new PackageInfo { Key = packageKey });
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
            foreach (var plugin in _plugins.Values.Where(p => p.Key.Package.Equals(package.Key)).ToList())
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
                if (_packages.TryGetValue(packageRef.PackageInfo.Key, out var package))
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
