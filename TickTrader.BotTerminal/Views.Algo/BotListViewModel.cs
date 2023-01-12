using Caliburn.Micro;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal sealed class BotListViewModel : IDropHandler
    {
        private readonly AlgoEnvironment _algoEnv;


        public LocalAgentCmdWrapper LocalAgent { get; }

        public IObservableList<BotAgentViewModel> BotAgents { get; }


        public BotListViewModel(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;

            LocalAgent = new LocalAgentCmdWrapper(_algoEnv.LocalAgentVM);
            BotAgents = _algoEnv.BotAgents.AsObservable();
        }


        public void AddBotAgent()
        {
            var viewModel = new BotAgentLoginDialogViewModel(_algoEnv.BotAgentManager);
            _algoEnv.Shell.ToolWndManager.ShowDialog(viewModel);
        }

        public void Drop(object o)
        {
            if (o is AlgoPluginViewModel algoBot)
                _algoEnv.LocalAgentVM.OpenBotSetup(null, algoBot.Key);
        }

        public bool CanDrop(object o) => o is AlgoPluginViewModel algoBot && algoBot.Agent.Name == _algoEnv.LocalAgentVM.Name && algoBot.IsTradeBot;


        internal class LocalAgentCmdWrapper : PropertyChangedBase, IAgentCmdProvider, IDropHandler
        {
            public AlgoAgentViewModel Agent { get; }


            public LocalAgentCmdWrapper(AlgoAgentViewModel agent)
            {
                Agent = agent;
            }


            #region IAgentWrapper implementation

            public bool CanAddBot => Agent.Model.AccessManager.CanAddPlugin();

            public bool CanAddAccount => Agent.Model.AccessManager.CanAddAccount();

            public bool CanUploadPackage => Agent.Model.AccessManager.CanUploadPackage();

            public bool CanDownloadPackage => Agent.Model.AccessManager.CanDownloadPackage();

            public bool CanManageFiles => Agent.Model.AccessManager.CanGetBotFolderInfo(Algo.Server.PublicAPI.PluginFolderInfo.Types.PluginFolderId.AlgoData)
                || Agent.Model.AccessManager.CanGetBotFolderInfo(Algo.Server.PublicAPI.PluginFolderInfo.Types.PluginFolderId.BotLogs);

            public virtual bool CanChangeBotAgent => false;

            public virtual bool CanRemoveBotAgent => false;

            public virtual bool CanConnectBotAgent => false;

            public virtual bool CanDisconnectBotAgent => false;


            public void Drop(object o)
            {
                if (o is AlgoPluginViewModel algoBot)
                    Agent.OpenBotSetup(null, algoBot.Key);

                if (o is AlgoPackageViewModel algoPackage)
                    Agent.OpenUploadPackageDialog(algoPackage.Key, Agent);
            }

            public bool CanDrop(object o)
            {
                if (o is AlgoPluginViewModel algoBot && algoBot.Agent.Name == Agent.Name && algoBot.IsTradeBot)
                    return true;

                if (o is AlgoPackageViewModel algoPackage && algoPackage.CanUploadPackage && CanUploadPackage)
                    return true;

                return false;
            }

            public void AddAccount() => Agent.OpenAccountSetup(null);

            public void AddBot() => Agent.OpenBotSetup();

            public void UploadPackage() => Agent.OpenUploadPackageDialog();

            public void DownloadPackage() => Agent.OpenDownloadPackageDialog();

            public void ManageFiles() => Agent.OpenManageBotFilesDialog();


            public void ChangeBotAgent() => throw new System.NotImplementedException();

            public void RemoveBotAgent() => throw new System.NotImplementedException();

            public void ConnectBotAgent() => throw new System.NotImplementedException();

            public void DisconnectBotAgent() => throw new System.NotImplementedException();

            #endregion
        }
    }
}
