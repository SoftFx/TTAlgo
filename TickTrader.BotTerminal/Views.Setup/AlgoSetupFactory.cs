using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public static class AlgoSetupFactory
    {
        public static PluginSetupViewModel CreateSetup(PluginInfo plugin, SetupMetadata metadata, IPluginIdProvider idProvider)
        {
            switch (plugin.Descriptor.Type)
            {
                case AlgoTypes.Robot: return new TradeBotSetupViewModel(plugin, metadata, idProvider);
                case AlgoTypes.Indicator: return new IndicatorSetupViewModel(plugin, metadata, idProvider);
                default: throw new ArgumentException("Unknown plugin type");
            }
        }
    }
}
