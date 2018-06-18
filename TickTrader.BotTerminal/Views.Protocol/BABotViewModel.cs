using Caliburn.Micro;
using NLog;
using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    internal class BABotViewModel : PropertyChangedBase
    {
        private static readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        private BotModelInfo _entity;
        private RemoteAlgoAgent _remoteAgent;
        private IShell _shell;


        public string InstanceId => _entity.InstanceId;

        public AccountKey Account => _entity.Account;

        public BotStates State => _entity.State;


        public BABotViewModel(BotModelInfo entity, RemoteAlgoAgent remoteAgent, IShell shell)
        {
            _entity = entity;
            _remoteAgent = remoteAgent;
            _shell = shell;

            _remoteAgent.BotStateChanged += BotAgentOnBotStateChanged;
        }


        public void Start()
        {
            _remoteAgent.StartBot(_entity.InstanceId);
        }

        public void Stop()
        {
            _remoteAgent.StopBot(_entity.InstanceId);
        }

        public void Remove()
        {
            _remoteAgent.RemoveBot(_entity.InstanceId);
        }

        public void OpenSettings()
        {
            try
            {
                var model = new SetupPluginViewModel(_remoteAgent, _entity);
                _shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
                model.Closed += AlgoSetupClosed;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }


        private void BotAgentOnBotStateChanged(BotModelInfo bot)
        {
            if (bot.InstanceId == _entity.InstanceId)
            {
                NotifyOfPropertyChange(nameof(State));
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
