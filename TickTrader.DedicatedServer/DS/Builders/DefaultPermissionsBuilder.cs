using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.DedicatedServer.DS.Builders
{
    public class DefaultPermissionsBuilder
    {
        public PluginPermissions Build()
        {
            return new PluginPermissions
            {
                TradeAllowed = true
            };
        }
    }
}
