using System;
using System.Linq;
using Caliburn.Micro;
using TickTrader.Algo.Core.Metadata;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class BotListViewModel : PropertyChangedBase, IDropHandler
    {
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
                .Select(b => new BotAgentViewModel(b))
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
            var algoBot = o as AlgoItemViewModel;
            if (algoBot != null)
            {
                var pluginType = algoBot.PluginItem.Descriptor.Type;
                if (pluginType == AlgoTypes.Robot)
                    _botManager.OpenBotSetup(algoBot.PluginItem);
            }
        }

        public bool CanDrop(object o)
        {
            return o is AlgoItemViewModel;
        }
    }
}
