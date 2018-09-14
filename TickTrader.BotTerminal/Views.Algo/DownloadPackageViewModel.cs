using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.IO;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal class DownloadPackageViewModel : Screen, IWindowModel
    {
        private AlgoEnvironment _algoEnv;
        private AlgoPackageViewModel _selectedPackage;
        private AlgoAgentViewModel _selectedBotAgent;
        private string _fileName;
        private string _filePath;
        private bool _isValid;
        private bool _hasPendingRequest;
        private string _error;


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
                NotifyOfPropertyChange(nameof(Packages));
                Validate();
            }
        }

        public IObservableList<AlgoPackageViewModel> Packages { get; private set; }

        public AlgoPackageViewModel SelectedPackage
        {
            get { return _selectedPackage; }
            set
            {
                if (_selectedPackage == value)
                    return;

                _selectedPackage = value;
                NotifyOfPropertyChange(nameof(SelectedPackage));
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
            }
        }

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (_filePath == value)
                    return;

                _filePath = value;
                NotifyOfPropertyChange(nameof(FilePath));
                FileName = Path.GetFileName(_filePath);
                Validate();
            }
        }

        public string FileFilter => "Algo Packages (*.ttalgo)|*.ttalgo|Dll Files (*.dll)|*.dll|All files (*.*)|*.*";

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


        private DownloadPackageViewModel(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;

            DisplayName = "Download package";

            BotAgents = _algoEnv.BotAgents.Select(b => b.Agent).AsObservable();
        }

        public DownloadPackageViewModel(AlgoEnvironment algoEnv, string agentName)
            : this(algoEnv)
        {
            SelectedBotAgent = BotAgents.FirstOrDefault(a => a.Name == agentName);
        }

        public DownloadPackageViewModel(AlgoEnvironment algoEnv, PackageKey packageKey, string agentName)
            : this(algoEnv)
        {
            SelectedBotAgent = BotAgents.FirstOrDefault(a => a.Name == agentName);
            SelectedPackage = Packages.FirstOrDefault(p => p.Key.Equals(packageKey)) ?? Packages.FirstOrDefault();
        }


        public async void Ok()
        {
            HasPendingRequest = true;
            try
            {
                await SelectedBotAgent.Model.DownloadPackage(SelectedPackage.Key, FilePath);
                TryClose();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
            HasPendingRequest = false;
        }

        public void Cancel()
        {
            TryClose();
        }


        public override void TryClose(bool? dialogResult = null)
        {
            DeinitAlgoAgent(SelectedBotAgent);

            base.TryClose(dialogResult);
        }


        private void Validate()
        {
            if (SelectedBotAgent == null || SelectedPackage == null || FilePath == null)
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

            Error = null;
            IsValid = true;
        }

        private void InitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                Packages = agent.Packages.Where(p => p.Location == RepositoryLocation.LocalRepository).AsObservable();
                SelectedPackage = Packages.FirstOrDefault();

                agent.Model.Packages.Updated += BotAgentPackagesUpdated;
            }
        }

        private void DeinitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.Packages.Updated -= BotAgentPackagesUpdated;
            }
        }

        private void BotAgentPackagesUpdated(DictionaryUpdateArgs<PackageKey, PackageInfo> args)
        {
            if (args.Key.Equals(SelectedPackage?.Key))
            {
                SelectedPackage = null;
            }
        }
    }
}
