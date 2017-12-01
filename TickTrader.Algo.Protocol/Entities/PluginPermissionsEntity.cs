using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class PluginPermissionsEntity : IProtocolEntity<PluginPermissions>
    {
        public bool TradeAllowed { get; set; }


        internal void UpdateModel(PluginPermissions model)
        {
            model.TradeAllowed = TradeAllowed;
        }

        internal void UpdateSelf(PluginPermissions model)
        {
            TradeAllowed = model.TradeAllowed;
        }


        void IProtocolEntity<PluginPermissions>.UpdateModel(PluginPermissions model)
        {
            UpdateModel(model);
        }

        void IProtocolEntity<PluginPermissions>.UpdateSelf(PluginPermissions model)
        {
            UpdateSelf(model);
        }
    }
}
