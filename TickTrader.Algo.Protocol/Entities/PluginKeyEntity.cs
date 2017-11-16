using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class PluginKeyEntity : IProtocolEntity<PluginKey>
    {
        public string PackageName { get; set; }

        public string DescriptorId { get; set; }


        internal void UpdateModel(PluginKey model)
        {
            model.PackageName = PackageName;
            model.DescriptorId = DescriptorId;
        }

        internal void UpdateSelf(PluginKey model)
        {
            PackageName = model.PackageName;
            DescriptorId = model.DescriptorId;
        }


        void IProtocolEntity<PluginKey>.UpdateModel(PluginKey model)
        {
            UpdateModel(model);
        }

        void IProtocolEntity<PluginKey>.UpdateSelf(PluginKey model)
        {
            UpdateSelf(model);
        }
    }
}
