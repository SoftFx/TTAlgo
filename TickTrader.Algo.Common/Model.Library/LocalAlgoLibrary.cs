using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model
{
    public class LocalAlgoLibrary : IAlgoLibrary
    {
        private Dictionary<RepositoryLocation, PackageRepository> _repositories;
        private Dictionary<PackageKey, AlgoPackageRef> _packageRefs;
        private Dictionary<PluginKey, AlgoPluginRef> _pluginRefs;
        private Dictionary<PackageKey, PackageInfo> _packages;
        private Dictionary<PluginKey, PluginInfo> _plugins;
        private IAlgoCoreLogger _logger;
        private object _updateLock = new object();


        public event Action<UpdateInfo<PackageInfo>> PackageUpdated;

        public event Action<UpdateInfo<PluginInfo>> PluginUpdated;


        public LocalAlgoLibrary(IAlgoCoreLogger logger)
        {
            _logger = logger;

            _repositories = new Dictionary<RepositoryLocation, PackageRepository>();
            _packageRefs = new Dictionary<PackageKey, AlgoPackageRef>();
            _pluginRefs = new Dictionary<PluginKey, AlgoPluginRef>();
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
            return _packageRefs.ContainsKey(key) ? _packageRefs[key] : null;
        }

        public AlgoPluginRef GetPluginRef(PluginKey key)
        {
            return _pluginRefs.ContainsKey(key) ? _pluginRefs[key] : null;
        }

        public void RegisterRepositoryLocation(RepositoryLocation location, string repoPath)
        {
            if (_repositories.ContainsKey(location))
                throw new ArgumentException($"Cannot register multiple paths for location '{location}'");

            var repo = new PackageRepository(repoPath, location, _logger);
            _repositories.Add(location, repo);

            repo.Added += RepositoryOnAdded;
            repo.Updated += RepositoryOnUpdated;
            repo.Removed += RepositoryOnRemoved;

            repo.Start();
        }

        public void AddAssemblyAsPackage(Assembly assembly)
        {
            var packageRef = new AlgoPackageRef(Path.GetFileName(assembly.Location).ToLowerInvariant(), RepositoryLocation.Embedded,
                File.GetLastWriteTimeUtc(assembly.Location), AlgoAssemblyInspector.FindPlugins(assembly).Select(m => new AlgoPluginRef(m)));

            RepositoryOnAdded(packageRef);
        }

        public Task WaitInit()
        {
            return Task.WhenAll(_repositories.Values.Select(r => r.WaitInit()));
        }


        private void RepositoryOnAdded(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var package = packageRef.ToInfo();
                _packageRefs.Add(package.Key, packageRef);
                _packages.Add(package.Key, package);
                OnPackageAdded(package);
                MergePlugins(package, packageRef);
            }
        }

        private void RepositoryOnUpdated(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var package = packageRef.ToInfo();
                _packageRefs[package.Key] = packageRef;
                _packages[package.Key] = package;
                OnPackageReplaced(package);
                MergePlugins(package, packageRef);
            }
        }

        private void RepositoryOnRemoved(AlgoPackageRef packageRef)
        {
            lock (_updateLock)
            {
                var packageKey = packageRef.GetKey();
                if (_packages.TryGetValue(packageKey, out var package))
                {
                    _packageRefs.Remove(packageKey);
                    _packages.Remove(packageKey);
                    OnPackageRemoved(package);
                    MergePlugins(new PackageInfo { Key = packageKey }, null);
                }
            }
        }

        private void MergePlugins(PackageInfo package, AlgoPackageRef packageRef)
        {
            // upsert
            foreach (var plugin in package.Plugins)
            {
                if (!_plugins.ContainsKey(plugin.Key))
                {
                    _plugins.Add(plugin.Key, plugin);
                    _pluginRefs.Add(plugin.Key, packageRef.GetPluginRef(plugin.Key.DescriptorId));
                    OnPluginAdded(plugin);
                }
                else
                {
                    _plugins[plugin.Key] = plugin;
                    _pluginRefs[plugin.Key] = packageRef.GetPluginRef(plugin.Key.DescriptorId);
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
                    _pluginRefs.Remove(plugin.Key);
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

        #endregion Event invokers
    }
}
