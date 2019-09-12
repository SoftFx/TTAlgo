using System;
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

        public virtual PluginExecutorCore CreateExecutor()
        {
            return new PluginExecutorCore(Id);
        }

        public virtual PluignExecutorFactory CreateExecutorFactory()
        {
            return new PluignExecutorFactory(Id);
        }

        public virtual T CreateObject<T>()
            where T : MarshalByRefObject, new()
        {
            return new T();
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

        public override PluginExecutorCore CreateExecutor()
        {
            return _sandbox.CreateExecutor(Id);
        }

        public override PluignExecutorFactory CreateExecutorFactory()
        {
            return _sandbox.CreateExecutorFactory(Id);
        }

        public override T CreateObject<T>()
        {
            return _sandbox.Activate<T>();
        }
    }
}
