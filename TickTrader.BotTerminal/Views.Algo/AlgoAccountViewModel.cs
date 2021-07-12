using Caliburn.Micro;
using Machinarium.Qnil;
using System.Linq;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class AlgoAccountViewModel : PropertyChangedBase, IDropHandler
    {
        public const string ServerLevelHeader = nameof(Server);

        public AccountModelInfo Info { get; }

        public string AccountId => Info.AccountId;

        public AlgoAgentViewModel Agent { get; }

        public bool CanInteract => Agent.Model.SupportsAccountManagement;


        public string Login { get; }

        public string Server { get; }

        public string Status
        {
            get
            {
                if (Info.LastError.IsOk)
                    return $"{Info.ConnectionState}";
                if (Info.LastError.Code == ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError)
                    return $"{Info.ConnectionState} - {Info.LastError.TextMessage}";
                return $"{Info.ConnectionState} - {Info.LastError.Code}";
            }
        }

        public string DisplayName { get; }

        public IObservableList<AlgoBotViewModel> Bots { get; }

        public bool CanChangeAccount => Agent.Model.AccessManager.CanChangeAccount();

        public bool CanRemoveAccount => !HasRunningBots && Agent.Model.AccessManager.CanRemoveAccount();

        public bool CanTestAccount => Agent.Model.AccessManager.CanTestAccount();

        public bool CanAddBot => Agent.Model.AccessManager.CanAddPlugin();

        public bool CanManageFiles => Agent.Model.AccessManager.CanGetBotFolderInfo(PluginFolderInfo.Types.PluginFolderId.BotLogs);

        public bool HasRunningBots => Bots.Any(b => b.IsRunning);

        public string AgentName => Agent.Name;

        public string DisplayNameWithAgent => $"{AgentName} - {DisplayName}";

        public string AccountTooltip => $"{Server} - {Login}, status = {Status}";


        public AlgoAccountViewModel(AccountModelInfo info, AlgoAgentViewModel agent)
        {
            Info = info;
            Agent = agent;

            Algo.Domain.AccountId.Unpack(info.AccountId, out var accId);
            Server = accId.Server;
            Login = accId.UserId;

            DisplayName = $"{Info.DisplayName}";
            Bots = Agent.Bots.Where(b => BotIsAttachedToAccount(b)).AsObservable();

            Agent.Model.AccountStateChanged += OnAccountStateChanged;
            Agent.Model.BotStateChanged += OnBotStateChanged;
            Agent.Model.AccessLevelChanged += OnAccessLevelChanged;
        }


        public void AddAccount()
        {
            Agent.OpenAccountSetup(null, Server);
        }

        public void ChangeAccount()
        {
            Agent.OpenAccountSetup(Info);
        }

        public void RemoveAccount()
        {
            Agent.RemoveAccount(Info.AccountId).Forget();
        }

        public void TestAccount()
        {
            Agent.TestAccount(Info.AccountId).Forget();
        }

        public void AddBot()
        {
            Agent.OpenBotSetup(Info.AccountId);
        }

        public void ManageFiles()
        {
            Agent.OpenManageBotFilesDialog();
        }


        private void OnAccountStateChanged(AccountModelInfo account)
        {
            if (Info.AccountId.Equals(account.AccountId))
            {
                NotifyOfPropertyChange(nameof(Status));
                NotifyOfPropertyChange(nameof(AccountTooltip));
            }
        }

        private bool BotIsAttachedToAccount(AlgoBotViewModel bot)
        {
            return Info.AccountId.Equals(bot.AccountId);
        }

        private void OnBotStateChanged(ITradeBot bot)
        {
            if (Info.AccountId.Equals(bot.AccountId))
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
            NotifyOfPropertyChange(nameof(CanManageFiles));
        }

        public void Drop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null)
            {
                Agent.OpenBotSetup(Info.AccountId, algoBot.Key);
            }
        }

        public bool CanDrop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null && algoBot.Agent.Name == Agent.Name && algoBot.IsTradeBot)
            {
                return true;
            }
            return false;
        }
    }
}
