using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal sealed class BotListViewModel : IDropHandler
    {
        private readonly AlgoEnvironment _algoEnv;


        public IObservableList<AlgoBotViewModel> LocalBots { get; }

        public AlgoAgentViewModel LocalAgent { get; }

        public IObservableList<BotAgentViewModel> BotAgents { get; }


        public BotListViewModel(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;

            LocalBots = _algoEnv.LocalAgentVM.Bots.AsObservable();
            LocalAgent = _algoEnv.LocalAgentVM;
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
    }
}
