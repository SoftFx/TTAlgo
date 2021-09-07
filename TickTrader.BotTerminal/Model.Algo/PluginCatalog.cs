using Machinarium.Qnil;
using NLog;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class PluginCatalogItem
    {
        public IAlgoAgent Agent { get; }

        public PluginInfo Info { get; }

        public PluginKey Key => Info.Key;

        public PluginDescriptor Descriptor => Info.Descriptor_;

        public string DisplayName => Info.Descriptor_.UiDisplayName;

        public string Category => Info.Descriptor_.Category;


        public PluginCatalogItem(IAlgoAgent agent, PluginInfo info)
        {
            Agent = agent;
            Info = info;
        }
    }


    internal class PluginCatalog
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private IAlgoAgent _algoAgent;


        public IVarList<PluginCatalogItem> PluginList { get; }

        public IVarList<PluginCatalogItem> Indicators { get; }

        public IVarList<PluginCatalogItem> BotTraders { get; }


        public PluginCatalog(IAlgoAgent algoAgent)
        {
            _algoAgent = algoAgent;

            PluginList = algoAgent.Plugins.OrderBy((k, p) => p.Descriptor_.UiDisplayName).Select(info => new PluginCatalogItem(_algoAgent, info));
            Indicators = PluginList.Where(i => i.Descriptor.IsIndicator);
            BotTraders = PluginList.Where(i => i.Descriptor.IsTradeBot);
        }
    }
}
