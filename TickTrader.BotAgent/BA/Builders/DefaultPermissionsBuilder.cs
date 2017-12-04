using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;

namespace TickTrader.BotAgent.BA.Builders
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
