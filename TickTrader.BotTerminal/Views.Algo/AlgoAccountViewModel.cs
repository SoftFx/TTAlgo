using Caliburn.Micro;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class AlgoAccountViewModel : PropertyChangedBase
    {
        public AccountModelInfo Info { get; }

        public AlgoAgentViewModel Agent { get; }

        public bool CanInteract => Agent.Model.SupportsAccountManagement;


        public string Login => Info.Key.Login;

        public string Server => Info.Key.Server;

        public string Status
        {
            get
            {
                if (Info.LastError.Code == ConnectionErrorCodes.None)
                    return $"{Info.ConnectionState}";
                if (Info.LastError.Code == ConnectionErrorCodes.Unknown)
                    return $"{Info.ConnectionState} - {Info.LastError.TextMessage}";
                return $"{Info.ConnectionState} - {Info.LastError.Code}";
            }
        }


        public AlgoAccountViewModel(AccountModelInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            Agent.Model.AccountStateChanged += OnAccountStateChanged;
        }


        public void ChangeAccount()
        {
            Agent.OpenAccountSetup(Info);
        }

        public void RemoveAccount()
        {
            Agent.RemoveAccount(Info.Key).Forget();
        }

        public void TestAccount()
        {
            Agent.TestAccount(Info.Key).Forget();
        }


        private void OnAccountStateChanged(AccountModelInfo account)
        {
            if (Info.Key.Equals(account.Key))
            {
                NotifyOfPropertyChange(nameof(Status));
            }
        }
    }
}
