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


        public IObservableList<AlgoBotViewModel> LocalBots { get; }

        public IObservableList<BotAgentViewModel> BotAgents { get; }


        public BotListViewModel(AlgoEnvironment algoEnv)
        {
            _algoEnv = algoEnv;

            LocalBots = _algoEnv.LocalAgentVM.Bots.AsObservable();

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
            if (algoBot != null)
            {
                _algoEnv.LocalAgentVM.OpenBotSetup(null, algoBot.Info);
            }
            var algoPackage = o as AlgoPackageViewModel;
            if (algoPackage != null)
            {
                algoPackage.Agent.OpenDownloadPackageDialog(algoPackage.Key);
            }
        }

        public bool CanDrop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null && algoBot.Agent.Name == _algoEnv.LocalAgentVM.Name && algoBot.Type == AlgoTypes.Robot)
            {
                return true;
            }
            var algoPackage = o as AlgoPackageViewModel;
            if (algoPackage != null && algoPackage.CanDownloadPackage)
            {
                return true;
            }
            return false;
        }
    }
}
