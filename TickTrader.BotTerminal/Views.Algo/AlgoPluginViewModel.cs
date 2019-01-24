using Caliburn.Micro;
using System;
using System.IO;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

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

        public string Group { get; }


        public AlgoPluginViewModel(PluginInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            var packageDisplayName = Info.Key.PackageName;
            var packagePath = "Unknown path";
            if (Agent.Model.Packages.Snapshot.TryGetValue(info.Key.GetPackageKey(), out var packageInfo))
            {
                packageDisplayName = packageInfo.Identity.FileName;
                packagePath = Path.GetDirectoryName(packageInfo.Identity.FilePath);
            }

            Description = string.Join(Environment.NewLine, Info.Descriptor.Description, string.Empty, $"Agent {Agent.Name}. Package {packageDisplayName} at {packagePath}").Trim();

            switch (Type)
            {
                case AlgoTypes.Robot:
                    Group = "Bots";
                    break;
                case AlgoTypes.Indicator:
                    Group = "Indicators";
                    break;
                case AlgoTypes.Unknown:
                    Group = "Unknown";
                    break;
            }
        }
    }
}
