using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
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
        private AlgoPackageViewModel _selectedPackage;
        private AlgoAgentViewModel _selectedBotAgent;
        private string _fileName;
        private bool _overwriteFile;
        private bool _isValid;
        private bool _hasPendingRequest;
        private string _error;

        public IObservableList<AlgoPackageViewModel> LocalPackages { get; }

        public AlgoPackageViewModel SelectedPackage
        {
            get { return _selectedPackage; }
            set
            {
                if (_selectedPackage == value)
                    return;

                _selectedPackage = value;
                NotifyOfPropertyChange(nameof(SelectedPackage));
                UpdateFileName();
            }
        }

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

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName == value)
                    return;

                _fileName = value;
                NotifyOfPropertyChange(nameof(FileName));
                Validate();
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

        public bool IsEnabled => !_hasPendingRequest;

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

        public bool CanOk => !_hasPendingRequest && _isValid;

        public bool HasError { get; set; }

        public string Error
        {
            get { return _error; }
            set
            {
                if (_error == value)
                    return;

                _error = value;
                HasError = !string.IsNullOrEmpty(_error);
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }

        public ProgressViewModel UploadProgress { get; }


        private UploadPackageViewModel(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;

            DisplayName = "Upload package";

            LocalPackages = _algoEnv.LocalAgentVM.Packages.Where(
                p => p.Location == RepositoryLocation.LocalRepository
                || p.Location == RepositoryLocation.CommonRepository).AsObservable();
            BotAgents = _algoEnv.BotAgents.Select(b => b.Agent).AsObservable();

            UploadProgress = new ProgressViewModel();
        }

        public UploadPackageViewModel(AlgoEnvironment algoEnv, string agentName)
            : this(algoEnv)
        {
            SelectedPackage = LocalPackages.First();
            SelectedBotAgent = BotAgents.FirstOrDefault(a => a.Name == agentName);
        }

        public UploadPackageViewModel(AlgoEnvironment algoEnv, PackageKey packageKey, string agentName)
            : this(algoEnv)
        {
            SelectedPackage = LocalPackages.FirstOrDefault(p => agentName == LocalAlgoAgent.LocalAgentName ? p.Key.Equals(packageKey) : p.Key.Name == packageKey.Name);
            SelectedBotAgent = BotAgents.FirstOrDefault(a => agentName == LocalAlgoAgent.LocalAgentName ? true : a.Name == agentName);
        }

        public async void Ok()
        {
            HasPendingRequest = true;
            try
            {
                UploadProgress.SetMessage($"Uploading package {SelectedPackage.Key.Name} from {SelectedPackage.Key.Location} to {SelectedBotAgent.Name}");
                var progressListener = new FileProgressListenerAdapter(UploadProgress, SelectedPackage.Identity.Size);
                await SelectedBotAgent.Model.UploadPackage(FileName, SelectedPackage.Identity.FilePath, progressListener);
                TryClose();
            }
            catch (ConnectionFailedException exx)
            {
                Error = GetErrorConnectionMessage();
                _logger.Error(exx, Error);
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


        private void UpdateFileName()
        {
            FileName = SelectedPackage?.Identity.FileName;
        }

        private void Validate()
        {
            if (SelectedPackage == null || SelectedBotAgent == null || string.IsNullOrEmpty(FileName))
            {
                Error = null;
                IsValid = false;
                return;
            }

            if (!PackageWatcher.IsFileSupported(FileName))
            {
                Error = "File extension is not supported. Supported extensions are .ttalgo and .dll";
                IsValid = false;
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

            Error = null;
            IsValid = true;
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

        private void BotAgentPackagesUpdated(DictionaryUpdateArgs<PackageKey, PackageInfo> args)
        {
            if (args.Key.Name == FileName?.ToLowerInvariant())
                Validate();
        }

        private void BotAgentPackageStateChanged(PackageInfo package)
        {
            if (package.Key.Name == FileName?.ToLowerInvariant())
                Validate();
        }

        private string GetErrorConnectionMessage() => $"Error connecting to agent: {SelectedBotAgent.Name}";
    }
}
