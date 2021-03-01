using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class UploadPackageViewModel : BaseloadPackageViewModel
    {
        public UploadPackageViewModel(AlgoEnvironment algoEnv, string agentName, PackageKey package = null) : base(algoEnv, agentName, LoadPackageMode.Upload)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            Packages = _algoEnv.LocalAgentVM.Packages.Where(u => IsDefaultFolder(u.Location)).Select(u => u.Identity).AsObservable();

            SetDefaultFileSource(package, out PackageIdentity identity);
            SelectedFolder = identity != null ? Path.GetDirectoryName(identity.FilePath) : EnvService.Instance.AlgoRepositoryFolder;
        }

        protected override void UpdateAgentPackage(ListUpdateArgs<AlgoPackageViewModel> args) => RefreshTargetName();

        protected override bool CheckFileNameTarget(string name) => !SelectedBotAgent.PackageList.Any(p => p.Identity.FileName == name);

        protected override void WatcherEventHandling(object o, object e)
        {
            var selectedFile = FileNameSource; //Restore previous value after Algo package upload

            UploadSelectedSource();

            FileNameSource = Packages.Any(u => u.FileName == selectedFile) ? selectedFile : Packages.FirstOrDefault()?.FileName;
        }

        protected override void UploadSelectedSource()
        {
            try
            {
                Packages = Directory.GetFiles(SelectedFolder, FileNameWatcherTemplate).Select(u => PackageIdentity.CreateInvalid(new FileInfo(u)));
                NotifyOfPropertyChange(nameof(Packages));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private bool IsDefaultFolder(RepositoryLocation path) => path == RepositoryLocation.LocalRepository || path == RepositoryLocation.CommonRepository;
    }
}
