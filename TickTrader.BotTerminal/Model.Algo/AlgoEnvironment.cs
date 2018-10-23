using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Metadata;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class AlgoEnvironment
    {
        private VarList<AlgoAgentViewModel> _localAgentStub;


        public IShell Shell { get; }

        public LocalAlgoAgent LocalAgent { get; }

        public BotAgentManager BotAgentManager { get; }

        public AlgoAgentViewModel LocalAgentVM { get; }

        public IVarList<BotAgentViewModel> BotAgents { get; }

        public IVarList<AlgoAgentViewModel> Agents { get; }


        public AlgoEnvironment(IShell shell, LocalAlgoAgent localAgent, BotAgentManager botAgentManager)
        {
            Shell = shell;
            LocalAgent = localAgent;
            BotAgentManager = botAgentManager;

            ProfileResolver.Mappings = LocalAgent.Mappings;

            LocalAgentVM = new AlgoAgentViewModel(LocalAgent, this);
            _localAgentStub = new VarList<AlgoAgentViewModel>();
            _localAgentStub.Add(LocalAgentVM);
            BotAgents = BotAgentManager.BotAgents.OrderBy((k, v) => k).Select(v => new BotAgentViewModel(v, this));
            Agents = VarCollection.CombineChained(_localAgentStub, BotAgents.Select(b => b.Agent));
        }
    }
}
