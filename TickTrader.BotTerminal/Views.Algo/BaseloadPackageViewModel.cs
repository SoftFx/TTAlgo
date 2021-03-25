using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.ServerControl;

namespace TickTrader.BotTerminal
{
    internal enum LoadPackageMode
    {
        Upload,
        Download,
    }

    internal abstract class BaseloadPackageViewModel : Screen, IWindowModel
    {
        protected static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        protected const string FileNameWatcherTemplate = "*.ttalgo";

        protected readonly AlgoEnvironment _algoEnv;

        private readonly LoadPackageMode _mode;

        private FileSystemWatcher _watcher;
        private string _selectedFolder, _fileNameSource, _fileNameTarget, _error;
        private bool _isEnabled = true;


        public ProgressViewModel ProgressModel { get; }

        public AlgoAgentViewModel SelectedBotAgent { get; }

        public IEnumerable<AlgoPackageViewModel> Packages { get; protected set; }


        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder == value)
                    return;

                DeinitWatcher();

                _selectedFolder = value;

                InitWatcher();
                UploadSelectedSource();

                NotifyOfPropertyChange(nameof(SelectedFolder));
            }
        }

        public string FileNameSource
        {
            get => _fileNameSource;
            set
            {
                if (_fileNameSource == value)
                    return;

                _fileNameSource = value;
                FileNameTarget = GenerateTargetFileName(value);

                NotifyOfPropertyChange(nameof(FileNameSource));
            }
        }

        public string FileNameTarget
        {
            get => _fileNameTarget;
            set
            {
                if (_fileNameTarget == value)
                    return;

                _fileNameTarget = value;

                NotifyOfPropertyChange(nameof(FileNameTarget));
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

        public bool HasError => !string.IsNullOrEmpty(_error);


        public BaseloadPackageViewModel(AlgoEnvironment algoEnv, string agentName, LoadPackageMode mode)
        {
            _algoEnv = algoEnv;
            _mode = mode;
            //_selectedFolder = EnvService.Instance.AlgoRepositoryFolder;

            DisplayName = $"{mode} Algo package";
            ProgressModel = new ProgressViewModel();

            SelectedBotAgent = _algoEnv.BotAgents.Where(b => b.Agent.Name == agentName).AsObservable().FirstOrDefault()?.Agent;
            SelectedBotAgent.Packages.Updated += UpdateAgentPackage;
        }

        public async void Ok()
        {
            IsEnabled = false;

            try
            {
                if (FileNameSource == null)
                    throw new Exception("Please select a Algo package.");

                var message = $"{_mode}ing Algo package {FileNameTarget} {(_mode == LoadPackageMode.Upload ? $"to {SelectedBotAgent.Name}" : $"from {SelectedBotAgent.Name} to {SelectedFolder}")}";

                ProgressModel.SetMessage(message);

                var selectPackageInfo = Packages.FirstOrDefault(u => u.FileName == FileNameSource);
                var progressListener = new FileProgressListenerAdapter(ProgressModel, selectPackageInfo.Identity.Size);

                if (_mode == LoadPackageMode.Upload)
                    await SelectedBotAgent.Model.UploadPackage(FileNameTarget, FullSelectePackagePath(FileNameSource), progressListener);
                else
                {
                    var package = SelectedBotAgent.PackageList.FirstOrDefault(u => u.DisplayName == FileNameSource).Key;
                    await SelectedBotAgent.Model.DownloadPackage(package, FullSelectePackagePath(FileNameTarget), progressListener);
                }

                TryClose();
            }
            catch (ConnectionFailedException ex)
            {
                Error = $"Error connecting to agent: {SelectedBotAgent.Name}";
                _logger.Error(ex, Error);
            }
            catch (Exception ex)
            {
                Error = $"{ex.Message} Failed to {_mode} Algo package";
                _logger.Error(ex, Error);
            }

            IsEnabled = true;
        }

        public void Cancel() => TryClose();


        protected virtual void UploadSelectedSource() { }

        protected virtual void WatcherEventHandling(object o, object e) { }

        protected virtual void UpdateAgentPackage(ListUpdateArgs<AlgoPackageViewModel> args) { }

        protected virtual bool CheckFileNameTarget(string name) => true;

        protected override void OnDeactivate(bool close)
        {
            DeinitWatcher();
            SelectedBotAgent.Packages.Updated -= UpdateAgentPackage;

            base.OnDeactivate(close);
        }

        protected void SetDefaultFileSource(string packageId, IObservableList<AlgoPackageViewModel> packages, out AlgoPackageViewModel package)
        {
            //if packageId != null - trying to find a similar package on the local server (a specific package from a Remote AlgoServer)
            package = packages?.FirstOrDefault(u => u.Key == packageId) ?? packages.FirstOrDefault();

            FileNameSource = package?.FileName;
        }

        protected void RefreshTargetName() => FileNameTarget = GenerateTargetFileName(FileNameSource);


        private string GenerateTargetFileName(string name) //File name generation as in Windows (a.ttalgo, a - Copy.ttalgo, a - Copy (2).ttalgo)
        {
            if (name == null)
                return null;

            var ext = Path.GetExtension(name);
            name = Path.GetFileNameWithoutExtension(name);
            var step = 0;

            do
            {
                var fileName = $"{name}{(step > 0 ? " - Copy" : "")}{(step > 1 ? $" ({step})" : "")}{ext}";

                if (CheckFileNameTarget(fileName))
                    return fileName;
            }
            while (++step < int.MaxValue);

            return $"{name} - Copy ({DateTime.Now.Ticks}){ext}";
        }

        private void InitWatcher()
        {
            _watcher = new FileSystemWatcher(SelectedFolder, FileNameWatcherTemplate)
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            _watcher.Created += WatcherEventHandling;
            _watcher.Deleted += WatcherEventHandling;
            _watcher.Renamed += WatcherEventHandling;
        }

        private void DeinitWatcher()
        {
            if (_watcher == null)
                return;

            _watcher.Created -= WatcherEventHandling;
            _watcher.Deleted -= WatcherEventHandling;
            _watcher.Renamed -= WatcherEventHandling;
            _watcher.Dispose();
        }

        private string FullSelectePackagePath(string fileName) => Path.Combine(SelectedFolder, fileName);
    }
}
