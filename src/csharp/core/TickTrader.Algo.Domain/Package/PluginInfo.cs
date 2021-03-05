namespace TickTrader.Algo.Domain
{
    public partial class PluginInfo
    {
        public PluginInfo(PluginKey key, PluginDescriptor descriptor)
        {
            Key = key;
            Descriptor_ = descriptor;
        }
    }
}
