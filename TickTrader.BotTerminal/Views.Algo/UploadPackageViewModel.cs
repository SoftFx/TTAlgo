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
        private readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoEnvironment _algoEnv;
        private AlgoAgentViewModel _selectedBotAgent;
        private string _fileNameSource;
        private string _fileName;
        private string _filePath;
        private bool _overwriteFile;
        private bool _isValid;
        private bool _hasPendingRequest;
        private string _error;


        public IEnumerable<string> PackageNames => Packages.Select(u => u.FileName);

        public IEnumerable<PackageIdentity> Packages { get; private set; }

        public IObservableList<AlgoAgentViewModel> BotAgents { get; }


        public AlgoAgentViewModel SelectedBotAgent
        {
            get { return _selectedBotAgent; }
            set
            {
                if (_selectedBotAgent == value)
                    return;

                DeinitAlgoAgent(_selectedBotAgent);
                _selectedBotAgent = value;
                InitAlgoAgent(_selectedBotAgent);
                NotifyOfPropertyChange(nameof(SelectedBotAgent));
                Validate();
            }
        }

        public string FileNameSource
        {
            get { return _fileNameSource; }
            set
            {
                if (_fileNameSource == value)
                    return;

                _fileNameSource = value;
                _fileName = value;
                NotifyOfPropertyChange(nameof(FileNameSource));
                NotifyOfPropertyChange(nameof(FileName));
                Validate();
            }
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                if (_fileName == value)
                    return;

                _fileName = value;
                NotifyOfPropertyChange(nameof(FileName));
                Validate();
            }
        }


        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath == value)
                    return;

                _filePath = value;
                FindPackages();
                NotifyOfPropertyChange(nameof(FilePath));
            }
        }

        public bool OverwriteFile
        {
            get { return _overwriteFile; }
            set
            {
                if (_overwriteFile == value)
                    return;

                _overwriteFile = value;
                NotifyOfPropertyChange(nameof(OverwriteFile));
                Validate();
            }
        }

        public bool HasPendingRequest
        {
            get { return _hasPendingRequest; }
            set
            {
                if (_hasPendingRequest == value)
                    return;

                _hasPendingRequest = value;
                NotifyOfPropertyChange(nameof(HasPendingRequest));
                NotifyOfPropertyChange(nameof(IsEnabled));
                NotifyOfPropertyChange(nameof(CanOk));
            }
        }

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                if (_isValid == value)
                    return;

                _isValid = value;
                NotifyOfPropertyChange(nameof(IsValid));
                NotifyOfPropertyChange(nameof(CanOk));
            }
        }

        public string Error
        {
            get { return _error; }
            set
            {
                if (_error == value)
                    return;

                _error = value;
                UpdateErrors();
            }
        }

        public bool CanOk => !_hasPendingRequest && _isValid;

        public bool IsEnabled => !_hasPendingRequest;

        public bool HasError => !string.IsNullOrEmpty(_error);

        public string FullPackagePath => Path.Combine(FilePath, FileNameSource);

        public bool HasFileError { get; set; }

        public ProgressViewModel UploadProgress { get; }


        private UploadPackageViewModel(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;

            DisplayName = "Upload package";

            Packages = _algoEnv.LocalAgentVM.Packages.Where(u => u.Location == RepositoryLocation.LocalRepository ||
                                                            u.Location == RepositoryLocation.CommonRepository).
                                                            Select(u => u.Identity).AsObservable();

            BotAgents = _algoEnv.BotAgents.Select(b => b.Agent).AsObservable();

            UploadProgress = new ProgressViewModel();
            InitAlgoAgent(algoEnv.LocalAgentVM);
        }

        public UploadPackageViewModel(AlgoEnvironment algoEnv, string agentName) : this(algoEnv)
        {
            UpdateFilePaths(Packages.First() ?? null);
            SelectedBotAgent = BotAgents.FirstOrDefault(a => a.Name == agentName);
        }

        public UploadPackageViewModel(AlgoEnvironment algoEnv, PackageKey packageKey, string agentName) : this(algoEnv)
        {
            UpdateFilePaths(_algoEnv.LocalAgentVM.Packages.AsObservable().FirstOrDefault(p => agentName == LocalAlgoAgent.LocalAgentName ? p.Key.Equals(packageKey) : p.Key.Name == packageKey.Name)?.Identity ?? null);
            SelectedBotAgent = BotAgents.FirstOrDefault(a => agentName == LocalAlgoAgent.LocalAgentName ? true : a.Name == agentName);
        }

        public async void Ok()
        {
            HasPendingRequest = true;

            try
            {
                UploadProgress.SetMessage($"Uploading package {FileName} to {SelectedBotAgent.Name}");

                var fileIdentity = Packages.FirstOrDefault(u => u.FileName == FileNameSource);
                var progressListener = new FileProgressListenerAdapter(UploadProgress, fileIdentity.Size);

                await SelectedBotAgent.Model.UploadPackage(FileName, FullPackagePath, progressListener);

                TryClose();
            }
            catch (ConnectionFailedException)
            {
                Error = GetErrorConnectionMessage();
                _logger.Error(Error);
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                _logger.Error(ex, "Failed to upload package");
            }

            HasPendingRequest = false;
        }

        public void Cancel()
        {
            TryClose();
        }

        protected override void OnDeactivate(bool close)
        {
            DeinitAlgoAgent(SelectedBotAgent);

            base.OnDeactivate(close);
        }

        private void UpdateFilePaths(PackageIdentity identity)
        {
            if (identity == null)
            {
                FilePath = EnvService.Instance.AlgoRepositoryFolder;
                return;
            }

            FilePath = Path.GetDirectoryName(identity.FilePath);
            FileNameSource = identity.FileName;
        }

        private void Validate()
        {
            Error = null;
            HasFileError = false;

            if (SelectedBotAgent == null)
            {
                IsValid = false;
                return;
            }

            if (!ValidateFileName(FileName))
            {
                HasFileError = true;
                IsValid = false;
                UpdateErrors();
                return;
            }

            if (SelectedBotAgent != null)
            {
                var targetPackage = SelectedBotAgent.PackageList.FirstOrDefault(p => p.Identity.FileName == FileName);
                if (!OverwriteFile && targetPackage != null)
                {
                    Error = $"Package with name '{FileName}' already exists";
                    IsValid = false;
                    return;
                }
                if (targetPackage != null && targetPackage.IsLocked)
                {
                    Error = $"Package with name '{FileName}' is locked. Stop all running bots first";
                    IsValid = false;
                    return;
                }
            }

            IsValid = true;
            UpdateErrors();
        }

        private void InitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.Packages.Updated += BotAgentPackagesUpdated;
                agent.Model.PackageStateChanged += BotAgentPackageStateChanged;
            }
        }

        private void DeinitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.Packages.Updated -= BotAgentPackagesUpdated;
                agent.Model.PackageStateChanged -= BotAgentPackageStateChanged;
            }
        }

        private bool ValidateFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                Error = "File name is required";
            else
            if (!PackageWatcher.IsFileSupported(name))
                Error = "File extension is not supported. Supported extensions are .ttalgo and .dll";
            else
            {
                var incorrectSymbols = Path.GetInvalidFileNameChars();

                if (!name.All(s => !incorrectSymbols.Contains(s)))
                    Error = "File name contains invalid characters";
            }

            return string.IsNullOrEmpty(Error);
        }

        private void UpdateErrors()
        {
            NotifyOfPropertyChange(nameof(Error));
            NotifyOfPropertyChange(nameof(HasError));
            NotifyOfPropertyChange(nameof(HasFileError));
        }

        private string GetErrorConnectionMessage() => $"Error connecting to agent: {SelectedBotAgent.Name}";

        private void BotAgentPackagesUpdated(DictionaryUpdateArgs<PackageKey, PackageInfo> args) => TriggerValidate(args.Key.Name);

        private void BotAgentPackageStateChanged(PackageInfo package) => TriggerValidate(package.Key.Name);

        private void TriggerValidate(string name)
        {
            if (name == FileNameSource?.ToLowerInvariant())
            {
                FindPackages();
                Validate();
            }
        }

        private void FindPackages()
        {
            Packages = Directory.GetFiles(FilePath).Where(u => Path.GetExtension(u) == ".ttalgo").Select(u => PackageIdentity.Create(new FileInfo(u)));
            NotifyOfPropertyChange(nameof(PackageNames));
        }
    }
}
