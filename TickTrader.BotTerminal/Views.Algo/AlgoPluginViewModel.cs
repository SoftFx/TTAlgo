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
        public enum FolderType { Common, Local, Embedded }

        public PluginInfo Info { get; }

        public AlgoAgentViewModel Agent { get; }

        public PluginKey Key => Info.Key;

        public PluginDescriptor Descriptor => Info.Descriptor;

        public string DisplayName => Info.Descriptor.UiDisplayName;

        public string PackageDisplayName { get; }

        public string PackageNameWithoutExtension { get; }

        public string FullPackagePath { get; }

        public string Category => Info.Descriptor.Category;

        public AlgoTypes Type => Info.Descriptor.Type;

        public FolderType Folder { get; }

        public string Description { get; }

        public string Group { get; }


        public AlgoPluginViewModel(PluginInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            PackageDisplayName = Info.Key.PackageName;

            var packagePath = "Unknown path";

            if (Agent.Model.Packages.Snapshot.TryGetValue(info.Key.GetPackageKey(), out var packageInfo))
            {
                PackageDisplayName = packageInfo.Identity.FileName;
                packagePath = Path.GetDirectoryName(packageInfo.Identity.FilePath);
                FullPackagePath = $"Full path: {packageInfo.Identity.FilePath}";
            }

            PackageNameWithoutExtension = GetPathWithoutExtension(PackageDisplayName);
            Description = string.Join(Environment.NewLine, Info.Descriptor.Description, string.Empty, $"Package {PackageDisplayName} at {packagePath}").Trim();

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

            switch (Info.Key.PackageLocation)
            {
                case RepositoryLocation.LocalRepository:
                case RepositoryLocation.LocalExtensions:
                    Folder = FolderType.Local;
                    break;
                case RepositoryLocation.Embedded:
                    Folder = FolderType.Embedded;
                    break;
                default:
                    Folder = FolderType.Common;
                    break;
            }
        }

        private string GetPathWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);
    }
}
