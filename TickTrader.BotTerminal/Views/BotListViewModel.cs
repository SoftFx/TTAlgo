using Caliburn.Micro;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class BotListViewModel : PropertyChangedBase
    {
        private BotAgentManager _botAgentManager;
        private IShell _shell;


        public IObservableListSource<BotAgentViewModel> BotAgents { get; }


        public BotListViewModel(IShell shell, BotAgentManager botAgentManager)
        {
            _shell = shell;
            _botAgentManager = botAgentManager;

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
    }
}
