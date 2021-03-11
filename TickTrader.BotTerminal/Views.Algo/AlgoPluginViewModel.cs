using System;
using System.IO;
using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Core.Metadata;
using System.Diagnostics;
using System.Windows;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class AlgoPluginViewModel : PropertyChangedBase
    {
        private const string UnknownPath = "Unknown path";

        public const string LevelHeader = nameof(CurrentGroup);
        public const string PackageLevelHeader = nameof(FullPackagePath);
        public const string SortPackageLevelHeader = nameof(PackageName);
        public const string BotLevelHeader = nameof(DisplayName);


        public enum FolderType { Common, Local, Embedded }

        public enum GroupType { Unknown, Indicators, Bots }

        public PluginInfo PluginInfo { get; }

        public PackageInfo PackageInfo { get; }

        public AlgoAgentViewModel Agent { get; }

        public PluginKey Key { get; }

        public PluginDescriptor Descriptor { get; }

        public string DisplayName => Descriptor.UiDisplayName;

        public string PackageName { get; }

        public string PackageDirectory { get; }

        public string FullPackagePath { get; }

        public string DisplayPackagePath { get; }

        public Metadata.Types.PluginType Type => Descriptor.Type;

        public FolderType Folder { get; }

        public string Description { get; }

        public GroupType CurrentGroup { get; }

        public string Category => Descriptor.Category; //used in ContextMenu AddIndicator/AddBot

        public bool IsRemote => Agent.Model.IsRemote;

        public bool IsLocal => !Agent.Model.IsRemote;

        public bool IsTradeBot => Descriptor.IsTradeBot;

        public bool IsIndicator => Descriptor.IsIndicator;

        public AlgoPluginViewModel(PluginInfo info, AlgoAgentViewModel agent)
        {
            PluginInfo = info;
            Agent = agent;

            Key = info.Key;
            Descriptor = info.Descriptor_;

            PackageName = Key.Package.Name;
            PackageDirectory = UnknownPath;
            CurrentGroup = (GroupType)Type;

            if (Agent.Model.Packages.Snapshot.TryGetValue(info.Key.Package, out var packageInfo))
            {
                PackageInfo = packageInfo;

                PackageName = packageInfo.Identity.FileName;
                FullPackagePath = packageInfo.Identity.FilePath;

                PackageDirectory = Path.GetDirectoryName(FullPackagePath);

                DisplayPackagePath = $"Full path: {FullPackagePath}{Environment.NewLine}Last modified: {PackageInfo.Identity.LastModifiedUtc} (UTC)";
                Description = string.Join(Environment.NewLine, Descriptor.Description, string.Empty, $"Algo Package {PackageName} at {PackageDirectory}").Trim();
            }

            switch (Key.Package.Location)
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

        public void RemovePackage()
        {
            Agent.RemovePackage(PackageInfo.Key).Forget();
        }

        public void UploadPackage()
        {
            Agent.OpenUploadPackageDialog(PackageInfo.Key);
        }

        public void DownloadPackage()
        {
            Agent.OpenDownloadPackageDialog(PackageInfo.Key);
        }

        public void OpenFolder()
        {
            Process.Start(PackageDirectory);
        }

        public void CopyPath()
        {
            Clipboard.SetText(PackageDirectory);
        }
    }
}
