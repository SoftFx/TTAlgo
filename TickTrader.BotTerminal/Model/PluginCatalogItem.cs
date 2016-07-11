using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public class PluginCatalogItem
    {
        public PluginCatalogItem(PluginCatalogKey key, AlgoPluginRef pluginRef)
        {
            this.Key = key;
            this.Ref = pluginRef;
        }

        public PluginCatalogKey Key { get; private set; }
        public AlgoPluginRef Ref { get; private set; }
        public AlgoPluginDescriptor Descriptor { get { return Ref.Descriptor; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        public string Category { get { return Descriptor.Category; } }
    }
}
