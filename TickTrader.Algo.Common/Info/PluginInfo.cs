namespace TickTrader.Algo.Common.Info
{
    public class PluginInfo
    {
        public PluginKey Key { get; set; }

        public PluginMetadataInfo Metadata { get; set; }


        public PluginInfo() { }

        public PluginInfo(PluginKey key, PluginMetadataInfo metadata)
        {
            Key = key;
            Metadata = metadata;
        }
    }
}
