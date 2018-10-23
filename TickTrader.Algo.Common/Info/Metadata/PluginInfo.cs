using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Info
{
    public class PluginInfo
    {
        public PluginKey Key { get; set; }

        public PluginDescriptor Descriptor { get; set; }


        public PluginInfo() { }

        public PluginInfo(PluginKey key, PluginDescriptor descriptor)
        {
            Key = key;
            Descriptor = descriptor;
        }


        public override string ToString()
        {
            return Key.ToString();
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
