using System;
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

        private AlgoEnvironment _algoEnv;
        private BotManagerViewModel _botManager;


        public IObservableList<BotControlViewModel> LocalBots { get; }

        public IObservableList<BotAgentViewModel> BotAgents { get; }


        public BotListViewModel(AlgoEnvironment algoEnv, BotManagerViewModel botManager)
        {
            _algoEnv = algoEnv;
            _botManager = botManager;

            LocalBots = _botManager.Bots.AsObservable();

            BotAgents = _algoEnv.BotAgents.AsObservable();
        }


        public void AddBotAgent()
        {
            var viewModel = new BotAgentLoginDialogViewModel(_algoEnv.BotAgentManager);
            _algoEnv.Shell.ToolWndManager.ShowDialog(viewModel);
        }

        public void Drop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null && algoBot.Type == AlgoTypes.Robot)
            {
                _algoEnv.LocalAgentVM.OpenBotSetup(algoBot.Info);
            }
        }

        public bool CanDrop(object o)
        {
            return o is AlgoPluginViewModel;
        }
    }
}
