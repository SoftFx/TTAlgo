using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class CopyBotInstanceViewModel : Screen, IWindowModel
    {
        private readonly static ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private AlgoEnvironment _algoEnv;
        private AlgoAgentViewModel _selectedAgent;
        private AlgoAccountViewModel _selectedAccount;
        private string _instanceId;
        private bool _isValid;
        private bool _hasPendingRequest;
        private string _error;
        private AlgoAgentViewModel _fromAgent;
        private string _fromBotId;
        private bool _fromBotRemoved;


        public string From { get; }

        public IObservableList<AlgoAccountViewModel> Accounts { get; private set; }

        public AlgoAccountViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                if (_selectedAccount == value)
                    return;

                _selectedAccount = value;
                NotifyOfPropertyChange(nameof(SelectedAccount));
                if (_selectedAccount != null && _selectedAgent != _selectedAccount.Agent)
                {
                    DeinitAlgoAgent(_selectedAgent);
                    _selectedAgent = _selectedAccount.Agent;
                    InitAlgoAgent(_selectedAgent);
                    NotifyOfPropertyChange(nameof(IsInstanceIdValid));
                }
                Validate();
            }
        }

        public string InstanceId
        {
            get { return _instanceId; }
            set
            {
                if (_instanceId == value)
                    return;

                _instanceId = value;
                NotifyOfPropertyChange(nameof(InstanceId));
                NotifyOfPropertyChange(nameof(IsInstanceIdValid));
                Validate();
            }
        }

        public bool IsInstanceIdValid => _selectedAgent != null && !string.IsNullOrEmpty(InstanceId) && _selectedAgent.Model.IdProvider.IsValidPluginId(AlgoTypes.Robot, InstanceId);

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

        public bool CanOk => !_hasPendingRequest && _isValid
            && _selectedAgent.Model.AccessManager.CanUploadPackage()
            && _selectedAgent.Model.AccessManager.CanAddBot()
            && _selectedAgent.Model.AccessManager.CanUploadBotFile();

        public bool HasError => !string.IsNullOrEmpty(_error);

        public string Error
        {
            get { return _error; }
            set
            {
                if (_error == value)
                    return;

                _error = value;
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }

        public ProgressViewModel CopyProgress { get; }


        public CopyBotInstanceViewModel(AlgoEnvironment algoEnv, string agentName, string botId)
        {
            _algoEnv = algoEnv;

            DisplayName = "Copy Bot Instance";

            Accounts = _algoEnv.Agents.SelectMany(a => a.Accounts).AsObservable();
            SelectedAccount = Accounts.FirstOrDefault();
            InstanceId = botId;
            From = $"{agentName}/{botId}";

            _fromBotId = botId;
            _fromAgent = _algoEnv.Agents.Snapshot.First(a => a.Name == agentName);
            _fromBotRemoved = !_fromAgent.Model.Bots.Snapshot.ContainsKey(_fromBotId);
            _fromAgent.Model.Bots.Updated += FromAgentOnBotsUpdated;

            CopyProgress = new ProgressViewModel();
        }


        public async void Ok()
        {
            HasPendingRequest = true;
            try
            {
                _logger.Info($"Copying bot from '{From}' to '{_selectedAgent.Name}/{_selectedAccount.DisplayName}/{InstanceId}'");

                if (!_fromAgent.Model.Bots.Snapshot.TryGetValue(_fromBotId, out var srcBot))
                    throw new ArgumentException("Can't find bot to copy");

                var dstConfig = srcBot.Config.Clone();

                await ResolvePackage(srcBot, dstConfig);

                dstConfig.InstanceId = InstanceId;
                await _selectedAgent.AddBot(_selectedAccount.Key, dstConfig);

                await ResolveBotFiles(srcBot, dstConfig);

                //CopyProgress.SetMessage($"Downloading package {SelectedPackage.Key.Name} from {SelectedBotAgent.Name} to {FilePath}");
                //var progressListener = new FileProgressListenerAdapter(CopyProgress, SelectedPackage.Identity.Size);
                //await SelectedBotAgent.Model.DownloadPackage(SelectedPackage.Key, FilePath, progressListener);
                TryClose();
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                _logger.Error(ex, "Failed to copy bot instance");
            }
            HasPendingRequest = false;
        }

        public void Cancel()
        {
            TryClose();
        }


        protected override void OnDeactivate(bool close)
        {
            DeinitAlgoAgent(_selectedAgent);
            _fromAgent.Model.Bots.Updated -= FromAgentOnBotsUpdated;

            base.OnDeactivate(close);
        }


        private void Validate()
        {
            IsValid = !_fromBotRemoved && SelectedAccount != null && IsInstanceIdValid;
        }

        private void InitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.AccessLevelChanged += AgentOnAccessLevelChanged;
                agent.Model.Bots.Updated += AgentOnBotsUpdated;
            }
        }

        private void DeinitAlgoAgent(AlgoAgentViewModel agent)
        {
            if (agent != null)
            {
                agent.Model.AccessLevelChanged -= AgentOnAccessLevelChanged;
                agent.Model.Bots.Updated -= AgentOnBotsUpdated;
            }
        }

        private void AgentOnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanOk));
        }

        private void AgentOnBotsUpdated(DictionaryUpdateArgs<string, ITradeBot> args)
        {
            if (args.Action == DLinqAction.Insert || args.Action == DLinqAction.Remove)
            {
                Validate();
            }
        }

        private void FromAgentOnBotsUpdated(DictionaryUpdateArgs<string, ITradeBot> args)
        {
            if (args.Action == DLinqAction.Remove && args.Key == _fromBotId)
            {
                _fromBotRemoved = true;
                Error = "Can't copy bot that has been removed";
            }
            else if (_fromBotRemoved && args.Action == DLinqAction.Insert && args.Key == _fromBotId)
            {
                _fromBotRemoved = false;
                Error = null;
            }
        }

        private async Task ResolvePackage(ITradeBot srcBot, PluginConfig dstConfig)
        {
            if (!_fromAgent.Model.Packages.Snapshot.TryGetValue(srcBot.Config.Key.GetPackageKey(), out var srcPackage))
                throw new ArgumentException("Can't find bot package");

            var uploadSrcPackage = true;
            var dstPackageKey = dstConfig.Key.GetPackageKey();
            //TODO: CommonRepository logic
            var dstPackage = _selectedAgent.Model.Packages.Snapshot.Values.FirstOrDefault(p => p.Identity.Hash == srcPackage.Identity.Hash);
            if (dstPackage != null)
            {
                _logger.Info($"'{_selectedAgent.Name}' has matching package {dstPackage.Key.Name}({dstPackage.Key.Location})");
                _logger.Info($"Src package: {srcPackage.Identity.Hash}; Dst package: {dstPackage.Identity.Hash}");
                uploadSrcPackage = false;
            }
            else
            {
                _logger.Info($"'{_selectedAgent.Name}' has no matching package.");
            }
            if (uploadSrcPackage)
            {
                var progressListener = new FileProgressListenerAdapter(CopyProgress, srcPackage.Identity.Size);

                var dstPath = "";
                if (!_fromAgent.Model.IsRemote)
                {
                    dstPath = srcPackage.Identity.FilePath;
                }
                else
                {

                    dstPath = Path.GetTempFileName();
                    CopyProgress.SetMessage($"Downloading {srcPackage.Key.Name} from {_fromAgent.Name}");
                    await _fromAgent.Model.DownloadPackage(srcPackage.Key, dstPath, progressListener);
                }
                var dstPackageName = srcPackage.Key.Name;
                //TODO: Resolve package name conflict
                CopyProgress.SetMessage($"Uploading {dstPackageName} to {_fromAgent.Name}");
                await _selectedAgent.Model.UploadPackage(dstPackageName, dstPath, progressListener);
            }
        }

        private async Task ResolveBotFiles(ITradeBot srcBot, PluginConfig dstConfig)
        {

        }
    }
}
