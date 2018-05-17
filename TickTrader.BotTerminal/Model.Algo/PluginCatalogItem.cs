using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public class PluginCatalogItem
    {
        public PluginInfo Info { get; }

        public PluginKey Key => Info.Key;

        public PluginDescriptor Descriptor => Info.Descriptor;

        public string DisplayName => Info.Descriptor.UiDisplayName;

        public string Category => Info.Descriptor.Category;


        public PluginCatalogItem(PluginInfo info)
        {
            Info = info;
        }
    }
}
