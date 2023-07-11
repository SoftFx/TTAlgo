﻿using Caliburn.Micro;
using System;

namespace TickTrader.BotTerminal
{
    internal interface IAgentCmdProvider : IDropHandler
    {
        bool CanChangeBotAgent { get; }
        bool CanRemoveBotAgent { get; }
        bool CanConnectBotAgent { get; }
        bool CanDisconnectBotAgent { get; }
        bool CanAddBot { get; }
        bool CanAddAccount { get; }
        bool CanUploadPackage { get; }
        bool CanDownloadPackage { get; }
        bool CanManageFiles { get; }
        bool CanOpenUpdate { get; }

        void ChangeBotAgent();
        void RemoveBotAgent();
        void ConnectBotAgent();
        void DisconnectBotAgent();
        void AddAccount();
        void AddBot();
        void UploadPackage();
        void DownloadPackage();
        void ManageFiles();
        void OpenUpdate();
    }


    internal class BotAgentViewModel : PropertyChangedBase, IAgentCmdProvider, IDisposable
    {
        private AlgoEnvironment _algoEnv;


        public BotAgentConnectionManager Connection { get; }

        public string Status => Connection.Status;

        public AlgoAgentViewModel Agent { get; }

        public string DisplayName =>
            Connection.State == BotAgentConnectionManager.States.Online ? $"{Connection.Creds.Name} - {Connection.AccessLevel}" : $"{Connection.Creds.Name}";

        public string ToolTipInformation => $"{Connection.Creds.ServerAddress}:{Connection.Creds.Port}, status = {Status}";

        public bool CanChangeBotAgent => true;

        public bool CanRemoveBotAgent => true;

        public bool CanConnectBotAgent => Connection.State == BotAgentConnectionManager.States.Offline || Connection.State == BotAgentConnectionManager.States.WaitReconnect;

        public bool CanDisconnectBotAgent => Connection.State == BotAgentConnectionManager.States.Connecting || Connection.State == BotAgentConnectionManager.States.Online || Connection.State == BotAgentConnectionManager.States.WaitReconnect;

        public bool CanAddBot => Agent.Model.AccessManager.CanAddPlugin();

        public bool CanAddAccount => Agent.Model.AccessManager.CanAddAccount();

        public bool CanUploadPackage => Agent.Model.AccessManager.CanUploadPackage();

        public bool CanDownloadPackage => Agent.Model.AccessManager.CanDownloadPackage();

        public bool CanManageFiles => Agent.Model.AccessManager.CanGetBotFolderInfo(Algo.Server.PublicAPI.PluginFolderInfo.Types.PluginFolderId.AlgoData)
                || Agent.Model.AccessManager.CanGetBotFolderInfo(Algo.Server.PublicAPI.PluginFolderInfo.Types.PluginFolderId.BotLogs);

        public bool CanOpenUpdate => Connection.State == BotAgentConnectionManager.States.Online;


        public BotAgentViewModel(BotAgentConnectionManager connection, AlgoEnvironment algoEnv)
        {
            Connection = connection;
            _algoEnv = algoEnv;

            Agent = new AlgoAgentViewModel(Connection.RemoteAgent, _algoEnv);

            Connection.StateChanged += ConnectionOnStateChanged;
            Agent.Model.AccessLevelChanged += OnAccessLevelChanged;
        }


        public void Dispose()
        {
            Connection.StateChanged -= ConnectionOnStateChanged;
            Agent.Model.AccessLevelChanged -= OnAccessLevelChanged;
        }


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

        public void ChangeBotAgent()
        {
            var viewModel = new BotAgentLoginDialogViewModel(_algoEnv.BotAgentManager, Connection.Creds);
            _algoEnv.Shell.ToolWndManager.ShowDialog(viewModel);
        }

        public void RemoveBotAgent()
        {
            var result = _algoEnv.Shell.ShowDialog(DialogButton.YesNo, DialogMode.Question, DialogMessages.GetRemoveTitle("server"), DialogMessages.GetRemoveMessage("server"));

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

        public void OpenUpdate()
        {
            _algoEnv.Shell.OpenUpdate(this);
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
            NotifyOfPropertyChange(nameof(ToolTipInformation));
            NotifyOfPropertyChange(nameof(CanOpenUpdate));
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
