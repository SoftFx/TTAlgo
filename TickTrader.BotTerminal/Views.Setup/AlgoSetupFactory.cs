using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public static class AlgoSetupFactory
    {
        public static PluginSetupViewModel CreateSetup(PluginInfo plugin, SetupMetadataInfo metadata, SetupContextInfo context)
        {
            switch (plugin.Descriptor.Type)
            {
                case AlgoTypes.Robot: return new TradeBotSetupViewModel(plugin, metadata, context);
                case AlgoTypes.Indicator: return new IndicatorSetupViewModel(plugin, metadata, context);
                default: throw new ArgumentException("Unknown plugin type");
            }
        }
    }
}
