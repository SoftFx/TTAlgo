using System.Linq;

namespace TickTrader.Algo.Domain
{
    public partial class PackageInfo
    {
        public PluginInfo GetPlugin(PluginKey key)
        {
            if (key.PackageId != PackageId)
                return null;

            return Plugins.FirstOrDefault(p => p.Key.DescriptorId == key.DescriptorId);
        }
    }
}
