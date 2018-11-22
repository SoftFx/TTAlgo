using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class AlgoPackageViewModel : PropertyChangedBase
    {
        public PackageInfo Info { get; }

        public AlgoAgentViewModel Agent { get; }


        public PackageKey Key => Info.Key;

        public PackageIdentity Identity => Info.Identity;

        public bool IsValid => Info.IsValid;

        public bool IsLocked => Info.IsLocked;

        public RepositoryLocation Location => Info.Key.Location;

        public string DisplayName => Info.Identity.FileName;

        public string FilePath => Info.Identity.FilePath;

        public IObservableList<AlgoPluginViewModel> Plugins { get; }

        public bool CanRemovePackage => Agent.Model.AccessManager.CanRemovePackage();

        public bool IsRemote => Agent.Model.IsRemote;

        public bool IsLocal =>!Agent.Model.IsRemote;

        public bool CanUploadPackage => IsLocal;

        public bool CanDownloadPackage => IsRemote && Agent.Model.AccessManager.CanDownloadPackage();


        public AlgoPackageViewModel(PackageInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            Plugins = Agent.Plugins.Where(p => PluginIsFromPackage(p)).AsObservable();

            Agent.Model.PackageStateChanged += OnPackageStateChanged;
            Agent.Model.AccessLevelChanged += OnAccessLevelChanged;
        }


        public void RemovePackage()
        {
            Agent.RemovePackage(Info.Key).Forget();
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
            if (Info.Key.Equals(package.Key))
            {
                NotifyOfPropertyChange(nameof(IsValid));
                NotifyOfPropertyChange(nameof(IsLocked));
            }
        }

        private bool PluginIsFromPackage(AlgoPluginViewModel plugin)
        {
            return Info.Key.Name == plugin.Key.PackageName && Info.Key.Location == plugin.Key.PackageLocation;
        }

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanRemovePackage));
            NotifyOfPropertyChange(nameof(CanDownloadPackage));
        }
    }
}
