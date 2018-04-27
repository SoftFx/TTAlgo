using TickTrader.Algo.Core.Container;

namespace TickTrader.Algo.Core.Metadata
{
    public class AlgoPluginRef
    {
        public string Id => Metadata.Id;

        public string DisplayName => Metadata.Descriptor.DisplayName;

        public PluginMetadata Metadata { get; private set; }


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


        public override PluginExecutor CreateExecutor()
        {
            return _sandbox.CreateExecutor(Id);
        }
    }
}
