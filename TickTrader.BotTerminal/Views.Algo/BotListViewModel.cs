﻿using System;
using System.Linq;
using Caliburn.Micro;
using TickTrader.Algo.Core.Metadata;
using Machinarium.Qnil;
using NLog;

namespace TickTrader.BotTerminal
{
    internal class BotListViewModel : PropertyChangedBase, IDropHandler
    {
        private static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private BotAgentManager _botAgentManager;
        private IShell _shell;
        private BotManagerViewModel _botManager;


        public IObservableList<BotControlViewModel> LocalBots { get; }

        public IObservableList<BotAgentViewModel> BotAgents { get; }


        public BotListViewModel(IShell shell, BotAgentManager botAgentManager, BotManagerViewModel botManager)
        {
            _shell = shell;
            _botAgentManager = botAgentManager;
            _botManager = botManager;

            LocalBots = _botManager.Bots.AsObservable();

            BotAgents = _botAgentManager.BotAgents
                .OrderBy((s, b) => s)
                .Select(b => new BotAgentViewModel(b, _shell))
                .AsObservable();
        }


        public void AddBotAgent()
        {
            var viewModel = new BotAgentLoginDialogViewModel(_botAgentManager);
            _shell.ToolWndManager.ShowDialog(viewModel);
        }

        public void ChangeBotAgent(BotAgentViewModel connectionModel)
        {
            if (connectionModel != null)
            {
                var viewModel = new BotAgentLoginDialogViewModel(_botAgentManager, connectionModel.Connection.Creds);
                _shell.ToolWndManager.ShowDialog(viewModel);
            }
        }

        public void RemoveBotAgent(BotAgentViewModel connectionModel)
        {
            if (connectionModel != null)
            {
                _botAgentManager.Remove(connectionModel.Server);
            }
        }

        public void ConnectBotAgent(BotAgentViewModel connectionModel)
        {
            if (connectionModel != null)
            {
                _botAgentManager.Connect(connectionModel.Server);
            }
        }

        public void DisconnectBotAgent(BotAgentViewModel connectionModel)
        {
            if (connectionModel != null)
            {
                _botAgentManager.Disconnect(connectionModel.Server);
            }
        }

        public void Drop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null && algoBot.Type == AlgoTypes.Robot)
            {
                _botManager.OpenBotSetup(algoBot.Info);
            }
        }

        public bool CanDrop(object o)
        {
            return o is AlgoPluginViewModel;
        }

        public void AddAccount(BotAgentViewModel botAgent)
        {
            try
            {
                var model = new BAAccountDialogViewModel(botAgent.Connection.RemoteAgent, null);
                _shell.ToolWndManager.OpenMdiWindow("AccountSetupWindow", model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        public void AddBot(BotAgentViewModel botAgent)
        {
            try
            {
                var model = new SetupPluginViewModel(botAgent.Connection.RemoteAgent, null, AlgoTypes.Robot, botAgent.Connection.BotAgent.SetupContext);
                _shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
                model.Closed += AlgoSetupClosed;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }


        private void AlgoSetupClosed(SetupPluginViewModel setupModel, bool dlgResult)
        {
            setupModel.Closed -= AlgoSetupClosed;
            if (dlgResult)
            {
                var remoteAgent = (RemoteAlgoAgent)setupModel.Agent;
                var config = setupModel.GetConfig();
                if (setupModel.Setup.IsEditMode)
                {
                    remoteAgent.ChangeBotConfig(setupModel.Setup.InstanceId, config);
                }
                else
                {
                    remoteAgent.AddBot(setupModel.SelectedAccount.Key, config);
                }
            }
        }
    }
}
