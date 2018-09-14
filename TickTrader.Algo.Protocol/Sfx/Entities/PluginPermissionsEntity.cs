using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class PluginPermissionsEntity
    {
        public bool TradeAllowed { get; set; }

        public bool Isolated { get; set; }


        internal void UpdateModel(PluginPermissions model)
        {
            model.TradeAllowed = TradeAllowed;
            model.Isolated = Isolated;
        }

        internal void UpdateSelf(PluginPermissions model)
        {
            TradeAllowed = model.TradeAllowed;
            Isolated = model.Isolated;
        }
    }
}
