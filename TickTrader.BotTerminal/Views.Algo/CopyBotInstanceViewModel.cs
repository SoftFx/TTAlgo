using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Linq;
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

        public IObservableList<AlgoAgentViewModel> Agents { get; }

        public AlgoAgentViewModel SelectedAgent
        {
            get { return _selectedAgent; }
            set
            {
                if (_selectedAgent == value)
                    return;

                DeinitAlgoAgent(SelectedAgent);
                _selectedAgent = value;
                InitAlgoAgent(SelectedAgent);
                NotifyOfPropertyChange(nameof(SelectedAgent));
                NotifyOfPropertyChange(nameof(IsInstanceIdValid));
                Validate();
            }
        }

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

        public bool IsInstanceIdValid => SelectedAgent != null && !string.IsNullOrEmpty(InstanceId) && SelectedAgent.Model.IdProvider.IsValidPluginId(AlgoTypes.Robot, InstanceId);

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
            && SelectedAgent.Model.AccessManager.CanUploadPackage()
            && SelectedAgent.Model.AccessManager.CanAddBot()
            && SelectedAgent.Model.AccessManager.CanUploadBotFile();

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

            Agents = _algoEnv.Agents.AsObservable();
            SelectedAgent = Agents.FirstOrDefault();
            InstanceId = botId;
            From = $"{agentName}/{botId}";

            _fromBotId = botId;
            _fromAgent = Agents.First(a => a.Name == agentName);
            _fromBotRemoved = _fromAgent.Model.Bots.Snapshot.ContainsKey(_fromBotId);
            _fromAgent.Model.Bots.Updated += FromAgentOnBotsUpdated;

            CopyProgress = new ProgressViewModel();
        }


        public async void Ok()
        {
            HasPendingRequest = true;
            try
            {
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
            DeinitAlgoAgent(SelectedAgent);
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
                Accounts = agent.Accounts.AsObservable();
                SelectedAccount = Accounts.FirstOrDefault();

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
    }
}
