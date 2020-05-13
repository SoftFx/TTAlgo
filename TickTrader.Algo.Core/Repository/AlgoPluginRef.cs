using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    public class AlgoPluginRef
    {
        public string Id => Metadata.Id;

        public string DisplayName => Metadata.Descriptor.DisplayName;

        public string PackagePath { get; }

        public PluginMetadata Metadata { get; }

        public virtual bool IsIsolated => false;

        public AlgoPluginRef(PluginMetadata metadata, string packagePath)
        {
            Metadata = metadata;
            PackagePath = packagePath;
        }
    }


    public class IsolatedPluginRef : AlgoPluginRef
    {
        internal IsolatedPluginRef(PluginMetadata metadata, string packagePath)
            : base(metadata, packagePath)
        {
        }

        public override bool IsIsolated => true;
    }
}
