using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class UploadPackageViewModel : Screen, IWindowModel
    {
        private const string FileNameTemplate = "*.ttalgo";

        private readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private FileSystemWatcher _watcher;
        private AlgoEnvironment _algoEnv;
        private string _fileNameSource, _filePathSource, _error;
        private bool _isEnabled;


        public ProgressViewModel UploadProgress { get; }

        public AlgoAgentViewModel SelectedBotAgent { get; }

        public IEnumerable<PackageIdentity> Packages { get; private set; }

        public bool HasError => !string.IsNullOrEmpty(_error);

        public string FullPackagePath => Path.Combine(FilePathSource, FileNameSource);

        public string FileName => GenerateFileName(FileNameSource);

        public string FileNameSource
        {
            get => _fileNameSource;
            set
            {
                if (_fileNameSource == value)
                    return;

                _fileNameSource = value;

                NotifyOfPropertyChange(nameof(FileNameSource));
                NotifyOfPropertyChange(nameof(FileName));
            }
        }

        public string FilePathSource
        {
            get => _filePathSource;
            set
            {
                if (_filePathSource == value)
                    return;

                DeinitWatcher();

                _filePathSource = value;

                InitWatcher();
                UploadPackages();

                NotifyOfPropertyChange(nameof(FilePathSource));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;
                NotifyOfPropertyChange(nameof(IsEnabled));
            }
        }

        public string Error
        {
            get => _error;
            set
            {
                if (_error == value)
                    return;

                _error = value;
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }


        public UploadPackageViewModel(AlgoEnvironment algoEnv, string agentName, PackageKey package = null)
        {
            _algoEnv = algoEnv;

            DisplayName = "Upload package";
            UploadProgress = new ProgressViewModel();

            Packages = _algoEnv.LocalAgentVM.Packages.Where(u => IsDefaultFolder(u.Location)).Select(u => u.Identity).AsObservable();

            SelectedBotAgent = _algoEnv.BotAgents.Where(b => b.Agent.Name == agentName).AsObservable().FirstOrDefault()?.Agent;
            UpdatePackageSource(Packages?.FirstOrDefault(u => GetDefaultPath(u, package)));

            SelectedBotAgent.Packages.Updated += UpdatePackageTarget;
            IsEnabled = true;
        }

        public async void Ok()
        {
            IsEnabled = false;

            try
            {
                if (FileNameSource == null)
                    throw new Exception("Please select a package.");

                UploadProgress.SetMessage($"Uploading package {FileName} to {SelectedBotAgent.Name}");

                var fileSize = Packages.FirstOrDefault(u => u.FileName == FileNameSource).Size;
                var progressListener = new FileProgressListenerAdapter(UploadProgress, fileSize);

                await SelectedBotAgent.Model.UploadPackage(FileName, FullPackagePath, progressListener);

                TryClose();
            }
            catch (ConnectionFailedException ex)
            {
                Error = $"Error connecting to agent: {SelectedBotAgent.Name}";
                _logger.Error(ex, Error);
            }
            catch (Exception ex)
            {
                Error = $"{ex.Message} Failed to upload package";
                _logger.Error(ex, Error);
            }

            IsEnabled = true;
        }

        public void Cancel() => TryClose();

        protected override void OnDeactivate(bool close)
        {
            DeinitWatcher();
            SelectedBotAgent.Packages.Updated -= UpdatePackageTarget;

            base.OnDeactivate(close);
        }

        private void UpdatePackageSource(PackageIdentity identity)
        {
            FilePathSource = identity != null ? Path.GetDirectoryName(identity.FilePath) : EnvService.Instance.AlgoRepositoryFolder;
            FileNameSource = identity?.FileName ?? Packages.FirstOrDefault()?.FileName;
        }

        private void UpdatePackageTarget(ListUpdateArgs<AlgoPackageViewModel> args) => NotifyOfPropertyChange(nameof(FileName));

        private string GenerateFileName(string name) //File name generation as in Windows (a.ttalgo, a - Copy.ttalgo, a - Copy (2).ttalgo)
        {
            if (name == null)
                return null;

            var ext = Path.GetExtension(name);
            name = Path.GetFileNameWithoutExtension(name);
            var step = 0;

            do
            {
                var fileName = $"{name}{(step > 0 ? " - Copy" : "")}{(step > 1 ? $" ({step})" : "")}{ext}";

                if (!CheckName(fileName))
                    return fileName;
            }
            while (++step < int.MaxValue);

            return $"{name} - Copy ({DateTime.Now.Ticks}){ext}";
        }

        private bool CheckName(string name) => SelectedBotAgent.PackageList.Any(p => p.Identity.FileName == name);

        private bool IsDefaultFolder(RepositoryLocation path) => path == RepositoryLocation.LocalRepository || path == RepositoryLocation.CommonRepository;

        private bool GetDefaultPath(PackageIdentity identity, PackageKey key) => key != null ? identity.FileName.ToLowerInvariant() == key.Name.ToLowerInvariant() : true;

        private void InitWatcher()
        {
            _watcher = new FileSystemWatcher(FilePathSource, FileNameTemplate)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            _watcher.Created += UploadPackagesArg;
            _watcher.Deleted += UploadPackagesArg;
            _watcher.Renamed += UploadPackagesArg;
        }

        private void DeinitWatcher()
        {
            if (_watcher == null)
                return;

            _watcher.Created -= UploadPackagesArg;
            _watcher.Deleted -= UploadPackagesArg;
            _watcher.Renamed -= UploadPackagesArg;
            _watcher.Dispose();
        }

        private void UploadPackagesArg(object o = null, object e = null)
        {
            var selectedFile = FileNameSource; //Restore previous value after package upload

            UploadPackages();

            FileNameSource = Packages.Any(u => u.FileName == selectedFile) ? selectedFile : Packages.FirstOrDefault()?.FileName;
        }

        private void UploadPackages()
        {
            try
            {
                Packages = Directory.GetFiles(FilePathSource, FileNameTemplate).Select(u => PackageIdentity.CreateInvalid(new FileInfo(u)));
                NotifyOfPropertyChange(nameof(Packages));
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
