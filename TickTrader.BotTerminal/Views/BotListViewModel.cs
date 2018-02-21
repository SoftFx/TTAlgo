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
        private TraderClientModel _clientModel;

        private ChartCollectionViewModel _charts;
        private ChartViewModel _botDefaultChart;
        private string _defaultSymbolName;


        public IObservableListSource<BotControlViewModel> LocalBots { get; }

        public IObservableListSource<BotAgentViewModel> BotAgents { get; }


        public BotListViewModel(IShell shell, BotAgentManager botAgentManager, TraderClientModel clientModel, ChartCollectionViewModel charts)
        {
            _shell = shell;
            _botAgentManager = botAgentManager;
            _clientModel = clientModel;
            _charts = charts;
            _clientModel.Disconnected += ()=>_botDefaultChart = null; 

            LocalBots = _shell.BotAggregator.BotControls.AsObservable();


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
            if (_botDefaultChart == null)
            {
                if (InitDefaultSymbolName())
                    _charts.Open(_defaultSymbolName);
                else
                    return;

                _botDefaultChart = _charts.ActiveItem;
            }
            else
                _charts.ActivateItem(_botDefaultChart);
                
            var algoBot = o as AlgoItemViewModel;
            if (algoBot != null)
            {
                var pluginType = algoBot.PluginItem.Descriptor.AlgoLogicType;
                if (pluginType == AlgoTypes.Robot)
                    _botDefaultChart.OpenPlugin(algoBot.PluginItem);                    
            }
        }

        public bool CanDrop(object o)
        {
            return o is AlgoItemViewModel;
        }

        private bool InitDefaultSymbolName()
        {
            var symbolsNames = _clientModel.Symbols.Snapshot.Values.Select((k) => k.Name).ToList();

            if (symbolsNames.Count == 0)
                return false;

            _defaultSymbolName = (symbolsNames.Contains("EURUSD")) ? "EURUSD" : symbolsNames[(new Random()).Next() % symbolsNames.Count];
            return true;
        }
    }
}
