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
        public PluginCatalogItem(PluginCatalogKey key, AlgoPluginRef pluginRef, string filePath)
        {
            this.Key = key;
            this.Ref = pluginRef;
            this.FilePath = filePath;
        }

        public PluginCatalogKey Key { get; private set; }
        public AlgoPluginRef Ref { get; private set; }
        public string FilePath { get; private set; }
        public AlgoPluginDescriptor Descriptor { get { return Ref.Descriptor; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        public string Category { get { return Descriptor.Category; } }
    }
}
