using System.Linq;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class AlgoEnvironment
    {
        private VarList<AlgoAgentViewModel> _localAgentStub;


        public IShell Shell { get; }

        public TraderClientModel ClientModel { get; }

        public LocalAlgoAgent2 LocalAgent { get; }

        public BotAgentManager BotAgentManager { get; }

        public AlgoAgentViewModel LocalAgentVM { get; }

        public IVarList<BotAgentViewModel> BotAgents { get; }

        public IVarList<AlgoAgentViewModel> Agents { get; }

        public IObservableList<AlgoAgentViewModel> AgentsList { get; }

        public AlgoEnvironment(IShell shell, TraderClientModel clientModel, LocalAlgoAgent2 localAgent, BotAgentManager botAgentManager)
        {
            Shell = shell;
            ClientModel = clientModel;
            LocalAgent = localAgent;
            BotAgentManager = botAgentManager;

            LocalAgentVM = new AlgoAgentViewModel(LocalAgent, this);
            _localAgentStub = new VarList<AlgoAgentViewModel>();
            _localAgentStub.Add(LocalAgentVM);
            BotAgents = BotAgentManager.BotAgents.OrderBy((k, v) => k).Select(v => new BotAgentViewModel(v, this)).DisposeItems();
            Agents = VarCollection.CombineChained(_localAgentStub, BotAgents.Select(b => b.Agent));
            AgentsList = Agents.AsObservable();
        }
    }
}
