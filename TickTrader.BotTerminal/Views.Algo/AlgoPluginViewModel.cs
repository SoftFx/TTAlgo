using Caliburn.Micro;
using System;
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

        public AlgoTypes Type => Info.Descriptor.Type;

        public string Description { get; }


        public AlgoPluginViewModel(PluginInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            Description = string.Join(Environment.NewLine, Info.Descriptor.Description, string.Empty, $"Package {Info.Key.PackageName} at {Info.Key.PackageLocation}").Trim();
        }
    }
}
