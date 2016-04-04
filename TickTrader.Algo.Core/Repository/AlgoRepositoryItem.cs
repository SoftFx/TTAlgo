using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Repository
{
    public class AlgoRepositoryItem
    {
        private AlgoSandbox sandbox;

        internal AlgoRepositoryItem(AlgoSandbox sandbox, AlgoPluginDescriptor info)
        {
            this.sandbox = sandbox;
            this.Descriptor = info;
        }

        public string Id { get { return Descriptor.Id; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        public AlgoPluginDescriptor Descriptor { get; private set; }
    }
}
