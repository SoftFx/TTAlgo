using TickTrader.Algo.Core.Container;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    public class AlgoPluginRef
    {
        public string Id => Metadata.Id;

        public string DisplayName => Metadata.Descriptor.DisplayName;

        public PluginMetadata Metadata { get; }
        public virtual bool IsIsolated => false;

        public AlgoPluginRef(PluginMetadata metadata)
        {
            Metadata = metadata;
        }

        public virtual PluginExecutor CreateExecutor()
        {
            return new PluginExecutor(Id);
        }
    }


    public class IsolatedPluginRef : AlgoPluginRef
    {
        private AlgoSandbox _sandbox;

        internal IsolatedPluginRef(PluginMetadata metadata, AlgoSandbox sandbox)
            : base(metadata)
        {
            _sandbox = sandbox;
        }

        public override bool IsIsolated => true;

        public override PluginExecutor CreateExecutor()
        {
            return _sandbox.CreateExecutor(Id);
        }
    }
}
