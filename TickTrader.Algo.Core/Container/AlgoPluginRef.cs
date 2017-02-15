using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Container;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Core.Metadata
{
    public class AlgoPluginRef
    {
        public AlgoPluginRef(AlgoPluginDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }

        public string Id { get { return Descriptor.Id; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        public AlgoPluginDescriptor Descriptor { get; private set; }

        public virtual PluginExecutor CreateExecutor()
        {
            return new PluginExecutor(Id);
        }
    }

    public class IsolatedPluginRef : AlgoPluginRef
    {
        private AlgoSandbox sandbox;

        internal IsolatedPluginRef(AlgoPluginDescriptor descriptor, AlgoSandbox sandbox)
            : base(descriptor)
        {
            this.sandbox = sandbox;
        }

        public override PluginExecutor CreateExecutor()
        {
            return sandbox.CreateExecutor(Id);
        }
    }
}
