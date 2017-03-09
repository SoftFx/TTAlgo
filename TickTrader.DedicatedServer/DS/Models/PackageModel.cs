using System;
using TickTrader.Algo.Core;

namespace TickTrader.DedicatedServer.DS.Models
{
    public class PackageModel: IPackage, IDisposable
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

        public void Dispose()
        {
            Container?.Dispose();
        }
    }
}
