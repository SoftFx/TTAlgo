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
    internal class DownloadPackageViewModel : BaseloadPackageViewModel
    {
        private SortedSet<string> _filesInFolder;

        public DownloadPackageViewModel(AlgoEnvironment algoEnv, string agentName, PackageKey package) : base(algoEnv, agentName, LoadPackageMode.Download)
        {
            _filesInFolder = new SortedSet<string>();
            _logger = NLog.LogManager.GetCurrentClassLogger();

            UpdatePackageSource();
            SetDefaultFileSource(package, out _);
            SelectedFolder = EnvService.Instance.AlgoCommonRepositoryFolder;
        }

        protected override void UpdateAgentPackage(ListUpdateArgs<AlgoPackageViewModel> args)
        {
            var selectedFile = FileNameSource; //Restore previous value after package upload

            UpdatePackageSource();

            FileNameSource = Packages.Any(u => u.FileName == selectedFile) ? selectedFile : Packages.FirstOrDefault()?.FileName;
        }

        protected override void WatcherEventHandling(object o, object e) => UploadSelectedSource();

        protected override void UploadSelectedSource()
        {
            _filesInFolder = new SortedSet<string>(Directory.GetFiles(SelectedFolder, FileNameWatcherTemplate).Select(u => Path.GetFileName(u)));
            RefreshTargetName();
        }

        protected override bool CheckFileNameTarget(string name) => !_filesInFolder.Contains(name);

        private void UpdatePackageSource() => Packages = SelectedBotAgent.Packages.Where(p => p.Location == RepositoryLocation.LocalRepository).Select(u => u.Identity).AsObservable();
    }
}
