using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class PluginInfoEntity
    {
        public PluginKeyEntity Key { get; set; }

        public PluginDescriptorEntity Descriptor { get; set; }


        public PluginInfoEntity()
        {
            Key = new PluginKeyEntity();
            Descriptor = new PluginDescriptorEntity();
        }


        internal void UpdateModel(PluginInfo model)
        {
            Key.UpdateModel(model.Key);
            Descriptor.UpdateModel(model.Descriptor);
        }

        internal void UpdateSelf(PluginInfo model)
        {
            Key.UpdateSelf(model.Key);
            Descriptor.UpdateSelf(model.Descriptor);
        }
    }
}
