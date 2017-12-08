using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.BotAgent.BA.Repository;

namespace TickTrader.BotAgent.BA.Models
{
    public class PackageModel : IPackage, IDisposable
    {
        private int _refCount;

        public PackageModel(string name, DateTime created, PluginContainer container)
        {
            Name = name;
            Created = created;
            Container = container;
        }

        public string Name { get; private set; }
        public DateTime Created { get; private set; }
        public PluginContainer Container { get; private set; }
        public bool IsValid => Container != null;
        public bool IsLocked => _refCount > 0;

        public event Action<PackageModel> IsLockedChanged;

        public IEnumerable<PluginInfo> GetPlugins()
        {
            return Container?.Plugins
                .Select(p => new PluginInfo(new PluginKey(Name, p.Descriptor.Id), p.Descriptor))
                ?? Enumerable.Empty<PluginInfo>();
        }

        public IEnumerable<PluginInfo> GetPluginsByType(AlgoTypes type)
        {
            return Container?.Plugins
                .Where(p => p.Descriptor.AlgoLogicType == type)
                .Select(p => new PluginInfo(new PluginKey(Name, p.Descriptor.Id), p.Descriptor))
                ?? Enumerable.Empty<PluginInfo>();
        }

        public AlgoPluginRef GetPluginRef(string id)
        {
            return Container?.Plugins.FirstOrDefault(pr => pr.Id == id);
        }

        internal void IncrementRef()
        {
            _refCount++;
            if (_refCount == 1)
                IsLockedChanged?.Invoke(this);
        }

        internal void DecrementRef()
        {
            _refCount--;
            if (_refCount == 0)
                IsLockedChanged?.Invoke(this);
        }

        public void Dispose()
        {
            Container?.Dispose();
        }

        public bool NameEquals(string name)
        {
            return PackageStorage.GetPackageKey(name) == PackageStorage.GetPackageKey(Name);
        }
    }
}
