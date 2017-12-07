using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class PluginPermissionsEntity
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
    }
}
