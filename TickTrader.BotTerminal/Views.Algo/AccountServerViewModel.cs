using Caliburn.Micro;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class AccountServerViewModel : PropertyChangedBase
    {
        private readonly AlgoAgentViewModel _agent;


        public string Server { get; }

        public IObservableList<AlgoAccountViewModel> Accounts { get; }

        public bool CanAddAccount => _agent.Model.AccessManager.CanAddAccount();


        public AccountServerViewModel(string serverAddress, AlgoAgentViewModel agent)
        {
            Server = serverAddress;
            _agent = agent;

            Accounts = _agent.Accounts.Where(a => a.Server == serverAddress).AsObservable();

            _agent.Model.AccessLevelChanged += OnAccessLevelChanged;
        }


        public void AddAccount()
        {
            _agent.OpenAccountSetup(null, Server);
        }


        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanAddAccount));
        }
    }
}
