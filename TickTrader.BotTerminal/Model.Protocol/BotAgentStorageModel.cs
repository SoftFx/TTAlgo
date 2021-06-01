using Machinarium.Qnil;
using System.Linq;
using System.Runtime.Serialization;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    internal class BotAgentStorageModel : StorageModelBase<BotAgentStorageModel>
    {
        [DataMember(Name = "BotAgents")]
        private VarList<BotAgentStorageEntry> _botAgents;


        public VarList<BotAgentStorageEntry> BotAgents => _botAgents;


        public BotAgentStorageModel()
        {
            _botAgents = new VarList<BotAgentStorageEntry>();
        }


        public override BotAgentStorageModel Clone()
        {
            return new BotAgentStorageModel()
            {
                _botAgents = new VarList<BotAgentStorageEntry>(_botAgents.Values.Select(b => b.Clone())),
            };
        }


        public void Remove(string agentName)
        {
            var index = _botAgents.Values.IndexOf(b => b.Name == agentName);
            if (index != -1)
            {
                _botAgents.RemoveAt(index);
            }
        }

        public BotAgentStorageEntry Update(string name, string login, string password, string server, int port)
        {
            var index = _botAgents.Values.IndexOf(b => b.Name == name);
            if (index == -1)
            {
                _botAgents.Add(new BotAgentStorageEntry(name));
                index = _botAgents.Count - 1;
            }
            var botAgent = _botAgents[index];
            botAgent.Login = login;
            botAgent.Password = password;
            botAgent.ServerAddress = server;
            botAgent.Port = port;

            return botAgent;
        }

        public void UpdateConnect(string agentName, bool connect)
        {
            var index = _botAgents.Values.IndexOf(b => b.Name == agentName);
            if (index != -1)
            {
                _botAgents[index].Connect = connect;
            }
        }
    }
}
