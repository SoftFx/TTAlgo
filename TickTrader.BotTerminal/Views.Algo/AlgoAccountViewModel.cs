using Caliburn.Micro;
using Machinarium.Qnil;
using System.Linq;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class AlgoAccountViewModel : PropertyChangedBase, IDropHandler
    {
        public AccountModelInfo Info { get; }

        public AccountKey Key => Info.Key;

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

        public string DisplayName { get; }

        public IObservableList<AlgoBotViewModel> Bots { get; }

        public bool CanChangeAccount => Agent.Model.AccessManager.CanChangeAccount();

        public bool CanRemoveAccount => !HasRunningBots && Agent.Model.AccessManager.CanRemoveAccount();

        public bool CanTestAccount => Agent.Model.AccessManager.CanTestAccount();

        public bool CanAddBot => Agent.Model.AccessManager.CanAddBot();

        public bool HasRunningBots => Bots.Any(b => b.IsRunning);


        public AlgoAccountViewModel(AccountModelInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            DisplayName = $"{Info.Key.Server} - {Info.Key.Login}";
            Bots = Agent.Bots.Where(b => BotIsAttachedToAccount(b)).AsObservable();

            Agent.Model.AccountStateChanged += OnAccountStateChanged;
            Agent.Model.BotStateChanged += OnBotStateChanged;
            Agent.Model.AccessLevelChanged += OnAccessLevelChanged;
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

        public void AddBot()
        {
            Agent.OpenBotSetup(Info.Key);
        }


        private void OnAccountStateChanged(AccountModelInfo account)
        {
            if (Info.Key.Equals(account.Key))
            {
                NotifyOfPropertyChange(nameof(Status));
            }
        }

        private bool BotIsAttachedToAccount(AlgoBotViewModel bot)
        {
            return Info.Key.Equals(bot.Account);
        }

        private void OnBotStateChanged(ITradeBot bot)
        {
            if (Info.Key.Equals(bot.Account))
            {
                NotifyOfPropertyChange(nameof(HasRunningBots));
                NotifyOfPropertyChange(nameof(CanRemoveAccount));
            }
        }

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanChangeAccount));
            NotifyOfPropertyChange(nameof(CanRemoveAccount));
            NotifyOfPropertyChange(nameof(CanTestAccount));
            NotifyOfPropertyChange(nameof(CanAddBot));
        }

        public void Drop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null)
            {
                Agent.OpenBotSetup(Info.Key, algoBot.Info);
            }
        }

        public bool CanDrop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null && algoBot.Agent.Name == Agent.Name && algoBot.Type == AlgoTypes.Robot)
            {
                return true;
            }
            return false;
        }
    }
}
