﻿using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class AlgoPackageViewModel : PropertyChangedBase
    {
        public PackageInfo Info { get; }

        public AlgoAgentViewModel Agent { get; }


        public bool IsValid => Info.IsValid;

        public bool IsLocked => Info.IsLocked;

        public RepositoryLocation Location => Info.Key.Location;

        public string DisplayName => Info.Identity.FileName;

        public IObservableList<AlgoPluginViewModel> Plugins { get; }


        public AlgoPackageViewModel(PackageInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            Plugins = Agent.Plugins.Where(p => PluginIsFromPackage(p)).AsObservable();

            Agent.Model.PackageStateChanged += OnPackageStateChanged;
        }


        public void UpdatePackage()
        {
        }

        public void RemovePackage()
        {
            Agent.RemovePackage(Info.Key).Forget();
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
    }
}
