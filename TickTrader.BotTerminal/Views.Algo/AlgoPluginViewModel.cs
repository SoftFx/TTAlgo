using Caliburn.Micro;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class AlgoPluginViewModel : PropertyChangedBase
    {
        public PluginInfo Info { get; }

        public AlgoAgentViewModel Agent { get; }


        public PluginKey Key => Info.Key;

        public PluginDescriptor Descriptor => Info.Descriptor;

        public string DisplayName => Info.Descriptor.UiDisplayName;

        public string Category => Info.Descriptor.Category;


        public AlgoPluginViewModel(PluginInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;
        }
    }
}
