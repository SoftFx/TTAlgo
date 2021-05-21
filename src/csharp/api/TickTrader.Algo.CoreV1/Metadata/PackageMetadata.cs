using System.Collections.Generic;

namespace TickTrader.Algo.CoreV1.Metadata
{
    public class PackageMetadata
    {
        public string Id { get; }

        public List<PluginMetadata> Plugins { get; }

        public List<ReductionMetadata> Reductions { get; }


        public PackageMetadata(string packageId, List<PluginMetadata> plugins, List<ReductionMetadata> reductions)
        {
            Id = packageId;
            Plugins = plugins;
            Reductions = reductions;
        }
    }
}
