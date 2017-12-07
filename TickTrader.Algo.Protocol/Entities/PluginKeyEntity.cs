using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class PluginKeyEntity
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
    }
}
