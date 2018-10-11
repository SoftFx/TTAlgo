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

        public string Server => Connection.Server;

        public string DisplayName =>
            Connection.State == BotAgentConnectionManager.States.Online
            ? $"{Connection.Server}:{Connection.Port} ({Connection.AccessLevel})"
            : $"{Connection.Server}:{Connection.Port}";


        public BotAgentViewModel(BotAgentConnectionManager connection, AlgoEnvironment algoEnv)
        {
            Connection = connection;
            _algoEnv = algoEnv;

            Agent = new AlgoAgentViewModel(Connection.RemoteAgent, _algoEnv);

            Connection.StateChanged += ConnectionOnStateChanged;
        }


        public void Drop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null)
            {
                Agent.OpenBotSetup(algoBot.Info);
            }
            var algoPackage = o as AlgoPackageViewModel;
            if (algoPackage != null)
            {
                Agent.OpenUploadPackageDialog(algoPackage.Key);
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
            if (algoPackage != null && algoPackage.Agent.Name == _algoEnv.LocalAgentVM.Name)
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
            _algoEnv.BotAgentManager.Remove(Server);

        }

        public void ConnectBotAgent()
        {
            _algoEnv.BotAgentManager.Connect(Server);
        }

        public void DisconnectBotAgent()
        {
            _algoEnv.BotAgentManager.Disconnect(Server);
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
            if (Connection.State == BotAgentConnectionManager.States.Online || Connection.State == BotAgentConnectionManager.States.Offline)
                NotifyOfPropertyChange(nameof(DisplayName));
        }
    }
}
