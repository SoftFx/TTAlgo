using Caliburn.Micro;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class AlgoBotViewModel : PropertyChangedBase
    {
        public ITradeBot Model { get; }

        public AlgoAgentViewModel Agent { get; }


        public string InstanceId => Model.InstanceId;

        public AccountKey Account => Model.Account;

        public PluginStates State => Model.State;

        public bool IsRunning => PluginStateHelper.IsRunning(Model.State);

        public bool IsStopped => PluginStateHelper.IsStopped(Model.State);

        public bool CanStart => PluginStateHelper.IsStopped(Model.State);

        public bool CanStop => PluginStateHelper.IsRunning(Model.State);

        public bool CanStartStop => CanStart || CanStop;

        public bool CanRemove => PluginStateHelper.IsStopped(Model.State);

        public bool CanOpenChart => !Model.IsRemote && (Model.Descriptor?.SetupMainSymbol ?? false);

        public string Status => Model.Status;


        public AlgoBotViewModel(ITradeBot bot, AlgoAgentViewModel agent)
        {
            Model = bot;
            Agent = agent;

            Model.StateChanged += OnStateChanged;
            Model.ConfigurationChanged += OnConfigurationChanged;
            Model.StatusChanged += OnStatusChanged;
        }


        public void Start()
        {
            Agent.StartBot(InstanceId).Forget();
        }

        public void Stop()
        {
            Agent.StopBot(InstanceId).Forget();
        }

        public void StartStop()
        {
            if (IsRunning)
                Stop();
            else if (IsStopped)
                Start();
        }

        public void Remove()
        {
            Agent.RemoveBot(InstanceId).Forget();
        }

        public void OpenSettings()
        {
            Agent.OpenBotSetup(Model);
        }

        public void OpenChart()
        {
            Agent.ShowChart(Model);
        }

        public void OpenState()
        {
            Agent.OpenBotState(this);
        }


        private void OnStateChanged(ITradeBot bot)
        {
            NotifyOfPropertyChange(nameof(State));
            NotifyOfPropertyChange(nameof(IsRunning));
            NotifyOfPropertyChange(nameof(IsStopped));
            NotifyOfPropertyChange(nameof(CanStart));
            NotifyOfPropertyChange(nameof(CanStop));
            NotifyOfPropertyChange(nameof(CanStartStop));
            NotifyOfPropertyChange(nameof(CanRemove));
        }

        private void OnConfigurationChanged(ITradeBot bot)
        {
        }

        private void OnStatusChanged(ITradeBot bot)
        {
            NotifyOfPropertyChange(nameof(Status));
        }
    }
}
