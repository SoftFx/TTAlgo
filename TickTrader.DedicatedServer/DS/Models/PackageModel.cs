using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class PackageModel : IPackage, IDisposable
    {
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

        public IEnumerable<PlguinInfo> GetPlugins()
        {
            return Container?.Plugins
                .Select(p => new PlguinInfo(new PluginKey(Name, p.Descriptor.Id), p.Descriptor))
                ?? Enumerable.Empty<PlguinInfo>();
        }

        public IEnumerable<PlguinInfo> GetPluginsByType(AlgoTypes type)
        {
            return Container?.Plugins
                .Where(p => p.Descriptor.AlgoLogicType == type)
                .Select(p => new PlguinInfo(new PluginKey(Name, p.Descriptor.Id), p.Descriptor))
                ?? Enumerable.Empty<PlguinInfo>();
        }

        public AlgoPluginRef GetPluginRef(string id)
        {
            return Container?.Plugins.FirstOrDefault(pr => pr.Id == id);
        }

        public void Dispose()
        {
            Container?.Dispose();
        }
    }
}
