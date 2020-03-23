using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentViewModel : PropertyChangedBase, IDropHandler
    {
        private AlgoEnvironment _algoEnv;


        public BotAgentConnectionManager Connection { get; }

        public string Status => Connection.Status;

        public AlgoAgentViewModel Agent { get; }

        public string DisplayName =>
            Connection.State == BotAgentConnectionManager.States.Online
            ? $"{Connection.Creds.Name} - {Connection.AccessLevel} ({Connection.Creds.ServerAddress}:{Connection.Creds.Port})"
            : $"{Connection.Creds.Name} ({Connection.Creds.ServerAddress}:{Connection.Creds.Port})";

        public bool CanConnectBotAgent => Connection.State == BotAgentConnectionManager.States.Offline || Connection.State == BotAgentConnectionManager.States.WaitReconnect;

        public bool CanDisconnectBotAgent => Connection.State == BotAgentConnectionManager.States.Connecting || Connection.State == BotAgentConnectionManager.States.Online || Connection.State == BotAgentConnectionManager.States.WaitReconnect;

        public bool CanAddBot => Agent.Model.AccessManager.CanAddBot();

        public bool CanAddAccount => Agent.Model.AccessManager.CanAddAccount();

        public bool CanUploadPackage => Agent.Model.AccessManager.CanUploadPackage();

        public bool CanDownloadPackage => Agent.Model.AccessManager.CanDownloadPackage();


        public BotAgentViewModel(BotAgentConnectionManager connection, AlgoEnvironment algoEnv)
        {
            Connection = connection;
            _algoEnv = algoEnv;

            Agent = new AlgoAgentViewModel(Connection.RemoteAgent, _algoEnv);

            Connection.StateChanged += ConnectionOnStateChanged;
            Agent.Model.AccessLevelChanged += OnAccessLevelChanged;
        }


        public void Drop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null)
            {
                Agent.OpenBotSetup(null, algoBot.PluginInfo.Key);
            }
            var algoPackage = o as AlgoPackageViewModel;
            if (algoPackage != null)
            {
                Agent.OpenUploadPackageDialog(algoPackage.Key, Agent.Name);
            }
        }

        public bool CanDrop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null && algoBot.Agent.Name == Agent.Name && algoBot.Type == AlgoTypes.Robot)
            {
                return true;
            }
            var algoPackage = o as AlgoPackageViewModel;
            if (algoPackage != null && algoPackage.CanUploadPackage && CanUploadPackage)
            {
                return true;
            }
            return false;
        }

        public void ChangeBotAgent()
        {
            var viewModel = new BotAgentLoginDialogViewModel(_algoEnv.BotAgentManager, Connection.Creds);
            _algoEnv.Shell.ToolWndManager.ShowDialog(viewModel);
        }

        public void RemoveBotAgent()
        {
            var result = _algoEnv.Shell.ShowDialog(DialogButton.YesNo, DialogMode.Question, DialogMessages.GetRemoveTitle("agent"), DialogMessages.GetRemoveMessage("agent"));

            if (result != DialogResult.OK)
                return;

            _algoEnv.BotAgentManager.Remove(Agent.Name);
        }

        public void ConnectBotAgent()
        {
            _algoEnv.BotAgentManager.Connect(Agent.Name);
        }

        public void DisconnectBotAgent()
        {
            _algoEnv.BotAgentManager.Disconnect(Agent.Name);
        }

        public void AddAccount()
        {
            Agent.OpenAccountSetup(null);
        }

        public void AddBot()
        {
            Agent.OpenBotSetup();
        }

        public void UploadPackage()
        {
            Agent.OpenUploadPackageDialog();
        }

        public void DownloadPackage()
        {
            Agent.OpenDownloadPackageDialog();
        }

        public void ManageFiles()
        {
            Agent.OpenManageBotFilesDialog();
        }


        private void ConnectionOnStateChanged()
        {
            NotifyOfPropertyChange(nameof(Status));
            NotifyOfPropertyChange(nameof(CanConnectBotAgent));
            NotifyOfPropertyChange(nameof(CanDisconnectBotAgent));
            if (Connection.State == BotAgentConnectionManager.States.Online
                || Connection.State == BotAgentConnectionManager.States.Offline
                || Connection.State == BotAgentConnectionManager.States.Connecting)
                NotifyOfPropertyChange(nameof(DisplayName));
        }

        private void OnAccessLevelChanged()
        {
            NotifyOfPropertyChange(nameof(CanAddBot));
            NotifyOfPropertyChange(nameof(CanAddAccount));
            NotifyOfPropertyChange(nameof(CanUploadPackage));
            NotifyOfPropertyChange(nameof(CanDownloadPackage));
        }
    }
}
