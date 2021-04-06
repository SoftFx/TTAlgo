using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class AlgoPackageViewModel : PropertyChangedBase
    {
        public PackageInfo Info { get; }

        public AlgoAgentViewModel Agent { get; }

        public string Key => Info.PackageId;

        public PackageIdentity Identity => Info.Identity;

        public bool IsValid => Info.IsValid;

        public bool IsLocked => Info.IsLocked;

        public string Name { get; }

        public string Location { get; }

        public string DisplayName => Info.Identity.FileName;

        public string Description { get; }

        public string FileName => Info.Identity.FileName;

        public IObservableList<AlgoPluginViewModel> Plugins { get; }

        public bool CanRemovePackage => Agent.Model.AccessManager.CanRemovePackage();

        public bool IsRemote => Agent.Model.IsRemote;

        public bool IsLocal => !Agent.Model.IsRemote;

        public bool CanUploadPackage => IsLocal;

        public bool CanDownloadPackage => IsRemote && Agent.Model.AccessManager.CanDownloadPackage();


        public AlgoPackageViewModel(PackageInfo info, AlgoAgentViewModel agent, bool listenEvents = true)
        {
            Info = info;
            Agent = agent;

            PackageId.Unpack(info.PackageId, out var pkgId);
            Location = pkgId.LocationId;
            Name = pkgId.PackageName;

            Plugins = Agent.Plugins.Where(p => PluginIsFromPackage(p)).AsObservable();
            Description = $"Server {Agent.Name}. Path {Info.Identity.FilePath}";

            if (listenEvents)
            {
                Agent.Model.PackageStateChanged += OnPackageStateChanged;
                Agent.Model.AccessLevelChanged += OnAccessLevelChanged;
            }
        }


        public void RemovePackage()
        {
            Agent.RemovePackage(Info.PackageId).Forget();
        }

        public void UploadPackage()
        {
            Agent.OpenUploadPackageDialog(Key);
        }

        public void DownloadPackage()
        {
            Agent.OpenDownloadPackageDialog(Key);
        }


        private void OnPackageStateChanged(PackageInfo package)
        {
            if (Info.PackageId == package.PackageId)
            {
                NotifyOfPropertyChange(nameof(IsValid));
                NotifyOfPropertyChange(nameof(IsLocked));
            }
        }

        private bool PluginIsFromPackage(AlgoPluginViewModel plugin)
        {
            return Info.PackageId == plugin.Key.PackageId;
        }

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanRemovePackage));
            NotifyOfPropertyChange(nameof(CanDownloadPackage));
        }
    }
}
