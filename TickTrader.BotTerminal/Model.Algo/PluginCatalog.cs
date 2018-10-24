using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class PluginCatalogItem
    {
        public IAlgoAgent Agent { get; }

        public PluginInfo Info { get; }

        public PluginKey Key => Info.Key;

        public PluginDescriptor Descriptor => Info.Descriptor;

        public string DisplayName => Info.Descriptor.UiDisplayName;

        public string Category => Info.Descriptor.Category;


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

            PluginList = algoAgent.Plugins.OrderBy((k, p) => p.Descriptor.UiDisplayName).Select(info => new PluginCatalogItem(_algoAgent, info));
            Indicators = PluginList.Where(i => i.Descriptor.Type == AlgoTypes.Indicator);
            BotTraders = PluginList.Where(i => i.Descriptor.Type == AlgoTypes.Robot);
        }
    }
}
