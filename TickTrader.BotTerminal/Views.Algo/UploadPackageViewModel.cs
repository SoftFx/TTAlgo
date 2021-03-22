using Machinarium.Qnil;
using System;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class UploadPackageViewModel : BaseloadPackageViewModel
    {
        public UploadPackageViewModel(AlgoEnvironment algoEnv, string agentName, string packageId = null) : base(algoEnv, agentName, LoadPackageMode.Upload)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();

            Packages = _algoEnv.LocalAgentVM.Packages.Where(u => IsDefaultFolder(u.Location)).AsObservable();

            SetDefaultFileSource(packageId, out var package);
            SelectedFolder = package != null ? Path.GetDirectoryName(package.Identity.FilePath) : EnvService.Instance.AlgoRepositoryFolder;
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
                Packages = Directory.GetFiles(SelectedFolder, FileNameWatcherTemplate).Select(u => PackageIdentity.CreateInvalid(new FileInfo(u)))
                    .Select(i => new PackageInfo {PackageId = PackageHelper.GetPackageIdFromPath("custom", i.FilePath), Identity = i, IsValid = true })
                    .Select(info => new AlgoPackageViewModel(info, _algoEnv.LocalAgentVM, false));
                NotifyOfPropertyChange(nameof(Packages));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private bool IsDefaultFolder(string location) => location == PackageHelper.LocalRepositoryId || location == PackageHelper.CommonRepositoryId;
    }
}
